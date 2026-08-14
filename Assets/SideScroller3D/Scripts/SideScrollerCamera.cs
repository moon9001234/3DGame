using UnityEngine;

public class SideScrollerCamera : MonoBehaviour
{
    [Header("跟隨目標")]
    [Tooltip("攝影機要跟隨的玩家角色。")]
    [SerializeField] private Transform target;

    [Tooltip("用角色 Collider 中心判斷垂直畫面位置，避免 Player 原點在腳底時判斷不準。")]
    [SerializeField] private bool useColliderCenterForVerticalFraming = true;

    [Tooltip("攝影機與玩家之間的本地座標偏移。X 是畫面左右，Y 是上下，Z 是鏡頭前後；攝影機 Yaw 不是 0 時也會正確跟隨。")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Tooltip("攝影機跟上目標位置的速度。")]
    [SerializeField] private float followSpeed = 8f;

    [Header("水平死區")]
    [Tooltip("開啟後，玩家在水平死區內移動時，攝影機不會水平跟隨。")]
    [SerializeField] private bool useHorizontalDeadZone = true;

    [Tooltip("畫面水平死區範圍，0 是畫面最左側，1 是畫面最右側。")]
    [SerializeField] private Vector2 horizontalDeadZoneViewport = new Vector2(0.35f, 0.65f);

    [Header("場景攝影機姿勢")]
    [Tooltip("Play 後採用場景中已經擺好的攝影機位置作為初始偏移。")]
    [SerializeField] private bool useSceneCameraPoseOnPlay = true;

    [Tooltip("Play 後保持場景中已經擺好的攝影機旋轉。")]
    [SerializeField] private bool keepSceneRotation = true;

    [Header("跳板垂直跟隨")]
    [Tooltip("開啟後，玩家站在 OneWayPlatform3D 或從它起跳時，攝影機會跟隨玩家 Y 軸。")]
    [SerializeField] private bool enableOneWayPlatformVerticalFollow = true;

    [Tooltip("離開跳板後，仍視為跳板起跳的短暫時間。")]
    [SerializeField] private float oneWayPlatformGraceSeconds = 0.25f;

    [Header("移動邊界")]
    [Tooltip("攝影機 X 軸可移動範圍。")]
    [SerializeField] private Vector2 xBounds = new Vector2(-3f, 100f);

    [Tooltip("攝影機 Y 軸可移動範圍。")]
    [SerializeField] private Vector2 yBounds = new Vector2(-20f, 30f);

    [Tooltip("攝影機轉角後沿世界 Z 軸移動時的可移動範圍。")]
    [SerializeField] private Vector2 zBounds = new Vector2(-3f, 50f);

    [Tooltip("開啟後，攝影機轉向後不再使用世界 X Bounds，避免 X Bounds 把攝影機推到角色身上。")]
    [SerializeField] private bool disableXBoundsAfterCameraTurn = true;

    [Header("移動邊界視覺化")]
    [Tooltip("在 Scene 視窗顯示攝影機 X/Y 移動邊界。")]
    [SerializeField] private bool showMovementBoundsGizmo = true;

    [Tooltip("開啟後，不需要選取攝影機也會顯示移動邊界。")]
    [SerializeField] private bool alwaysShowMovementBoundsGizmo;

    [Tooltip("移動邊界線框顏色。")]
    [SerializeField] private Color movementBoundsGizmoColor = new Color(0.15f, 0.75f, 1f, 0.75f);

    [Tooltip("線框在 Z 軸上的中心位置；通常可設成玩家或關卡所在的 Z。")]
    [SerializeField] private float movementBoundsGizmoCenterZ;

    [Tooltip("線框在 Z 軸上的厚度，方便從不同角度看到邊界。")]
    [SerializeField] private float movementBoundsGizmoDepth = 1f;

    [Header("轉角攝影機")]
    [Tooltip("開啟後，轉角旋轉結束時會把角色的水平畫面位置左右對調，例如右下角會變成左下角。")]
    [SerializeField] private bool mirrorViewportXOnTurn = true;

    [Tooltip("轉角後角色在畫面左右位置的最小值，避免太貼畫面邊緣。")]
    [SerializeField] private float turnViewportMinX = 0.18f;

    [Tooltip("轉角後角色在畫面左右位置的最大值，避免太貼畫面邊緣。")]
    [SerializeField] private float turnViewportMaxX = 0.82f;

