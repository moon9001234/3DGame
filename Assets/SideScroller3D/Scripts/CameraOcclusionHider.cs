using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class CameraOcclusionHider : MonoBehaviour
{
    private static readonly string[] DefaultExcludedNameKeywords =
    {
        "Road",
        "Sewer",
        "Hatch",
        "Ground",
        "Floor",
        "Sidewalk",
        "Pavement",
        "Trash",
        "Sign",
        "Lantern"
    };

    [SerializeField] private Transform target;
    [SerializeField] private LayerMask occluderMask;
    [SerializeField] private Transform[] occluderRoots;
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float viewportPadding = 0.04f;
    [SerializeField] private float refreshRenderersInterval = 0.5f;
    [SerializeField] private string[] excludedNameKeywords =
    {
        "Road",
        "Sewer",
        "Hatch",
        "Ground",
        "Floor",
        "Sidewalk",
        "Pavement",
        "Trash",
        "Sign",
        "Lantern"
    };

    private readonly List<Renderer> occluderRenderers = new List<Renderer>();
    private readonly HashSet<Renderer> hiddenRenderers = new HashSet<Renderer>();
    private readonly HashSet<Renderer> currentlyHiddenRenderers = new HashSet<Renderer>();
    private Camera cameraComponent;
    private float nextRefreshTime;

    public Transform Target
    {
        get => target;
        set => target = value;
    }

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        RefreshOccluderRenderers();
    }

    private void OnDisable()
    {
        RestoreHiddenRenderers();
    }

    private void LateUpdate()
    {
        if (target == null || cameraComponent == null)
        {
            RestoreHiddenRenderers();
            return;
        }

        if (Time.unscaledTime >= nextRefreshTime)
        {
            RefreshOccluderRenderers();
        }

        UpdateOcclusion();
    }

    private void RefreshOccluderRenderers()
    {
        nextRefreshTime = Time.unscaledTime + Mathf.Max(0.05f, refreshRenderersInterval);
        occluderRenderers.Clear();

        if (TryRefreshFromRoots())
        {
            return;
        }

        Renderer[] sceneRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        for (int i = 0; i < sceneRenderers.Length; i++)
        {
            Renderer sceneRenderer = sceneRenderers[i];
            if (!CanUseRendererBounds(sceneRenderer)
                || !IsLayerOccluder(sceneRenderer.gameObject.layer)
                || IsExcludedByName(sceneRenderer.transform))
            {
                continue;
            }

            occluderRenderers.Add(sceneRenderer);
        }
    }

    private void UpdateOcclusion()
    {
        currentlyHiddenRenderers.Clear();

        if (!TryGetTargetViewportRect(out Rect targetViewportRect, out float targetDistance))
        {
            RestoreHiddenRenderers();
            return;
        }

        for (int i = 0; i < occluderRenderers.Count; i++)
        {
            Renderer occluder = occluderRenderers[i];
            if (occluder == null || IsExcludedByName(occluder.transform))
            {
                continue;
            }

            if (ShouldHideRenderer(occluder, targetViewportRect, targetDistance))
            {
                currentlyHiddenRenderers.Add(occluder);
            }
        }

        foreach (Renderer rendererToRestore in hiddenRenderers)
        {
            if (rendererToRestore != null && !currentlyHiddenRenderers.Contains(rendererToRestore))
            {
                rendererToRestore.enabled = true;
            }
        }

        hiddenRenderers.Clear();
        foreach (Renderer rendererToHide in currentlyHiddenRenderers)
        {
            if (rendererToHide != null)
            {
                rendererToHide.enabled = false;
                hiddenRenderers.Add(rendererToHide);
            }
        }
    }

    private bool ShouldHideRenderer(Renderer occluder, Rect targetViewportRect, float targetDistance)
    {
        if (!TryGetValidRendererBounds(occluder, out Bounds bounds))
        {
            return false;
        }

        if (!TryGetForwardDistanceRange(bounds, out float nearestOccluderDistance, out _)
            || nearestOccluderDistance <= cameraComponent.nearClipPlane
            || nearestOccluderDistance >= targetDistance)
        {
            return false;
        }

        return TryGetViewportRect(bounds, out Rect viewportRect)
            && viewportRect.Overlaps(targetViewportRect);
    }

    private bool TryGetTargetViewportRect(out Rect targetViewportRect, out float targetDistance)
    {
        if (TryGetTargetBounds(out Bounds targetBounds))
        {
            if (!TryGetViewportRect(targetBounds, out targetViewportRect))
            {
                targetDistance = 0f;
                return false;
            }

            targetDistance = Vector3.Dot(targetBounds.center - transform.position, transform.forward);
            return targetDistance > cameraComponent.nearClipPlane;
        }

        Vector3 targetPoint = target.position + targetOffset;
        Vector3 targetViewport = cameraComponent.WorldToViewportPoint(targetPoint);
        if (targetViewport.z <= cameraComponent.nearClipPlane)
        {
            targetViewportRect = default;
            targetDistance = 0f;
            return false;
        }

        Vector2 targetViewportPoint = new Vector2(targetViewport.x, targetViewport.y);
        float padding = Mathf.Max(0.02f, viewportPadding);
        targetViewportRect = Rect.MinMaxRect(
            targetViewportPoint.x - padding,
            targetViewportPoint.y - padding,
            targetViewportPoint.x + padding,
            targetViewportPoint.y + padding);
        targetDistance = Vector3.Dot(targetPoint - transform.position, transform.forward);
        return true;
    }

    private bool TryGetTargetBounds(out Bounds targetBounds)
    {
        bool hasBounds = false;
        targetBounds = default;

        Collider[] targetColliders = target.GetComponentsInChildren<Collider>();
        for (int i = 0; i < targetColliders.Length; i++)
        {
            Collider targetCollider = targetColliders[i];
            if (!TryGetValidColliderBounds(targetCollider, out Bounds colliderBounds))
            {
                continue;
            }

            if (hasBounds)
            {
                targetBounds.Encapsulate(colliderBounds);
            }
            else
            {
                targetBounds = colliderBounds;
                hasBounds = true;
            }
        }

        if (hasBounds)
        {
            return true;
        }

        Renderer[] targetRenderers = target.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer targetRenderer = targetRenderers[i];
            if (!TryGetValidRendererBounds(targetRenderer, out Bounds rendererBounds))
            {
                continue;
            }

            if (hasBounds)
            {
                targetBounds.Encapsulate(rendererBounds);
            }
            else
            {
                targetBounds = rendererBounds;
                hasBounds = true;
            }
        }

        return hasBounds;
    }

    private bool TryGetViewportRect(Bounds bounds, out Rect rect)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };

        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;
        bool hasVisibleCorner = false;

        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 viewport = cameraComponent.WorldToViewportPoint(corners[i]);
            if (viewport.z <= cameraComponent.nearClipPlane)
            {
                continue;
            }

            hasVisibleCorner = true;
            minX = Mathf.Min(minX, viewport.x);
            minY = Mathf.Min(minY, viewport.y);
            maxX = Mathf.Max(maxX, viewport.x);
            maxY = Mathf.Max(maxY, viewport.y);
        }

        if (!hasVisibleCorner)
        {
            rect = default;
            return false;
        }

        float padding = Mathf.Max(0f, viewportPadding);
        rect = Rect.MinMaxRect(minX - padding, minY - padding, maxX + padding, maxY + padding);
        return rect.xMax >= 0f && rect.xMin <= 1f && rect.yMax >= 0f && rect.yMin <= 1f;
    }

    private bool TryGetForwardDistanceRange(Bounds bounds, out float minDistance, out float maxDistance)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };

        minDistance = float.PositiveInfinity;
        maxDistance = float.NegativeInfinity;
        for (int i = 0; i < corners.Length; i++)
        {
            float distance = Vector3.Dot(corners[i] - transform.position, transform.forward);
            minDistance = Mathf.Min(minDistance, distance);
            maxDistance = Mathf.Max(maxDistance, distance);
        }

        return !float.IsInfinity(minDistance) && !float.IsInfinity(maxDistance);
    }

    private bool TryRefreshFromRoots()
    {
        int rendererCountBeforeRoots = occluderRenderers.Count;
        bool hasRoot = false;
        if (occluderRoots != null)
        {
            for (int i = 0; i < occluderRoots.Length; i++)
            {
                hasRoot |= AddRenderersFromRoot(occluderRoots[i]);
            }
        }

        if (hasRoot)
        {
            return occluderRenderers.Count > rendererCountBeforeRoots;
        }

        AddNamedRootRenderers("Houses");
        AddNamedRootRenderers("StreetProps");
        return occluderRenderers.Count > rendererCountBeforeRoots;
    }

    private void AddNamedRootRenderers(string rootName)
    {
        GameObject root = GameObject.Find(rootName);
        AddRenderersFromRoot(root != null ? root.transform : null);
    }

    private bool AddRenderersFromRoot(Transform root)
    {
        if (root == null)
        {
            return false;
        }

        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (!CanUseRendererBounds(renderer) || IsExcludedByName(renderer.transform))
            {
                continue;
            }

            occluderRenderers.Add(renderer);
        }

        return true;
    }

    private bool IsLayerOccluder(int layer)
    {
        return (occluderMask.value & (1 << layer)) != 0;
    }

    private bool IsExcludedByName(Transform candidate)
    {
        if (candidate == null)
        {
            return false;
        }

        string[] keywords = excludedNameKeywords != null && excludedNameKeywords.Length > 0
            ? excludedNameKeywords
            : DefaultExcludedNameKeywords;

        Transform current = candidate;
        while (current != null)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                string keyword = keywords[i];
                if (!string.IsNullOrEmpty(keyword)
                    && current.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            current = current.parent;
        }

        return false;
    }

    private static bool TryGetValidRendererBounds(Renderer renderer, out Bounds bounds)
    {
        bounds = default;
        if (!CanUseRendererBounds(renderer))
        {
            return false;
        }

        Bounds candidateBounds = renderer.bounds;
        if (!IsValidBounds(candidateBounds))
        {
            return false;
        }

        bounds = candidateBounds;
        return true;
    }

    private static bool TryGetValidColliderBounds(Collider collider, out Bounds bounds)
    {
        bounds = default;
        if (collider == null || !collider.enabled || !collider.gameObject.activeInHierarchy)
        {
            return false;
        }

        Bounds candidateBounds = collider.bounds;
        if (!IsValidBounds(candidateBounds))
        {
            return false;
        }

        bounds = candidateBounds;
        return true;
    }

    private static bool CanUseRendererBounds(Renderer renderer)
    {
        return renderer != null
            && renderer.enabled
            && renderer.gameObject.activeInHierarchy
            && (renderer is MeshRenderer || renderer is SkinnedMeshRenderer);
    }

    private static bool IsValidBounds(Bounds bounds)
    {
        return IsFinite(bounds.center)
            && IsFinite(bounds.extents)
            && bounds.extents.x >= 0f
            && bounds.extents.y >= 0f
            && bounds.extents.z >= 0f
            && bounds.extents.x < 100000f
            && bounds.extents.y < 100000f
            && bounds.extents.z < 100000f;
    }

    private static bool IsFinite(Vector3 value)
    {
        return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private void RestoreHiddenRenderers()
    {
        foreach (Renderer hiddenRenderer in hiddenRenderers)
        {
            if (hiddenRenderer != null)
            {
                hiddenRenderer.enabled = true;
            }
        }

        hiddenRenderers.Clear();
    }
}
