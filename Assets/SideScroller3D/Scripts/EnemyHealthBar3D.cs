using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
[DefaultExecutionOrder(1000)]
public class EnemyHealthBar3D : MonoBehaviour
{
    [Header("Health Bar")]
    [Tooltip("World-space offset from the enemy position.")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.55f, -0.08f);

    [Tooltip("Legacy health bar width and current bar height in world units. Width is now driven by HP cells.")]
    [SerializeField] private Vector2 size = new Vector2(1.35f, 0.12f);

    [Tooltip("Small cube depth used for the generated bar meshes.")]
    [SerializeField] private float depth = 0.04f;

    [Tooltip("Fixed width for each HP cell in world units.")]
    [SerializeField] private float cellWidth = 0.24f;

    [Tooltip("Gap between each HP cell in world units.")]
    [SerializeField] private float cellSpacing = 0.025f;

    [Tooltip("Hide the health bar while the enemy is at full health.")]
    [SerializeField] private bool hideWhenFull;

    [Tooltip("Optional camera for the bar to face. Leave empty to use Camera.main.")]
    [SerializeField] private Camera facingCamera;

    private Health health;
    private Transform barRoot;
    private Transform background;
    private Transform cellsRoot;
    private Material backgroundMaterial;
    private Material fillMaterial;
    private Material emptyMaterial;
    private Transform cachedCameraTransform;
    private readonly List<Renderer> healthCells = new List<Renderer>();
    private int cachedMaxHealth = -1;

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
        Destroy(emptyMaterial);
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
        emptyMaterial = CreateMaterial(new Color(0.18f, 0.03f, 0.03f, 1f));

        background = CreateBlock("Background", barRoot, backgroundMaterial);
        background.localScale = new Vector3(size.x, size.y, depth);

        GameObject cellsObject = new GameObject("Health Cells");
        cellsObject.transform.SetParent(barRoot, false);
        cellsRoot = cellsObject.transform;
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
        if (barRoot == null)
        {
            return;
        }

        int cellCount = Mathf.Max(0, max);
        ResizeBar(cellCount);
        if (cachedMaxHealth != cellCount || healthCells.Count != cellCount)
        {
            RebuildHealthCells(cellCount);
        }

        int filledCount = Mathf.Clamp(current, 0, cellCount);
        for (int i = 0; i < healthCells.Count; i++)
        {
            if (healthCells[i] != null)
            {
                healthCells[i].sharedMaterial = i < filledCount ? fillMaterial : emptyMaterial;
            }
        }

        barRoot.gameObject.SetActive(!hideWhenFull || current < max);
    }

    private void ResizeBar(int cellCount)
    {
        float width = GetTotalBarWidth(cellCount);
        if (background != null)
        {
            background.localScale = new Vector3(width, size.y, depth);
        }
    }

    private void RebuildHealthCells(int cellCount)
    {
        cachedMaxHealth = cellCount;
        healthCells.Clear();
        if (cellsRoot == null)
        {
            return;
        }

        for (int i = cellsRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(cellsRoot.GetChild(i).gameObject);
        }

        if (cellCount <= 0)
        {
            return;
        }

        float innerHeight = Mathf.Max(0.01f, size.y - 0.04f);
        float resolvedCellWidth = Mathf.Max(0.01f, cellWidth);
        float spacing = Mathf.Max(0f, cellSpacing);
        float innerWidth = GetInnerBarWidth(cellCount);
        float left = -innerWidth * 0.5f + resolvedCellWidth * 0.5f;

        for (int i = 0; i < cellCount; i++)
        {
            Transform cell = CreateBlock("HP Cell " + (i + 1), cellsRoot, fillMaterial);
            cell.localScale = new Vector3(resolvedCellWidth, innerHeight, depth + 0.01f);
            cell.localPosition = new Vector3(left + i * (resolvedCellWidth + spacing), 0f, -0.03f);
            healthCells.Add(cell.GetComponent<Renderer>());
        }
    }

    private float GetTotalBarWidth(int cellCount)
    {
        return GetInnerBarWidth(cellCount) + 0.08f;
    }

    private float GetInnerBarWidth(int cellCount)
    {
        if (cellCount <= 0)
        {
            return 0.01f;
        }

        return Mathf.Max(0.01f, Mathf.Max(0.01f, cellWidth) * cellCount + Mathf.Max(0f, cellSpacing) * (cellCount - 1));
    }
}