    private Camera cameraComponent;
    private Collider targetCollider;
    private PlayerMotor3D targetMotor;
    private Quaternion sceneRotation;
    private bool capturedScenePose;
    private float lockedCameraX;
    private bool hasLockedCameraX;
    private float lockedCameraY;
    private float groundedCameraY;
    private float groundedTargetY;
    private float defaultCameraY;
    private bool hasGroundedCameraAnchor;
    private bool oneWayPlatformVerticalFollowActive;
    private bool turnRotationActive;
    private Quaternion turnStartRotation;
    private Quaternion turnTargetRotation;
    private Vector3 turnLockedViewport;
    private float turnTargetViewportX;
    private float turnCameraDistance;
    private float turnStartTime;
    private float turnDuration;
    private bool externalCameraYActive;
    private float externalCameraY;
    private bool offsetWasCapturedFromScene;
    private Vector3 activeOffset;
    private Vector2 activeYBounds;

    public Transform Target
    {
        get => target;
        set
        {
            target = value;
            targetCollider = null;
            targetMotor = null;
            capturedScenePose = false;
            offsetWasCapturedFromScene = false;
            hasLockedCameraX = false;
        }
    }

    public Vector3 Offset
    {
        get => capturedScenePose ? activeOffset : offset;
        set
        {
            offset = value;
            activeOffset = value;
        }
    }

    public float SceneCameraY => capturedScenePose ? defaultCameraY : transform.position.y;

    public void SetCameraLevelY(float cameraY)
    {
        externalCameraY = cameraY;
        externalCameraYActive = true;
    }

    public void EnsureCameraYBounds(float minY, float maxY)
    {
        activeYBounds.x = Mathf.Min(activeYBounds.x, minY);
        activeYBounds.y = Mathf.Max(activeYBounds.y, maxY);
    }

