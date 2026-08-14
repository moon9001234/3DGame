using UnityEngine;

public class EnemyHurtbox3D : MonoBehaviour
{
    [Header("\u6575\u4eba\u53d7\u64ca\u5224\u5b9a")]
    [Tooltip("\u53d7\u5230\u73a9\u5bb6\u6b66\u5668\u547d\u4e2d\u6642\uff0c\u8981\u6263\u8840\u7684 Health\u3002\u7559\u7a7a\u6642\u6703\u81ea\u52d5\u5c0b\u627e\u7236\u7269\u4ef6\u4e0a\u7684 Health\u3002")]
    [SerializeField] private Health targetHealth;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u7de8\u8f2f\u6a21\u5f0f\u6703\u4f9d\u7167\u6575\u4eba\u53ef\u898b\u6a21\u578b\u81ea\u52d5\u8abf\u6574 Box Collider\u3002\u95dc\u9589\u5f8c\u53ef\u76f4\u63a5\u624b\u52d5\u8abf Collider\u3002")]
    [SerializeField] private bool fitToVisibleModel = true;

    [Tooltip("\u958b\u555f\u5f8c\uff0cPlay \u6a21\u5f0f\u4e5f\u6703\u6301\u7e8c\u4f9d\u7167\u6a21\u578b\u908a\u754c\u66f4\u65b0 Collider\u3002\u901a\u5e38\u95dc\u9589\uff0c\u907f\u514d\u624b\u52d5\u8abf\u6574\u88ab\u8986\u84cb\u3002")]
    [SerializeField] private bool updateColliderDuringPlay;

    [Tooltip("\u81ea\u52d5\u4f9d\u7167\u6a21\u578b\u5efa\u7acb\u53d7\u64ca\u7bc4\u570d\u6642\uff0c\u984d\u5916\u5916\u64f4\u7684\u5c3a\u5bf8\u3002")]
    [SerializeField] private Vector3 modelBoundsPadding = new Vector3(0.12f, 0.16f, 0.12f);

    [Tooltip("\u81ea\u52d5\u5efa\u7acb\u53d7\u64ca\u7bc4\u570d\u6642\u5141\u8a31\u7684\u6700\u5c0f\u5927\u5c0f\uff0c\u907f\u514d\u6a21\u578b\u592a\u5c0f\u9020\u6210\u5224\u5b9a\u904e\u7a84\u3002")]
    [SerializeField] private Vector3 minimumHurtboxSize = new Vector3(0.7f, 1.2f, 0.7f);

    private BoxCollider hurtbox;

    public Health TargetHealth
    {
        get
        {
            if (targetHealth == null)
            {
                targetHealth = GetComponentInParent<Health>();
            }

            return targetHealth;
        }
    }

    public BoxCollider Collider
    {
        get
        {
            EnsureParts();
            return hurtbox;
        }
    }

    private void Awake()
    {
        EnsureParts();
    }

    private void LateUpdate()
    {
        FitToModelBounds();
    }

    public void RefreshParts()
    {
        EnsureParts();
    }

    private void EnsureParts()
    {
        if (hurtbox == null)
        {
            hurtbox = GetComponent<BoxCollider>();
            if (hurtbox == null)
            {
                hurtbox = gameObject.AddComponent<BoxCollider>();
            }
        }

        hurtbox.isTrigger = true;

        FitToModelBounds();
        RemoveLegacyVisual();
    }

    private void FitToModelBounds()
    {
        if (!ShouldFitToModelBounds() || hurtbox == null)
        {
            return;
        }

        Health health = TargetHealth;
        if (health == null)
        {
            return;
        }

        Renderer[] renderers = health.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (Renderer renderer in renderers)
        {
            if (!IsUsableModelRenderer(renderer))
            {
                continue;
            }

            Bounds worldBounds = renderer.bounds;
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, worldBounds.min);
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(worldBounds.min.x, worldBounds.min.y, worldBounds.max.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(worldBounds.min.x, worldBounds.max.y, worldBounds.min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(worldBounds.min.x, worldBounds.max.y, worldBounds.max.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(worldBounds.max.x, worldBounds.min.y, worldBounds.min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(worldBounds.max.x, worldBounds.min.y, worldBounds.max.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(worldBounds.max.x, worldBounds.max.y, worldBounds.min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, worldBounds.max);
        }

        if (!hasBounds)
        {
            hurtbox.size = minimumHurtboxSize;
            hurtbox.center = Vector3.up * (minimumHurtboxSize.y * 0.5f);
            return;
        }

        Vector3 size = localBounds.size + modelBoundsPadding;
        size = new Vector3(
            Mathf.Max(size.x, minimumHurtboxSize.x),
            Mathf.Max(size.y, minimumHurtboxSize.y),
            Mathf.Max(size.z, minimumHurtboxSize.z));

        hurtbox.center = localBounds.center;
        hurtbox.size = size;
    }

    private bool ShouldFitToModelBounds()
    {
        return fitToVisibleModel && (!Application.isPlaying || updateColliderDuringPlay);
    }

    private bool IsUsableModelRenderer(Renderer renderer)
    {
        if (renderer == null || !renderer.enabled || !renderer.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (renderer.GetComponentInParent<ParticleSystem>() != null)
        {
            return false;
        }

        string objectName = renderer.gameObject.name;
        return !objectName.Contains("Health")
            && !objectName.Contains("Hit")
            && !objectName.Contains("FX")
            && !objectName.Contains("EF");
    }

    private void EncapsulateLocalPoint(ref Bounds bounds, ref bool hasBounds, Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        if (!hasBounds)
        {
            bounds = new Bounds(localPoint, Vector3.zero);
            hasBounds = true;
        }
        else
        {
            bounds.Encapsulate(localPoint);
        }
    }

    private void RemoveLegacyVisual()
    {
        Transform visual = transform.Find("Enemy_Hurtbox_Visual");
        if (visual == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(visual.gameObject);
        }
        else
        {
            DestroyImmediate(visual.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0.08f, 0.04f, 0.9f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
