using UnityEngine;

public class SideScrollerCamera : MonoBehaviour
{
    [Header("\u8ddf\u96a8\u76ee\u6a19")]
    [Tooltip("\u651d\u5f71\u6a5f\u8981\u8ddf\u96a8\u7684\u73a9\u5bb6\u89d2\u8272\u3002")]
    [SerializeField] private Transform target;

    [Tooltip("\u7528\u89d2\u8272 Collider \u4e2d\u5fc3\u5224\u65b7\u5782\u76f4\u756b\u9762\u4f4d\u7f6e\uff0c\u907f\u514d Player \u539f\u9ede\u5728\u8173\u5e95\u6642\u5224\u65b7\u4e0d\u6e96\u3002")]
    [SerializeField] private bool useColliderCenterForVerticalFraming = true;

    [Tooltip("\u651d\u5f71\u6a5f\u8207\u73a9\u5bb6\u4e4b\u9593\u7684\u672c\u5730\u5ea7\u6a19\u504f\u79fb\u3002X \u662f\u756b\u9762\u5de6\u53f3\uff0cY \u662f\u4e0a\u4e0b\uff0cZ \u662f\u93e1\u982d\u524d\u5f8c\uff1b\u651d\u5f71\u6a5f Yaw \u4e0d\u662f 0 \u6642\u4e5f\u6703\u6b63\u78ba\u8ddf\u96a8\u3002")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -10f);

    [Tooltip("\u651d\u5f71\u6a5f\u8ddf\u4e0a\u76ee\u6a19\u4f4d\u7f6e\u7684\u901f\u5ea6\u3002")]
    [SerializeField] private float followSpeed = 8f;