    public void ClearCameraLevelY()
    {
        externalCameraYActive = false;
    }

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        CacheTargetReferences();
        InitializeRuntimeSettingsFromScene();
        CaptureScenePose();
    }

    private void OnEnable()
    {
        cameraComponent = cameraComponent != null ? cameraComponent : GetComponent<Camera>();
        CacheTargetReferences();
        if (!capturedScenePose)
        {
            InitializeRuntimeSettingsFromScene();
        }

        CaptureScenePose();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        CacheTargetReferences();
        CaptureScenePose();
        UpdateCameraVerticalPosition();

        if (turnRotationActive)
        {
            UpdateTurnRotation();
            return;
        }

        Vector3 desired = GetDesiredCameraPosition();
        desired = UpdateLockedHorizontalPosition(desired);

        Vector2 activeHorizontalBounds = GetActiveHorizontalBounds();
        Vector2 activeYBounds = GetActiveYBounds();
        if (ShouldApplyHorizontalBounds())
        {
            desired = ClampCameraHorizontalAxis(desired, activeHorizontalBounds);
            lockedCameraX = Vector3.Dot(desired, GetCameraHorizontalAxis());
        }

        desired.y = Mathf.Clamp(lockedCameraY, activeYBounds.x, activeYBounds.y);

        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);

        if (keepSceneRotation)
        {
            transform.rotation = sceneRotation;
        }
    }

    public void RotateYawKeepingTargetViewport(float yawDegrees, float duration)
    {
        if (target == null)
        {
            return;
        }

        cameraComponent = cameraComponent != null ? cameraComponent : GetComponent<Camera>();
        if (cameraComponent == null)
        {
            return;
        }

        CacheTargetReferences();
        turnStartRotation = transform.rotation;
        turnTargetRotation = Quaternion.AngleAxis(yawDegrees, Vector3.up) * turnStartRotation;
        turnLockedViewport = cameraComponent.WorldToViewportPoint(GetFramingPosition());
        turnTargetViewportX = mirrorViewportXOnTurn ? 1f - turnLockedViewport.x : turnLockedViewport.x;
        turnTargetViewportX = Mathf.Clamp(turnTargetViewportX, turnViewportMinX, turnViewportMaxX);
        turnCameraDistance = Mathf.Max(0.5f, GetCameraDistance(transform.position));
        turnStartTime = Time.time;
        turnDuration = Mathf.Max(0.01f, duration);
        turnRotationActive = true;
    }

    private void UpdateTurnRotation()
    {
        float stableCameraY = lockedCameraY;
        float progress = Mathf.Clamp01((Time.time - turnStartTime) / turnDuration);
        float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
        transform.rotation = Quaternion.Slerp(turnStartRotation, turnTargetRotation, easedProgress);
        sceneRotation = transform.rotation;
        transform.position = GetFramingPosition() - (sceneRotation * Vector3.forward) * turnCameraDistance;
        RestoreStableCameraY(stableCameraY);
        ApplyTurnViewportFraming(easedProgress);
        RestoreStableCameraY(stableCameraY);

        activeOffset = Quaternion.Inverse(sceneRotation) * (transform.position - target.position);
        offsetWasCapturedFromScene = true;
        lockedCameraX = Vector3.Dot(transform.position, GetCameraHorizontalAxis());
        hasLockedCameraX = true;
        lockedCameraY = transform.position.y;

        if (progress >= 1f)
        {
            transform.rotation = turnTargetRotation;
            sceneRotation = turnTargetRotation;
            transform.position = GetFramingPosition() - (sceneRotation * Vector3.forward) * turnCameraDistance;
            RestoreStableCameraY(stableCameraY);
            ApplyTurnViewportFraming(1f);
            RestoreStableCameraY(stableCameraY);
            activeOffset = Quaternion.Inverse(sceneRotation) * (transform.position - target.position);
            offsetWasCapturedFromScene = true;
            lockedCameraX = Vector3.Dot(transform.position, GetCameraHorizontalAxis());
            hasLockedCameraX = true;
            lockedCameraY = transform.position.y;
            turnRotationActive = false;
        }
    }

    private Vector3 UpdateLockedHorizontalPosition(Vector3 targetCameraPosition)
    {
        Vector3 horizontalAxis = GetCameraHorizontalAxis();
        float targetCameraX = Vector3.Dot(targetCameraPosition, horizontalAxis);

        if (!hasLockedCameraX)
        {
            lockedCameraX = Vector3.Dot(transform.position, horizontalAxis);
            hasLockedCameraX = true;
        }

        if (!useHorizontalDeadZone)
        {
            lockedCameraX = targetCameraX;
            return targetCameraPosition;
        }

        Vector3 lockedPosition = targetCameraPosition + horizontalAxis * (lockedCameraX - targetCameraX);
        float viewportX = CalculateViewportX(lockedPosition, GetFramingPosition());
        float leftEdge = Mathf.Clamp01(Mathf.Min(horizontalDeadZoneViewport.x, horizontalDeadZoneViewport.y));
        float rightEdge = Mathf.Clamp01(Mathf.Max(horizontalDeadZoneViewport.x, horizontalDeadZoneViewport.y));

        if (viewportX < leftEdge)
        {
            lockedPosition = MoveCameraToViewportX(lockedPosition, viewportX, leftEdge);
        }
        else if (viewportX > rightEdge)
        {
            lockedPosition = MoveCameraToViewportX(lockedPosition, viewportX, rightEdge);
        }

        lockedCameraX = Vector3.Dot(lockedPosition, horizontalAxis);
        return lockedPosition;
    }

    private Vector3 GetDesiredCameraPosition()
    {
        return target.position + sceneRotation * activeOffset;
    }

    private Vector3 GetCameraHorizontalAxis()
    {
        Vector3 horizontalAxis = Vector3.ProjectOnPlane(sceneRotation * Vector3.right, Vector3.up);
        if (horizontalAxis.sqrMagnitude < 0.0001f)
        {
            horizontalAxis = Vector3.ProjectOnPlane(transform.right, Vector3.up);
        }

        return horizontalAxis.sqrMagnitude < 0.0001f ? Vector3.right : horizontalAxis.normalized;
    }

    private Vector3 MoveCameraToViewportX(Vector3 cameraPosition, float currentViewportX, float desiredViewportX)
    {
        return cameraPosition + GetCameraHorizontalAxis() * ((currentViewportX - desiredViewportX) * GetOneScreenWorldWidth(cameraPosition));
    }

    private Vector3 ClampCameraHorizontalAxis(Vector3 cameraPosition, Vector2 bounds)
    {
        Vector3 horizontalAxis = GetCameraHorizontalAxis();
        float currentAxisPosition = Vector3.Dot(cameraPosition, horizontalAxis);
        float clampedAxisPosition = Mathf.Clamp(currentAxisPosition, Mathf.Min(bounds.x, bounds.y), Mathf.Max(bounds.x, bounds.y));
        return cameraPosition + horizontalAxis * (clampedAxisPosition - currentAxisPosition);
    }

    private void RestoreStableCameraY(float stableCameraY)
    {
        Vector3 position = transform.position;
        position.y = stableCameraY;
        transform.position = position;
    }

    private void ApplyTurnViewportFraming(float progress)
    {
        if (cameraComponent == null)
        {
            return;
        }

        Vector3 framingPosition = GetFramingPosition();
        Vector3 currentViewport = cameraComponent.WorldToViewportPoint(framingPosition);
        if (currentViewport.z <= 0.001f)
        {
            return;
        }

        float desiredViewportX = Mathf.Lerp(turnLockedViewport.x, turnTargetViewportX, progress);
        Vector3 desiredWorldAtViewport = cameraComponent.ViewportToWorldPoint(
            new Vector3(desiredViewportX, currentViewport.y, currentViewport.z));
        transform.position += framingPosition - desiredWorldAtViewport;
    }

    private void UpdateCameraVerticalPosition()
    {
        if (externalCameraYActive)
        {
            lockedCameraY = externalCameraY;
            return;
        }

        UpdateLockedVerticalPosition();
    }

    private Vector2 GetActiveHorizontalBounds()
    {
        Vector3 horizontalAxis = GetCameraHorizontalAxis();
        return Mathf.Abs(horizontalAxis.z) > Mathf.Abs(horizontalAxis.x) ? zBounds : xBounds;
    }

    private bool ShouldApplyHorizontalBounds()
    {
        if (!disableXBoundsAfterCameraTurn)
        {
            return true;
        }

        Vector3 horizontalAxis = GetCameraHorizontalAxis();
        if (Mathf.Abs(horizontalAxis.z) > Mathf.Abs(horizontalAxis.x))
        {
            return true;
        }

        Vector3 cameraForward = sceneRotation * Vector3.forward;
        return Mathf.Abs(Vector3.Dot(cameraForward.normalized, Vector3.forward)) >= 0.65f;
    }

    private Vector2 GetActiveYBounds()
    {
        return activeYBounds;
    }

    private void KeepTargetAtLockedViewport()
    {
        Vector3 framingPosition = GetFramingPosition();
        Vector3 currentViewport = cameraComponent.WorldToViewportPoint(framingPosition);
        if (currentViewport.z <= 0.001f)
        {
            return;
        }

        Vector3 desiredWorldAtLockedViewport = cameraComponent.ViewportToWorldPoint(
            new Vector3(turnLockedViewport.x, turnLockedViewport.y, currentViewport.z));
        transform.position += framingPosition - desiredWorldAtLockedViewport;
    }

    private void UpdateLockedVerticalPosition()
    {
        if (cameraComponent == null)
        {
            lockedCameraY = transform.position.y;
            return;
        }

        bool wasFollowingOneWayPlatform = oneWayPlatformVerticalFollowActive;
        bool shouldFollowOneWayPlatform = ShouldUseOneWayPlatformVerticalFollow();

        if (!shouldFollowOneWayPlatform)
        {
            if (targetMotor != null && targetMotor.IsGrounded)
            {
                if (!targetMotor.IsOnOneWayPlatform)
                {
                    UpdateGroundedCameraAnchor();
                }

                lockedCameraY = groundedCameraY;
            }

            return;
        }

        if (!wasFollowingOneWayPlatform)
        {
            if (targetMotor == null || !targetMotor.IsOnOneWayPlatform)
            {
                UpdateGroundedCameraAnchor();
            }
        }

        lockedCameraY = GetFramingPosition().y + activeOffset.y;
    }

    private void UpdateGroundedCameraAnchor()
    {
        Vector3 framingPosition = GetFramingPosition();
        if (!hasGroundedCameraAnchor || Mathf.Abs(framingPosition.y - groundedTargetY) > 0.15f)
        {
            groundedCameraY = lockedCameraY;
            groundedTargetY = framingPosition.y;
            hasGroundedCameraAnchor = true;
        }
    }

    private bool ShouldUseOneWayPlatformVerticalFollow()
    {
        if (!enableOneWayPlatformVerticalFollow || targetMotor == null)
        {
            oneWayPlatformVerticalFollowActive = false;
            return false;
        }

        if (targetMotor.IsOnOneWayPlatform)
        {
            if (targetMotor.IsGrounded)
            {
                oneWayPlatformVerticalFollowActive = false;
                return false;
            }

            oneWayPlatformVerticalFollowActive = true;
            return true;
        }

        if (!targetMotor.IsGrounded && targetMotor.WasRecentlyOnOneWayPlatform(oneWayPlatformGraceSeconds))
        {
            oneWayPlatformVerticalFollowActive = true;
            return true;
        }

        if (oneWayPlatformVerticalFollowActive && !targetMotor.IsGrounded)
        {
            return true;
        }

        oneWayPlatformVerticalFollowActive = false;
        return false;
    }

    private float CalculateCameraYForViewport(Vector3 cameraPosition, Vector3 worldPoint, float desiredViewportY)
    {
        float viewportY = CalculateViewportY(cameraPosition, worldPoint);
        float screenHeight = GetOneScreenWorldHeight(cameraPosition);
        return cameraPosition.y + (viewportY - desiredViewportY) * screenHeight;
    }

    private float CalculateViewportY(Vector3 cameraPosition, Vector3 worldPoint)
    {
        if (cameraComponent.orthographic)
        {
            float visibleHeight = cameraComponent.orthographicSize * 2f;
            float localY = Vector3.Dot(worldPoint - cameraPosition, sceneRotation * Vector3.up);
            return 0.5f + localY / visibleHeight;
        }

        Matrix4x4 viewMatrix = Matrix4x4.TRS(cameraPosition, sceneRotation, Vector3.one).inverse;
        Vector4 clipPosition = cameraComponent.projectionMatrix * viewMatrix * new Vector4(worldPoint.x, worldPoint.y, worldPoint.z, 1f);

        if (Mathf.Abs(clipPosition.w) < 0.0001f)
        {
            return 0.5f;
        }

        return clipPosition.y / clipPosition.w * 0.5f + 0.5f;
    }

    private float CalculateViewportX(Vector3 cameraPosition, Vector3 worldPoint)
    {
        if (cameraComponent.orthographic)
        {
            float visibleWidth = cameraComponent.orthographicSize * 2f * cameraComponent.aspect;
            float localX = Vector3.Dot(worldPoint - cameraPosition, sceneRotation * Vector3.right);
            return 0.5f + localX / visibleWidth;
        }

        Matrix4x4 viewMatrix = Matrix4x4.TRS(cameraPosition, sceneRotation, Vector3.one).inverse;
        Vector4 clipPosition = cameraComponent.projectionMatrix * viewMatrix * new Vector4(worldPoint.x, worldPoint.y, worldPoint.z, 1f);

        if (Mathf.Abs(clipPosition.w) < 0.0001f)
        {
            return 0.5f;
        }

        return clipPosition.x / clipPosition.w * 0.5f + 0.5f;
    }

    private float GetOneScreenWorldHeight(Vector3 cameraPosition)
    {
        if (cameraComponent == null)
        {
            return 1f;
        }

        float visibleHeight = cameraComponent.orthographic
            ? cameraComponent.orthographicSize * 2f
            : 2f * Mathf.Tan(cameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad) * GetCameraDistance(cameraPosition);
        float verticalProjection = Mathf.Abs(Vector3.Dot(Vector3.up, sceneRotation * Vector3.up));

        if (verticalProjection < 0.001f)
        {
            return visibleHeight;
        }

        return visibleHeight / verticalProjection;
    }

    private float GetOneScreenWorldWidth(Vector3 cameraPosition)
    {
        if (cameraComponent == null)
        {
            return 1f;
        }

        if (cameraComponent.orthographic)
        {
            return cameraComponent.orthographicSize * 2f * cameraComponent.aspect;
        }

        return 2f * Mathf.Tan(cameraComponent.fieldOfView * 0.5f * Mathf.Deg2Rad) * cameraComponent.aspect * GetCameraDistance(cameraPosition);
    }

    private float GetCameraDistance(Vector3 cameraPosition)
    {
        Vector3 framingPosition = GetFramingPosition();
        return Mathf.Abs(Vector3.Dot(framingPosition - cameraPosition, sceneRotation * Vector3.forward));
    }

    private Vector3 GetFramingPosition()
    {
        if (useColliderCenterForVerticalFraming && targetCollider != null)
        {
            return targetCollider.bounds.center;
        }

        return target.position;
    }

    private void CacheTargetReferences()
    {
        if (target == null)
        {
            return;
        }

        if (targetCollider == null)
        {
            targetCollider = target.GetComponent<Collider>();
        }

        if (targetMotor == null)
        {
            targetMotor = target.GetComponent<PlayerMotor3D>();
            if (targetMotor == null)
            {
                targetMotor = target.GetComponentInParent<PlayerMotor3D>();
            }
        }
    }

    private void CaptureScenePose()
    {
        if (capturedScenePose || target == null)
        {
            return;
        }

        InitializeRuntimeSettingsFromScene();
        sceneRotation = transform.rotation;

        if (useSceneCameraPoseOnPlay)
        {
            Vector3 sceneOffset = transform.position - target.position;
            activeOffset = Quaternion.Inverse(sceneRotation) * sceneOffset;
            offsetWasCapturedFromScene = true;
        }
        else if (!offsetWasCapturedFromScene)
        {
            offsetWasCapturedFromScene = true;
        }

        lockedCameraX = Vector3.Dot(transform.position, GetCameraHorizontalAxis());
        hasLockedCameraX = true;
        lockedCameraY = transform.position.y;
        groundedCameraY = lockedCameraY;
        defaultCameraY = lockedCameraY;
        groundedTargetY = GetFramingPosition().y;
        hasGroundedCameraAnchor = true;
        capturedScenePose = true;
    }

    private void InitializeRuntimeSettingsFromScene()
    {
        activeOffset = offset;
        activeYBounds = yBounds;
    }

    private void OnDrawGizmos()
    {
        if (!alwaysShowMovementBoundsGizmo)
        {
            return;
        }

        DrawMovementBoundsGizmo();
    }

    private void OnDrawGizmosSelected()
    {
        DrawMovementBoundsGizmo();
    }

    private void DrawMovementBoundsGizmo()
    {
        if (!showMovementBoundsGizmo)
        {
            return;
        }

        float minX = Mathf.Min(xBounds.x, xBounds.y);
        float maxX = Mathf.Max(xBounds.x, xBounds.y);
        float minZ = Mathf.Min(zBounds.x, zBounds.y);
        float maxZ = Mathf.Max(zBounds.x, zBounds.y);
        float minY = Mathf.Min(yBounds.x, yBounds.y);
        float maxY = Mathf.Max(yBounds.x, yBounds.y);
        if ((Mathf.Approximately(minX, maxX) && Mathf.Approximately(minZ, maxZ)) || Mathf.Approximately(minY, maxY))
        {
            return;
        }

        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, movementBoundsGizmoCenterZ);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, Mathf.Max(0.01f, movementBoundsGizmoDepth));

        Color previousColor = Gizmos.color;
        Gizmos.color = movementBoundsGizmoColor;
        if (!Mathf.Approximately(minX, maxX))
        {
            Gizmos.DrawWireCube(center, size);
        }

        if (!Mathf.Approximately(minZ, maxZ))
        {
            Vector3 zCenter = new Vector3(movementBoundsGizmoCenterZ, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);
            Vector3 zSize = new Vector3(Mathf.Max(0.01f, movementBoundsGizmoDepth), maxY - minY, maxZ - minZ);
            Gizmos.DrawWireCube(zCenter, zSize);
        }

        Gizmos.color = new Color(movementBoundsGizmoColor.r, movementBoundsGizmoColor.g, movementBoundsGizmoColor.b, movementBoundsGizmoColor.a * 0.25f);
        if (!Mathf.Approximately(minX, maxX))
        {
            Gizmos.DrawCube(center, size);
        }

        if (!Mathf.Approximately(minZ, maxZ))
        {
            Vector3 zCenter = new Vector3(movementBoundsGizmoCenterZ, (minY + maxY) * 0.5f, (minZ + maxZ) * 0.5f);
            Vector3 zSize = new Vector3(Mathf.Max(0.01f, movementBoundsGizmoDepth), maxY - minY, maxZ - minZ);
            Gizmos.DrawCube(zCenter, zSize);
        }

        Gizmos.color = previousColor;
    }
}
