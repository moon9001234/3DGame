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

    [Header("轉角設定")]
    [Tooltip("玩家第一次走到觸發器中心時要往右轉或往左轉。")]
    [SerializeField] private TurnDirection firstTurnDirection = TurnDirection.Left;

    [Tooltip("轉向角度。一般街角使用 90 度。")]
    [SerializeField] private float turnDegrees = 90f;

    [Tooltip("攝影機旋轉時間，同時也是玩家操作鎖定時間。")]
    [SerializeField] private float turnDuration = 1.2f;

    [Tooltip("開啟後，每次觸發後會自動反向，讓玩家再次碰到時可以轉回去。")]
    [SerializeField] private bool alternateDirection;

    [Tooltip("開啟後，轉角會依照玩家進入 Trigger 的方向決定旋轉方向。從另一側進入時會自動反向旋轉。")]
    [SerializeField] private bool turnDirectionFollowsEntrySide = true;

    [Tooltip("玩家沿目前移動方向距離觸發器中心小於這個值時才觸發。")]
    [SerializeField] private float centerTriggerDistance = 0.15f;

    [Tooltip("要旋轉的攝影機。沒有指定時會自動抓 Main Camera。")]
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