    [Header("\u6c34\u5e73\u6b7b\u5340")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u73a9\u5bb6\u5728\u6c34\u5e73\u6b7b\u5340\u5167\u79fb\u52d5\u6642\uff0c\u651d\u5f71\u6a5f\u4e0d\u6703\u6c34\u5e73\u8ddf\u96a8\u3002")]
    [SerializeField] private bool useHorizontalDeadZone = true;

    [Tooltip("\u756b\u9762\u6c34\u5e73\u6b7b\u5340\u7bc4\u570d\uff0c0 \u662f\u756b\u9762\u6700\u5de6\u5074\uff0c1 \u662f\u756b\u9762\u6700\u53f3\u5074\u3002")]
    [SerializeField] private Vector2 horizontalDeadZoneViewport = new Vector2(0.35f, 0.65f);

    [Header("\u5834\u666f\u651d\u5f71\u6a5f\u59ff\u52e2")]
    [Tooltip("Play \u5f8c\u63a1\u7528\u5834\u666f\u4e2d\u5df2\u7d93\u64fa\u597d\u7684\u651d\u5f71\u6a5f\u4f4d\u7f6e\u4f5c\u70ba\u521d\u59cb\u504f\u79fb\u3002")]
    [SerializeField] private bool useSceneCameraPoseOnPlay = true;

    [Tooltip("Play \u5f8c\u4fdd\u6301\u5834\u666f\u4e2d\u5df2\u7d93\u64fa\u597d\u7684\u651d\u5f71\u6a5f\u65cb\u8f49\u3002")]
    [SerializeField] private bool keepSceneRotation = true;

    [Header("\u8df3\u677f\u5782\u76f4\u8ddf\u96a8")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u73a9\u5bb6\u7ad9\u5728 OneWayPlatform3D \u6216\u5f9e\u5b83\u8d77\u8df3\u6642\uff0c\u651d\u5f71\u6a5f\u6703\u8ddf\u96a8\u73a9\u5bb6 Y \u8ef8\u3002")]
    [SerializeField] private bool enableOneWayPlatformVerticalFollow = true;

    [Tooltip("\u96e2\u958b\u8df3\u677f\u5f8c\uff0c\u4ecd\u8996\u70ba\u8df3\u677f\u8d77\u8df3\u7684\u77ed\u66ab\u6642\u9593\u3002")]
    [SerializeField] private float oneWayPlatformGraceSeconds = 0.25f;

    [Header("\u79fb\u52d5\u908a\u754c")]
    [Tooltip("\u651d\u5f71\u6a5f X \u8ef8\u53ef\u79fb\u52d5\u7bc4\u570d\u3002")]
    [SerializeField] private Vector2 xBounds = new Vector2(-3f, 100f);

    [Tooltip("\u651d\u5f71\u6a5f Y \u8ef8\u53ef\u79fb\u52d5\u7bc4\u570d\u3002")]
    [SerializeField] private Vector2 yBounds = new Vector2(-20f, 30f);

    [Tooltip("\u651d\u5f71\u6a5f\u8f49\u89d2\u5f8c\u6cbf\u4e16\u754c Z \u8ef8\u79fb\u52d5\u6642\u7684\u53ef\u79fb\u52d5\u7bc4\u570d\u3002")]
    [SerializeField] private Vector2 zBounds = new Vector2(-3f, 50f);

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u651d\u5f71\u6a5f\u8f49\u5411\u5f8c\u4e0d\u518d\u4f7f\u7528\u4e16\u754c X Bounds\uff0c\u907f\u514d X Bounds \u628a\u651d\u5f71\u6a5f\u63a8\u5230\u89d2\u8272\u8eab\u4e0a\u3002")]
    [SerializeField] private bool disableXBoundsAfterCameraTurn = true;

    [Header("\u79fb\u52d5\u908a\u754c\u8996\u89ba\u5316")]
    [Tooltip("\u5728 Scene \u8996\u7a97\u986f\u793a\u651d\u5f71\u6a5f X/Y \u79fb\u52d5\u908a\u754c\u3002")]
    [SerializeField] private bool showMovementBoundsGizmo = true;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u4e0d\u9700\u8981\u9078\u53d6\u651d\u5f71\u6a5f\u4e5f\u6703\u986f\u793a\u79fb\u52d5\u908a\u754c\u3002")]
    [SerializeField] private bool alwaysShowMovementBoundsGizmo;

    [Tooltip("\u79fb\u52d5\u908a\u754c\u7dda\u6846\u984f\u8272\u3002")]
    [SerializeField] private Color movementBoundsGizmoColor = new Color(0.15f, 0.75f, 1f, 0.75f);

    [Tooltip("\u7dda\u6846\u5728 Z \u8ef8\u4e0a\u7684\u4e2d\u5fc3\u4f4d\u7f6e\uff1b\u901a\u5e38\u53ef\u8a2d\u6210\u73a9\u5bb6\u6216\u95dc\u5361\u6240\u5728\u7684 Z\u3002")]
    [SerializeField] private float movementBoundsGizmoCenterZ;

    [Tooltip("\u7dda\u6846\u5728 Z \u8ef8\u4e0a\u7684\u539a\u5ea6\uff0c\u65b9\u4fbf\u5f9e\u4e0d\u540c\u89d2\u5ea6\u770b\u5230\u908a\u754c\u3002")]
    [SerializeField] private float movementBoundsGizmoDepth = 1f;

    [Header("\u8f49\u89d2\u651d\u5f71\u6a5f")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u8f49\u89d2\u65cb\u8f49\u7d50\u675f\u6642\u6703\u628a\u89d2\u8272\u7684\u6c34\u5e73\u756b\u9762\u4f4d\u7f6e\u5de6\u53f3\u5c0d\u8abf\uff0c\u4f8b\u5982\u53f3\u4e0b\u89d2\u6703\u8b8a\u6210\u5de6\u4e0b\u89d2\u3002")]
    [SerializeField] private bool mirrorViewportXOnTurn = true;

    [Tooltip("\u8f49\u89d2\u5f8c\u89d2\u8272\u5728\u756b\u9762\u5de6\u53f3\u4f4d\u7f6e\u7684\u6700\u5c0f\u503c\uff0c\u907f\u514d\u592a\u8cbc\u756b\u9762\u908a\u7de3\u3002")]
    [SerializeField] private float turnViewportMinX = 0.18f;

    [Tooltip("\u8f49\u89d2\u5f8c\u89d2\u8272\u5728\u756b\u9762\u5de6\u53f3\u4f4d\u7f6e\u7684\u6700\u5927\u503c\uff0c\u907f\u514d\u592a\u8cbc\u756b\u9762\u908a\u7de3\u3002")]
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
