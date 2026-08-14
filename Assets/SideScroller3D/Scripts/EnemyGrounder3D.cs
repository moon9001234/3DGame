using UnityEngine;

public class EnemyGrounder3D : MonoBehaviour
{
    [Header("\u8cbc\u5730\u4fee\u6b63")]
    [Tooltip("\u54ea\u4e9b Layer \u6703\u88ab\u8996\u70ba\u6575\u4eba\u53ef\u4ee5\u8cbc\u9f4a\u7684\u5730\u9762\u3002")]
    [SerializeField] private LayerMask groundMask;

    [Tooltip("\u5f9e\u6575\u4eba\u76ee\u524d\u4f4d\u7f6e\u5f80\u4e0a\u591a\u5c11\u8ddd\u96e2\u958b\u59cb\u5f80\u4e0b\u5075\u6e2c\u5730\u9762\u3002")]
    [SerializeField] private float rayStartHeight = 3f;

    [Tooltip("\u5f80\u4e0b\u641c\u5c0b\u5730\u9762\u7684\u6700\u5927\u8ddd\u96e2\u3002")]
    [SerializeField] private float snapDistance = 6f;

    [Tooltip("\u6575\u4eba\u5e95\u90e8\u8207\u5730\u9762\u4e4b\u9593\u4fdd\u7559\u7684\u5fae\u5c0f\u8ddd\u96e2\uff0c\u907f\u514d\u548c\u5730\u677f\u91cd\u758a\u3002")]
    [SerializeField] private float groundSkin = 0.02f;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6575\u4eba\u7684\u81ea\u52d5\u8cbc\u5730\u4e0d\u6703\u628a OneWayPlatform3D \u7576\u6210\u5730\u9762\uff0c\u907f\u514d\u5de1\u908f\u6642\u88ab\u55ae\u5411\u5e73\u53f0\u5438\u4e0a\u53bb\u3002")]
    [SerializeField] private bool ignoreOneWayPlatforms = true;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6bcf\u6b21 FixedUpdate \u90fd\u6703\u91cd\u65b0\u8cbc\u9f4a\u5730\u9762\u3002")]
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
