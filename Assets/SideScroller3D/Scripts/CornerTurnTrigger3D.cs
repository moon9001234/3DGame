using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class CornerTurnTrigger3D : MonoBehaviour
{
    private enum TurnDirection
    {
        Right,
        Left
    }

    [Header("\u8f49\u89d2\u8a2d\u5b9a")]
    [Tooltip("\u73a9\u5bb6\u7b2c\u4e00\u6b21\u8d70\u5230\u89f8\u767c\u5668\u4e2d\u5fc3\u6642\u8981\u5f80\u53f3\u8f49\u6216\u5f80\u5de6\u8f49\u3002")]
    [SerializeField] private TurnDirection firstTurnDirection = TurnDirection.Left;

    [Tooltip("\u8f49\u5411\u89d2\u5ea6\u3002\u4e00\u822c\u8857\u89d2\u4f7f\u7528 90 \u5ea6\u3002")]
    [SerializeField] private float turnDegrees = 90f;

    [Tooltip("\u651d\u5f71\u6a5f\u65cb\u8f49\u6642\u9593\uff0c\u540c\u6642\u4e5f\u662f\u73a9\u5bb6\u64cd\u4f5c\u9396\u5b9a\u6642\u9593\u3002")]
    [SerializeField] private float turnDuration = 1.2f;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6bcf\u6b21\u89f8\u767c\u5f8c\u6703\u81ea\u52d5\u53cd\u5411\uff0c\u8b93\u73a9\u5bb6\u518d\u6b21\u78b0\u5230\u6642\u53ef\u4ee5\u8f49\u56de\u53bb\u3002")]
    [SerializeField] private bool alternateDirection;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u8f49\u89d2\u6703\u4f9d\u7167\u73a9\u5bb6\u9032\u5165 Trigger \u7684\u65b9\u5411\u6c7a\u5b9a\u65cb\u8f49\u65b9\u5411\u3002\u5f9e\u53e6\u4e00\u5074\u9032\u5165\u6642\u6703\u81ea\u52d5\u53cd\u5411\u65cb\u8f49\u3002")]
    [SerializeField] private bool turnDirectionFollowsEntrySide = true;

    [Tooltip("\u73a9\u5bb6\u6cbf\u76ee\u524d\u79fb\u52d5\u65b9\u5411\u8ddd\u96e2\u89f8\u767c\u5668\u4e2d\u5fc3\u5c0f\u65bc\u9019\u500b\u503c\u6642\u624d\u89f8\u767c\u3002")]
    [SerializeField] private float centerTriggerDistance = 0.15f;

    [Tooltip("\u8981\u65cb\u8f49\u7684\u651d\u5f71\u6a5f\u3002\u6c92\u6709\u6307\u5b9a\u6642\u6703\u81ea\u52d5\u6293 Main Camera\u3002")]
    [SerializeField] private SideScrollerCamera sideScrollerCamera;

    [Header("Turn Direction Guide")]
    [SerializeField] private bool showTurnDirectionGuide = true;
    [SerializeField] private bool onlyShowGuideWhenSelected = true;
    [SerializeField] private Vector3 guideLocalMovementAxis = Vector3.right;
    [SerializeField] private bool flipGuideEntryDirection;
    [SerializeField] private float guideLineLength = 2.4f;
    [SerializeField] private Color guideLineColor = new Color(1f, 0.85f, 0.15f, 0.95f);

    private readonly HashSet<Collider> playerCollidersInside = new HashSet<Collider>();
    private Collider triggerCollider;
    private PlayerMotor3D activePlayer;
    private bool waitingForPlayerExit;
    private bool useOppositeDirection;
    private float enteredSide;
    private Vector3 enteredMovementAxis = Vector3.right;
    private bool hasEnteredSide;

    private void Reset()
    {
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        EnsureTriggerCollider();
    }

    private void OnValidate()
    {
        EnsureTriggerCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMotor3D playerMotor = other.GetComponentInParent<PlayerMotor3D>();
        if (playerMotor == null)
        {
            return;
        }

        if (!playerMotor.UsesSideScrollerMovement)
        {
            return;
        }

        playerCollidersInside.Add(other);
        activePlayer = playerMotor;

        if (!hasEnteredSide)
        {
            enteredMovementAxis = ResolveIncomingMovementAxis(playerMotor);
            enteredSide = GetSignedDistanceToCenter(playerMotor, enteredMovementAxis);
            hasEnteredSide = true;
        }

        TryTriggerTurnAtCenter(playerMotor);
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerMotor3D playerMotor = other.GetComponentInParent<PlayerMotor3D>();
        if (playerMotor == null || playerMotor != activePlayer)
        {
            return;
        }

        if (!playerMotor.UsesSideScrollerMovement)
        {
            return;
        }

        TryTriggerTurnAtCenter(playerMotor);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerMotor3D playerMotor = other.GetComponentInParent<PlayerMotor3D>();
        if (playerMotor == null)
        {
            return;
        }

        playerCollidersInside.Remove(other);
        if (playerCollidersInside.Count == 0)
        {
            waitingForPlayerExit = false;
            activePlayer = null;
            enteredMovementAxis = Vector3.right;
            hasEnteredSide = false;
        }
    }

    private void TryTriggerTurnAtCenter(PlayerMotor3D playerMotor)
    {
        if (waitingForPlayerExit || !HasReachedCenter(playerMotor))
        {
            return;
        }

        SideScrollerCamera cameraController = GetCameraController();
        if (cameraController == null)
        {
            return;
        }

        float signedDegrees = GetSignedTurnDegrees();
        waitingForPlayerExit = true;

        if (!turnDirectionFollowsEntrySide && alternateDirection)
        {
            useOppositeDirection = !useOppositeDirection;
        }

        cameraController.RotateYawKeepingTargetViewport(signedDegrees, turnDuration);
        playerMotor.ApplyCornerTurn(signedDegrees, turnDuration);
    }

    private bool HasReachedCenter(PlayerMotor3D playerMotor)
    {
        float currentSide = GetSignedDistanceToCenter(playerMotor);
        if (Mathf.Abs(currentSide) <= Mathf.Max(0.01f, centerTriggerDistance))
        {
            return true;
        }

        return hasEnteredSide && Mathf.Sign(currentSide) != Mathf.Sign(enteredSide);
    }

    private float GetSignedDistanceToCenter(PlayerMotor3D playerMotor)
    {
        Vector3 axis = hasEnteredSide ? enteredMovementAxis : ResolveIncomingMovementAxis(playerMotor);
        return GetSignedDistanceToCenter(playerMotor, axis);
    }

    private float GetSignedDistanceToCenter(PlayerMotor3D playerMotor, Vector3 axis)
    {
        Vector3 center = triggerCollider != null ? triggerCollider.bounds.center : transform.position;
        Vector3 playerPosition = playerMotor.transform.position;
        return Vector3.Dot(playerPosition - center, axis);
    }

    private float GetSignedTurnDegrees()
    {
        if (turnDirectionFollowsEntrySide)
        {
            return GetSignedTurnDegreesForIncomingAxis(enteredMovementAxis);
        }

        return GetPrimarySignedTurnDegrees();
    }

    private SideScrollerCamera GetCameraController()
    {
        if (sideScrollerCamera != null)
        {
            return sideScrollerCamera;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            sideScrollerCamera = mainCamera.GetComponent<SideScrollerCamera>();
        }

        if (sideScrollerCamera == null)
        {
            sideScrollerCamera = FindFirstObjectByType<SideScrollerCamera>();
        }

        return sideScrollerCamera;
    }

    private void OnDrawGizmos()
    {
        if (showTurnDirectionGuide && !onlyShowGuideWhenSelected)
        {
            DrawTurnDirectionGuide();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showTurnDirectionGuide && onlyShowGuideWhenSelected)
        {
            DrawTurnDirectionGuide();
        }
    }

    private void DrawTurnDirectionGuide()
    {
        Vector3 center = ResolveGuideCenter();
        Vector3 axis = ResolveGuideMovementAxis();
        DrawTurnPathForEntrySide(center, axis, GetGuideEntrySide());
    }

    private void DrawTurnPathForEntrySide(Vector3 center, Vector3 axis, float entrySide)
    {
        float length = Mathf.Max(0.25f, guideLineLength);
        float signedDegrees = GetPrimarySignedTurnDegrees();
        Vector3 entryDirection = -axis * entrySide;
        Vector3 exitDirection = Quaternion.AngleAxis(signedDegrees, Vector3.up) * entryDirection;
        Vector3 entryStart = center + axis * entrySide * length * 0.45f;
        Vector3 exitEnd = center + exitDirection * length * 0.7f;

        Gizmos.color = guideLineColor;
        Gizmos.DrawLine(entryStart, center);
        Gizmos.DrawLine(center, exitEnd);
        DrawGuideArrowHead(exitEnd, exitDirection, length * 0.22f);
    }

    private void DrawGuideArrowHead(Vector3 position, Vector3 direction, float size)
    {
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 forward = direction.normalized;
#if UNITY_EDITOR
        Handles.color = guideLineColor;
        Quaternion rotation = Quaternion.LookRotation(forward, Vector3.up);
        float handleSize = Mathf.Max(size, HandleUtility.GetHandleSize(position) * 0.18f);
        Handles.ArrowHandleCap(0, position - forward * handleSize * 0.45f, rotation, handleSize, EventType.Repaint);
#else
        Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;
        if (side.sqrMagnitude <= 0.0001f)
        {
            side = Vector3.right;
        }

        Gizmos.color = guideLineColor;
        Gizmos.DrawLine(position, position - forward * size + side * size * 0.55f);
        Gizmos.DrawLine(position, position - forward * size - side * size * 0.55f);
#endif
    }

    private float GetSignedTurnDegreesForIncomingAxis(Vector3 incomingAxis)
    {
        float primarySignedDegrees = GetPrimarySignedTurnDegrees();
        Vector3 primaryAxis = ResolveGuideMovementAxis();
        Vector3 secondaryAxis = Quaternion.AngleAxis(primarySignedDegrees, Vector3.up) * primaryAxis;
        Vector3 axis = FlattenHorizontal(incomingAxis);

        float primaryAlignment = Mathf.Abs(Vector3.Dot(axis, primaryAxis));
        float secondaryAlignment = Mathf.Abs(Vector3.Dot(axis, secondaryAxis));
        return secondaryAlignment > primaryAlignment ? -primarySignedDegrees : primarySignedDegrees;
    }

    private float GetPrimarySignedTurnDegrees()
    {
        bool turnRight = firstTurnDirection == TurnDirection.Right;
        if (!turnDirectionFollowsEntrySide && useOppositeDirection)
        {
            turnRight = !turnRight;
        }

        return turnRight ? turnDegrees : -turnDegrees;
    }

    private float GetGuideEntrySide()
    {
        return flipGuideEntryDirection ? 1f : -1f;
    }

    private Vector3 ResolveIncomingMovementAxis(PlayerMotor3D playerMotor)
    {
        Vector3 playerAxis = playerMotor != null ? FlattenHorizontal(playerMotor.MovementAxis) : Vector3.zero;
        if (playerAxis.sqrMagnitude <= 0.0001f)
        {
            return ResolveGuideMovementAxis();
        }

        if (!turnDirectionFollowsEntrySide)
        {
            return playerAxis;
        }

        float primarySignedDegrees = GetPrimarySignedTurnDegrees();
        Vector3 primaryAxis = ResolveGuideMovementAxis();
        Vector3 secondaryAxis = Quaternion.AngleAxis(primarySignedDegrees, Vector3.up) * primaryAxis;

        float primaryAlignment = Mathf.Abs(Vector3.Dot(playerAxis, primaryAxis));
        float secondaryAlignment = Mathf.Abs(Vector3.Dot(playerAxis, secondaryAxis));
        return secondaryAlignment > primaryAlignment ? secondaryAxis.normalized : primaryAxis.normalized;
    }

    private Vector3 ResolveGuideCenter()
    {
        Collider guideCollider = triggerCollider != null ? triggerCollider : GetComponent<Collider>();
        return guideCollider != null ? guideCollider.bounds.center : transform.position;
    }

    private Vector3 ResolveGuideMovementAxis()
    {
        Vector3 axis = transform.TransformDirection(guideLocalMovementAxis);
        axis = FlattenHorizontal(axis);
        if (axis.sqrMagnitude <= 0.0001f)
        {
            axis = Vector3.right;
        }

        return axis.normalized;
    }

    private static Vector3 FlattenHorizontal(Vector3 value)
    {
        value.y = 0f;
        return value.sqrMagnitude <= 0.0001f ? Vector3.zero : value.normalized;
    }

    private void EnsureTriggerCollider()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }
    }
}
