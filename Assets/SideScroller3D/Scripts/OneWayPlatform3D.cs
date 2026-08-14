using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class OneWayPlatform3D : MonoBehaviour
{
    [Header("One Way Platform")]
    [Tooltip("How long player collision is ignored when moving upward through this platform.")]
    [SerializeField] private float upwardPassThroughSeconds = 0.25f;

    [Tooltip("How long player collision is ignored when dropping downward through this platform.")]
    [SerializeField] private float dropThroughSeconds = 0.45f;

    [Tooltip("Vertical tolerance used to decide whether the player is standing on the top side.")]
    [SerializeField] private float topSkin = 0.08f;

    [Tooltip("Minimum upward velocity needed to keep passing through the platform from below.")]
    [SerializeField] private float upwardVelocityThreshold = 0.02f;

    private readonly List<IgnoredCollider> ignoredColliders = new List<IgnoredCollider>();
    private Collider platformCollider;

    public float UpwardPassThroughSeconds => upwardPassThroughSeconds;
    public float DropThroughSeconds => dropThroughSeconds;

    private void Reset()
    {
        EnsureGroundLayer();
    }

    private void Awake()
    {
        EnsureCollider();
        EnsureGroundLayer();
    }

    private void OnValidate()
    {
        EnsureCollider();
        EnsureGroundLayer();
        if (platformCollider != null)
        {
            platformCollider.isTrigger = false;
        }
    }

    private void FixedUpdate()
    {
        RestoreExpiredCollisions();
    }

    public bool CanStandOn(Collider actorCollider)
    {
        EnsureCollider();
        if (platformCollider == null || actorCollider == null)
        {
            return false;
        }

        return actorCollider.bounds.min.y >= platformCollider.bounds.max.y - Mathf.Max(0f, topSkin);
    }

    public bool ShouldPassThroughFromBelow(Collider actorCollider, float verticalVelocity)
    {
        EnsureCollider();
        if (platformCollider == null || actorCollider == null)
        {
            return false;
        }

        bool actorIsBelowTop = actorCollider.bounds.center.y < platformCollider.bounds.max.y - Mathf.Max(0f, topSkin);
        bool actorIsMovingUp = verticalVelocity > upwardVelocityThreshold;
        return actorIsBelowTop || actorIsMovingUp;
    }

    public bool ShouldPreparePassThroughFromBelow(Collider actorCollider, float verticalVelocity)
    {
        EnsureCollider();
        if (platformCollider == null || actorCollider == null)
        {
            return false;
        }

        bool actorIsNearOrBelowTop = actorCollider.bounds.min.y < platformCollider.bounds.max.y + Mathf.Max(0f, topSkin);
        bool actorIsMovingUp = verticalVelocity > upwardVelocityThreshold;
        return actorIsNearOrBelowTop && actorIsMovingUp;
    }

    public void IgnoreColliders(Collider[] actorColliders, float seconds)
    {
        IgnoreColliders(actorColliders, seconds, false);
    }

    public void IgnoreCollidersUntilAbove(Collider[] actorColliders, float seconds)
    {
        IgnoreColliders(actorColliders, seconds, RestoreMode.WhenStandable);
    }

    public void IgnoreCollidersUntilBelow(Collider[] actorColliders, float seconds)
    {
        IgnoreColliders(actorColliders, seconds, RestoreMode.WhenBelow);
    }

    private void IgnoreColliders(Collider[] actorColliders, float seconds, bool restoreWhenStandable)
    {
        IgnoreColliders(actorColliders, seconds, restoreWhenStandable ? RestoreMode.WhenStandable : RestoreMode.TimerOnly);
    }

    private void IgnoreColliders(Collider[] actorColliders, float seconds, RestoreMode restoreMode)
    {
        EnsureCollider();
        if (platformCollider == null || actorColliders == null)
        {
            return;
        }

        float restoreTime = Time.time + Mathf.Max(0.02f, seconds);
        foreach (Collider actorCollider in actorColliders)
        {
            if (actorCollider == null || actorCollider.isTrigger)
            {
                continue;
            }

            IgnoreCollider(actorCollider, restoreTime, restoreMode);
        }
    }

    public bool IsIgnoringAny(Collider[] actorColliders)
    {
        RestoreConditionalCollisions(actorColliders);

        if (actorColliders == null)
        {
            return false;
        }

        foreach (Collider actorCollider in actorColliders)
        {
            if (IsIgnoring(actorCollider))
            {
                return true;
            }
        }

        return false;
    }

    private void RestoreConditionalCollisions(Collider[] actorColliders)
    {
        EnsureCollider();
        if (platformCollider == null || actorColliders == null)
        {
            return;
        }

        for (int i = ignoredColliders.Count - 1; i >= 0; i--)
        {
            IgnoredCollider ignoredCollider = ignoredColliders[i];
            if (ignoredCollider.RestoreMode == RestoreMode.TimerOnly || !ContainsCollider(actorColliders, ignoredCollider.Collider))
            {
                continue;
            }

            if (!CanRestoreByMode(ignoredCollider.Collider, ignoredCollider.RestoreMode))
            {
                continue;
            }

            if (ignoredCollider.Collider != null)
            {
                Physics.IgnoreCollision(platformCollider, ignoredCollider.Collider, false);
            }

            ignoredColliders.RemoveAt(i);
        }
    }

    private void IgnoreCollider(Collider actorCollider, float restoreTime)
    {
        IgnoreCollider(actorCollider, restoreTime, RestoreMode.TimerOnly);
    }

    private void IgnoreCollider(Collider actorCollider, float restoreTime, RestoreMode restoreMode)
    {
        for (int i = 0; i < ignoredColliders.Count; i++)
        {
            if (ignoredColliders[i].Collider == actorCollider)
            {
                ignoredColliders[i] = new IgnoredCollider(actorCollider, restoreTime, restoreMode);
                return;
            }
        }

        Physics.IgnoreCollision(platformCollider, actorCollider, true);
        ignoredColliders.Add(new IgnoredCollider(actorCollider, restoreTime, restoreMode));
    }

    private bool IsIgnoring(Collider actorCollider)
    {
        if (actorCollider == null)
        {
            return false;
        }

        foreach (IgnoredCollider ignoredCollider in ignoredColliders)
        {
            if (ignoredCollider.Collider == actorCollider)
            {
                return true;
            }
        }

        return false;
    }

    private void RestoreExpiredCollisions()
    {
        EnsureCollider();
        if (platformCollider == null)
        {
            ignoredColliders.Clear();
            return;
        }

        for (int i = ignoredColliders.Count - 1; i >= 0; i--)
        {
            IgnoredCollider ignoredCollider = ignoredColliders[i];
            bool canRestoreByMode = CanRestoreByMode(ignoredCollider.Collider, ignoredCollider.RestoreMode);
            if (ignoredCollider.RestoreMode != RestoreMode.TimerOnly && !canRestoreByMode)
            {
                continue;
            }

            if (!canRestoreByMode && Time.time <= ignoredCollider.RestoreTime)
            {
                continue;
            }

            if (ignoredCollider.Collider != null)
            {
                Physics.IgnoreCollision(platformCollider, ignoredCollider.Collider, false);
            }

            ignoredColliders.RemoveAt(i);
        }
    }

    private bool CanRestoreWhenActorIsAbove(Collider actorCollider)
    {
        if (actorCollider == null || platformCollider == null)
        {
            return false;
        }

        Rigidbody attachedBody = actorCollider.attachedRigidbody;
        bool actorIsNotMovingUp = attachedBody == null || attachedBody.linearVelocity.y <= upwardVelocityThreshold;
        return actorIsNotMovingUp && CanStandOn(actorCollider);
    }

    private bool CanRestoreWhenActorIsBelow(Collider actorCollider)
    {
        if (actorCollider == null || platformCollider == null)
        {
            return false;
        }

        return actorCollider.bounds.max.y <= platformCollider.bounds.min.y + Mathf.Max(0f, topSkin);
    }

    private bool CanRestoreByMode(Collider actorCollider, RestoreMode restoreMode)
    {
        switch (restoreMode)
        {
            case RestoreMode.WhenStandable:
                return CanRestoreWhenActorIsAbove(actorCollider);
            case RestoreMode.WhenBelow:
                return CanRestoreWhenActorIsBelow(actorCollider);
            default:
                return false;
        }
    }

    private static bool ContainsCollider(Collider[] colliders, Collider target)
    {
        if (target == null)
        {
            return false;
        }

        foreach (Collider collider in colliders)
        {
            if (collider == target)
            {
                return true;
            }
        }

        return false;
    }

    private void EnsureCollider()
    {
        if (platformCollider == null)
        {
            platformCollider = GetComponent<Collider>();
        }
    }

    private void EnsureGroundLayer()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        if (groundLayer >= 0 && gameObject.layer != groundLayer)
        {
            gameObject.layer = groundLayer;
        }
    }

    private enum RestoreMode
    {
        TimerOnly,
        WhenStandable,
        WhenBelow
    }

    private struct IgnoredCollider
    {
        public IgnoredCollider(Collider collider, float restoreTime, RestoreMode restoreMode)
        {
            Collider = collider;
            RestoreTime = restoreTime;
            RestoreMode = restoreMode;
        }

        public Collider Collider { get; }
        public float RestoreTime { get; }
        public RestoreMode RestoreMode { get; }
    }
}
