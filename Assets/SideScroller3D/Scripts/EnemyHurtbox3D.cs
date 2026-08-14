using UnityEngine;

public class EnemyHurtbox3D : MonoBehaviour
{
    [Header("敵人受擊判定")]
    [Tooltip("受到玩家武器命中時，要扣血的 Health。留空時會自動尋找父物件上的 Health。")]
    [SerializeField] private Health targetHealth;

    [Tooltip("開啟後，編輯模式會依照敵人可見模型自動調整 Box Collider。關閉後可直接手動調 Collider。")]
    [SerializeField] private bool fitToVisibleModel = true;

    [Tooltip("開啟後，Play 模式也會持續依照模型邊界更新 Collider。通常關閉，避免手動調整被覆蓋。")]
    [SerializeField] private bool updateColliderDuringPlay;

    [Tooltip("自動依照模型建立受擊範圍時，額外外擴的尺寸。")]
    [SerializeField] private Vector3 modelBoundsPadding = new Vector3(0.12f, 0.16f, 0.12f);

    [Tooltip("自動建立受擊範圍時允許的最小大小，避免模型太小造成判定過窄。")]
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
