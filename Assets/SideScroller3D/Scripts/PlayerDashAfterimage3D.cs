using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerMotor3D))]
public class PlayerDashAfterimage3D : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float spawnInterval = 0.035f;
    [SerializeField] private float lifetime = 0.18f;
    [SerializeField] private Color afterimageColor = new Color(0.25f, 0.55f, 1f, 0.45f);
    [SerializeField] private bool includeMeshRenderers = true;
    [SerializeField] private bool includeInactiveRenderers;

    private readonly List<RendererSnapshot> renderers = new List<RendererSnapshot>();
    private PlayerMotor3D motor;
    private Material afterimageMaterial;
    private float nextSpawnTime;
    private bool wasDashing;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor3D>();
        CacheRenderers();
        EnsureMaterial();
    }

    private void OnEnable()
    {
        nextSpawnTime = 0f;
        wasDashing = false;
    }

    private void OnDestroy()
    {
        if (afterimageMaterial != null)
        {
            Destroy(afterimageMaterial);
        }
    }

    private void LateUpdate()
    {
        if (motor == null)
        {
            return;
        }

        bool isDashing = motor.IsDashing;
        if (!isDashing)
        {
            wasDashing = false;
            return;
        }

        if (!wasDashing)
        {
            CacheRenderers();
            nextSpawnTime = 0f;
            wasDashing = true;
        }

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        SpawnAfterimage();
        nextSpawnTime = Time.time + Mathf.Max(0.01f, spawnInterval);
    }

    public void RefreshRenderers()
    {
        CacheRenderers();
    }

    public void Configure(
        Transform newVisualRoot,
        float newSpawnInterval,
        float newLifetime,
        Color newAfterimageColor,
        bool newIncludeMeshRenderers,
        bool newIncludeInactiveRenderers)
    {
        visualRoot = newVisualRoot;
        spawnInterval = Mathf.Max(0.01f, newSpawnInterval);
        lifetime = Mathf.Max(0.01f, newLifetime);
        afterimageColor = newAfterimageColor;
        includeMeshRenderers = newIncludeMeshRenderers;
        includeInactiveRenderers = newIncludeInactiveRenderers;

        if (afterimageMaterial != null)
        {
            SetMaterialColor(afterimageMaterial, afterimageColor);
        }

        CacheRenderers();
    }

    private void CacheRenderers()
    {
        renderers.Clear();
        Transform root = visualRoot != null ? visualRoot : transform;
        SkinnedMeshRenderer[] skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactiveRenderers);
        for (int i = 0; i < skinnedRenderers.Length; i++)
        {
            if (skinnedRenderers[i] != null && skinnedRenderers[i].enabled)
            {
                renderers.Add(new RendererSnapshot(skinnedRenderers[i]));
            }
        }

        if (!includeMeshRenderers)
        {
            return;
        }

        MeshRenderer[] meshRenderers = root.GetComponentsInChildren<MeshRenderer>(includeInactiveRenderers);
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            MeshFilter meshFilter = meshRenderers[i] != null ? meshRenderers[i].GetComponent<MeshFilter>() : null;
            if (meshRenderers[i] != null && meshRenderers[i].enabled && meshFilter != null && meshFilter.sharedMesh != null)
            {
                renderers.Add(new RendererSnapshot(meshRenderers[i], meshFilter));
            }
        }
    }

    private void SpawnAfterimage()
    {
        if (renderers.Count == 0)
        {
            CacheRenderers();
        }

        EnsureMaterial();
        if (afterimageMaterial == null)
        {
            return;
        }

        GameObject root = new GameObject("Dash_Afterimage");
        root.transform.SetPositionAndRotation(transform.position, transform.rotation);
        int partCount = 0;

        for (int i = 0; i < renderers.Count; i++)
        {
            RendererSnapshot snapshot = renderers[i];
            if (!snapshot.IsValid)
            {
                continue;
            }

            Mesh mesh = snapshot.CreateMeshSnapshot();
            if (mesh == null)
            {
                continue;
            }

            GameObject part = new GameObject(snapshot.Name + "_Afterimage");
            part.transform.SetParent(root.transform, true);
            part.transform.SetPositionAndRotation(snapshot.Transform.position, snapshot.Transform.rotation);
            part.transform.localScale = snapshot.Transform.lossyScale;

            MeshFilter meshFilter = part.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = mesh;

            MeshRenderer meshRenderer = part.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = afterimageMaterial;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            partCount++;
        }

        if (partCount == 0)
        {
            Destroy(root);
            return;
        }

        DashAfterimageFade fade = root.AddComponent<DashAfterimageFade>();
        fade.Initialize(afterimageColor, lifetime);
    }

    private void EnsureMaterial()
    {
        if (afterimageMaterial != null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            return;
        }

        afterimageMaterial = new Material(shader);
        afterimageMaterial.name = "Dash Afterimage Material";
        ApplyTransparentSettings(afterimageMaterial);
        SetMaterialColor(afterimageMaterial, afterimageColor);
    }

    private static void ApplyTransparentSettings(Material material)
    {
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", 0f);
        }

        if (material.HasProperty("_SrcBlend"))
        {
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        }

        if (material.HasProperty("_DstBlend"))
        {
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }

        if (material.HasProperty("_ZWrite"))
        {
            material.SetFloat("_ZWrite", 0f);
        }

        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.EnableKeyword("_ALPHABLEND_ON");
    }

    private static void SetMaterialColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }

    private class RendererSnapshot
    {
        private readonly SkinnedMeshRenderer skinnedRenderer;
        private readonly MeshFilter meshFilter;

        public RendererSnapshot(SkinnedMeshRenderer renderer)
        {
            skinnedRenderer = renderer;
            Transform = renderer.transform;
            Name = renderer.name;
        }

        public RendererSnapshot(MeshRenderer renderer, MeshFilter filter)
        {
            meshFilter = filter;
            Transform = renderer.transform;
            Name = renderer.name;
        }

        public Transform Transform { get; }
        public string Name { get; }
        public bool IsValid => Transform != null && (skinnedRenderer != null || (meshFilter != null && meshFilter.sharedMesh != null));

        public Mesh CreateMeshSnapshot()
        {
            Mesh mesh = new Mesh();
            if (skinnedRenderer != null)
            {
                skinnedRenderer.BakeMesh(mesh);
                return mesh;
            }

            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                mesh = Object.Instantiate(meshFilter.sharedMesh);
                return mesh;
            }

            Object.Destroy(mesh);
            return null;
        }
    }

    private class DashAfterimageFade : MonoBehaviour
    {
        private readonly List<Mesh> meshes = new List<Mesh>();
        private Material material;
        private Color startColor;
        private float lifetime = 0.18f;
        private float startedAt;

        public void Initialize(Color color, float duration)
        {
            startColor = color;
            lifetime = Mathf.Max(0.01f, duration);
            startedAt = Time.time;

            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers.Length == 0 || meshRenderers[0].sharedMaterial == null)
            {
                Destroy(gameObject);
                return;
            }

            material = new Material(meshRenderers[0].sharedMaterial);
            for (int i = 0; i < meshRenderers.Length; i++)
            {
                meshRenderers[i].sharedMaterial = material;
            }

            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
            for (int i = 0; i < meshFilters.Length; i++)
            {
                if (meshFilters[i].sharedMesh != null)
                {
                    meshes.Add(meshFilters[i].sharedMesh);
                }
            }
        }

        private void Update()
        {
            float progress = Mathf.Clamp01((Time.time - startedAt) / lifetime);
            Color color = startColor;
            color.a *= 1f - progress;
            SetMaterialColor(material, color);

            if (progress >= 1f)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    if (meshes[i] != null)
                    {
                        Destroy(meshes[i]);
                    }
                }

                if (material != null)
                {
                    Destroy(material);
                }

                Destroy(gameObject);
            }
        }
    }
}
