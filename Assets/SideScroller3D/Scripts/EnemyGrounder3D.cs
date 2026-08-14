using UnityEngine;

public class EnemyGrounder3D : MonoBehaviour
{
    [Header("貼地修正")]
    [Tooltip("哪些 Layer 會被視為敵人可以貼齊的地面。")]
    [SerializeField] private LayerMask groundMask;

    [Tooltip("從敵人目前位置往上多少距離開始往下偵測地面。")]
    [SerializeField] private float rayStartHeight = 3f;

    [Tooltip("往下搜尋地面的最大距離。")]
    [SerializeField] private float snapDistance = 6f;

    [Tooltip("敵人底部與地面之間保留的微小距離，避免和地板重疊。")]
    [SerializeField] private float groundSkin = 0.02f;

    [Tooltip("開啟後，敵人的自動貼地不會把 OneWayPlatform3D 當成地面，避免巡邏時被單向平台吸上去。")]
    [SerializeField] private bool ignoreOneWayPlatforms = true;

    [Tooltip("開啟後，每次 FixedUpdate 都會重新貼齊地面。")]
    [SerializeField] private bool snapEveryFixedUpdate = true;

    private readonly RaycastHit[] groundHits = new RaycastHit[12];
    private Rigidbody body;
    private Collider bodyCollider;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        EnsureGroundMask();
        EnsureBodyCollider();
    }

    private void FixedUpdate()
    {
        if (snapEveryFixedUpdate)
        {
            SnapToGround();
        }
    }

    public void SnapToGround()
    {
        EnsureGroundMask();
        EnsureBodyCollider();

        if (bodyCollider == null)
        {
            return;
        }

        Vector3 origin = transform.position + Vector3.up * rayStartHeight;
        float rayDistance = rayStartHeight + snapDistance;
        int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, groundHits, rayDistance, groundMask, QueryTriggerInteraction.Ignore);
        if (!TryGetNearestGround(hitCount, out RaycastHit groundHit))
        {
            return;
        }

        Bounds bounds = bodyCollider.bounds;
        float desiredBottomY = groundHit.point.y + groundSkin;
        float yDelta = desiredBottomY - bounds.min.y;
        if (Mathf.Abs(yDelta) < 0.001f || Mathf.Abs(yDelta) > snapDistance)
        {
            return;
        }

        Vector3 position = body != null ? body.position : transform.position;
        position.y += yDelta;

        if (body != null)
        {
            body.position = position;
            Vector3 velocity = body.linearVelocity;
            if (Mathf.Abs(yDelta) > 0.001f)
            {
                velocity.y = 0f;
            }

            body.linearVelocity = velocity;
        }
        else
        {
            transform.position = position;
        }
    }

    private bool TryGetNearestGround(int hitCount, out RaycastHit nearestHit)
    {
        nearestHit = default;
        bool hasHit = false;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = groundHits[i];
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (ignoreOneWayPlatforms && hit.collider.GetComponentInParent<OneWayPlatform3D>() != null)
            {
                continue;
            }

            if (hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestHit = hit;
                hasHit = true;
            }
        }

        return hasHit;
    }

    private void EnsureBodyCollider()
    {
        if (bodyCollider != null && bodyCollider.enabled && !bodyCollider.isTrigger)
        {
            return;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider candidate in colliders)
        {
            if (candidate != null && candidate.enabled && !candidate.isTrigger)
            {
                bodyCollider = candidate;
                return;
            }
        }
    }

    private void EnsureGroundMask()
    {
        if (groundMask.value != 0)
        {
            return;
        }

        int groundLayer = LayerMask.NameToLayer("Ground");
        groundMask = groundLayer >= 0 ? LayerMask.GetMask("Ground") : Physics.DefaultRaycastLayers;
    }
}
