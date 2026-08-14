using UnityEngine;

[RequireComponent(typeof(Health))]
[DefaultExecutionOrder(1000)]
public class EnemyHealthBar3D : MonoBehaviour
{
    [Header("Health Bar")]
    [Tooltip("World-space offset from the enemy position.")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.55f, -0.08f);

    [Tooltip("Health bar width and height in world units.")]
    [SerializeField] private Vector2 size = new Vector2(1.35f, 0.12f);

    [Tooltip("Small cube depth used for the generated bar meshes.")]
    [SerializeField] private float depth = 0.04f;

    [Tooltip("Hide the health bar while the enemy is at full health.")]
    [SerializeField] private bool hideWhenFull;

    [Tooltip("Optional camera for the bar to face. Leave empty to use Camera.main.")]
    [SerializeField] private Camera facingCamera;

    private Health health;
    private Transform barRoot;
    private Transform fill;
    private Material backgroundMaterial;
    private Material fillMaterial;
    private Transform cachedCameraTransform;

    private void Awake()
    {
        health = GetComponent<Health>();
        CreateBar();
    }

    private void OnEnable()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        health.Changed += UpdateBar;
        UpdateBar(health.CurrentHealth, health.MaxHealth);
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Changed -= UpdateBar;
        }
    }

    private void LateUpdate()
    {
        if (barRoot == null)
        {
            return;
        }

        barRoot.position = transform.position + worldOffset;
        FaceCamera();
    }

    private void OnDestroy()
    {
        Destroy(backgroundMaterial);
        Destroy(fillMaterial);
    }

    private void CreateBar()
    {
        if (barRoot != null)
        {
            return;
        }

        GameObject root = new GameObject("Enemy Health Bar");
        root.transform.SetParent(transform, false);
        barRoot = root.transform;

        backgroundMaterial = CreateMaterial(new Color(0.04f, 0.04f, 0.04f, 1f));
        fillMaterial = CreateMaterial(new Color(0.9f, 0.08f, 0.06f, 1f));

        Transform background = CreateBlock("Background", barRoot, backgroundMaterial);
        background.localScale = new Vector3(size.x, size.y, depth);

        fill = CreateBlock("Fill", barRoot, fillMaterial);
        fill.localScale = new Vector3(size.x - 0.08f, size.y - 0.04f, depth + 0.01f);
        fill.localPosition = new Vector3(0f, 0f, -0.03f);
    }

    private void FaceCamera()
    {
        Transform cameraTransform = GetCameraTransform();
        barRoot.rotation = cameraTransform != null ? cameraTransform.rotation : Quaternion.identity;
    }

    private Transform GetCameraTransform()
    {
        if (facingCamera != null)
        {
            return facingCamera.transform;
        }

        if (cachedCameraTransform != null)
        {
            return cachedCameraTransform;
        }

        Camera mainCamera = Camera.main;
        cachedCameraTransform = mainCamera != null ? mainCamera.transform : null;
        return cachedCameraTransform;
    }

    private Transform CreateBlock(string name, Transform parent, Material material)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(parent, false);

        Collider blockCollider = block.GetComponent<Collider>();
        if (blockCollider != null)
        {
            Destroy(blockCollider);
        }

        block.GetComponent<Renderer>().sharedMaterial = material;
        return block.transform;
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private void UpdateBar(int current, int max)
    {
        if (barRoot == null || fill == null)
        {
            return;
        }

        float ratio = max <= 0 ? 0f : Mathf.Clamp01((float)current / max);
        float fillWidth = Mathf.Max(0.01f, size.x - 0.08f);
        fill.localScale = new Vector3(fillWidth * ratio, size.y - 0.04f, depth + 0.01f);
        fill.localPosition = new Vector3(-(fillWidth * (1f - ratio)) * 0.5f, 0f, -0.03f);
        barRoot.gameObject.SetActive(!hideWhenFull || current < max);
    }
}
