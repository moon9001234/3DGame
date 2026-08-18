using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor3D : MonoBehaviour
{
    private const float DashBlockCastSkin = 0.05f;
    private const string DashStateName = "Dash";
    private const string DashEndStateName = "Dash_End";
    private const string JumpUpStateName = "Jump_Up";
    private const string JumpDownStateName = "Jump_Down";
    private const string DeathStateName = "Death";
    private const string DashingParameterName = "Dashing";
    private const string DefaultSideScrollerAnimatorControllerPath = "Assets/SideScroller3D/Animation/PlayerVisual.controller";
    private const string DefaultFree3DAnimatorControllerPath = "Assets/SideScroller3D/Animation/PlayerVisual_Free3D.controller";

    private enum MovementMode
    {
        Free3D,
        SideScroller
    }

    [Header("\u79fb\u52d5\u8a2d\u5b9a")]
    [SerializeField] private MovementMode movementMode = MovementMode.Free3D;

    [Tooltip("\u89d2\u8272\u6c34\u5e73\u79fb\u52d5\u901f\u5ea6\u3002")]
    [SerializeField] private float moveSpeed = 6f;

    [Tooltip("\u7a7a\u4e2d\u53ef\u63a7\u5236\u79fb\u52d5\u7684\u6bd4\u4f8b\u30020 \u4ee3\u8868\u7a7a\u4e2d\u4e0d\u80fd\u8f49\u5411\uff0c1 \u4ee3\u8868\u548c\u5730\u9762\u4e00\u6a23\u597d\u63a7\u5236\u3002")]
    [SerializeField] private float airControl = 0.65f;

    [Tooltip("\u958b\u555f\u5f8c\uff0cWASD \u6703\u4f9d\u7167\u76f8\u6a5f\u6c34\u5e73\u671d\u5411\u79fb\u52d5\u3002")]
    [SerializeField] private bool useCameraRelativeMovement = true;

    [Tooltip("\u6307\u5b9a\u7528\u4f86\u8a08\u7b97\u79fb\u52d5\u65b9\u5411\u7684\u76f8\u6a5f\u3002\u7559\u7a7a\u6642\u6703\u4f7f\u7528 Main Camera\u3002")]
    [SerializeField] private Transform movementCamera;

    [Tooltip("Free3D \u6a21\u5f0f\u4e0b\uff0c\u89d2\u8272\u8f49\u5411\u79fb\u52d5\u65b9\u5411\u7684\u901f\u5ea6\u30020 \u4ee3\u8868\u7acb\u5373\u8f49\u5411\u3002")]
    [SerializeField] private float freeTurnSpeed = 720f;

    [Header("\u6a6b\u5411\u5377\u8ef8\u9650\u5236")]
    [Tooltip("\u53ea\u6709 SideScroller \u6a21\u5f0f\u6703\u4f7f\u7528\uff0c\u7528\u4f86\u628a\u89d2\u8272\u9396\u5728\u56fa\u5b9a\u6df1\u5ea6\u5e73\u9762\u3002")]
    [SerializeField] private float lockedZ = 0f;

    [Tooltip("\u53ea\u6709 SideScroller \u6a21\u5f0f\u6703\u4f7f\u7528\uff0c\u7528\u4f86\u6307\u5b9a\u820a\u6a6b\u5411\u5377\u8ef8\u7684\u79fb\u52d5\u8ef8\u3002")]
    [SerializeField] private Vector3 movementAxis = Vector3.right;

    [Header("\u8df3\u8e8d\u8a2d\u5b9a")]
    [Tooltip("\u89d2\u8272\u8df3\u8e8d\u521d\u901f\u5ea6\u3002\u6578\u503c\u8d8a\u9ad8\u8df3\u5f97\u8d8a\u9ad8\u3002")]
    [SerializeField] private float jumpForce = 8f;

    [Tooltip("\u4e0a\u5347\u6642\u984d\u5916\u91cd\u529b\u500d\u7387\u3002\u6578\u503c\u8d8a\u9ad8\uff0c\u8df3\u8e8d\u4e0a\u5347\u6642\u9593\u8d8a\u77ed\u3002")]
    [SerializeField] private float upwardGravityMultiplier = 1.2f;

    [Tooltip("\u4e0b\u843d\u6642\u984d\u5916\u91cd\u529b\u500d\u7387\u3002\u6578\u503c\u8d8a\u9ad8\uff0c\u843d\u4e0b\u8d8a\u5feb\u3002")]
    [SerializeField] private float fallGravityMultiplier = 2.6f;

    [Tooltip("\u6700\u5927\u4e0b\u843d\u901f\u5ea6\u3002\u907f\u514d\u89d2\u8272\u4e0b\u589c\u904e\u5feb\u3002")]
    [SerializeField] private float maxFallSpeed = 18f;

    [Tooltip("\u63d0\u524d\u6309\u8df3\u8e8d\u6642\u7684\u7de9\u885d\u6642\u9593\u3002")]
    [SerializeField] private float jumpBufferSeconds = 0.12f;

    [Tooltip("\u96e2\u958b\u5730\u9762\u5f8c\u4ecd\u5141\u8a31\u8df3\u8e8d\u7684\u5bec\u5bb9\u6642\u9593\u3002")]
    [SerializeField] private float coyoteTimeSeconds = 0.08f;

    [Tooltip("\u96e2\u5730\u5f8c\u53ef\u984d\u5916\u8df3\u8e8d\u7684\u6b21\u6578\u30021 \u4ee3\u8868\u53ef\u4e8c\u6bb5\u8df3\u3002")]
    [SerializeField] private int extraAirJumps = 1;

    [Tooltip("\u4e8c\u6bb5\u8df3\u7684\u5782\u76f4\u529b\u9053\u500d\u7387\u30021 \u4ee3\u8868\u548c\u666e\u901a\u8df3\u4e00\u6a23\u9ad8\u3002")]
    [SerializeField] private float airJumpForceMultiplier = 1f;

    [Header("\u885d\u523a\u8a2d\u5b9a")]
    [Tooltip("\u6309\u4e0b\u885d\u523a\u9375\u6642\uff0c\u89d2\u8272\u5f80\u9762\u5411\u65b9\u5411\u5feb\u901f\u4f4d\u79fb\u7684\u8ddd\u96e2\u3002")]
    [SerializeField] private float dashDistance = 4f;

    [Tooltip("\u885d\u523a\u4f4d\u79fb\u5b8c\u6210\u6240\u9700\u6642\u9593\u3002\u6578\u503c\u8d8a\u5c0f\uff0c\u885d\u523a\u8d8a\u77ac\u9593\u3002")]
    [SerializeField] private float dashDuration = 0.12f;

    [Tooltip("\u5169\u6b21\u885d\u523a\u4e4b\u9593\u7684\u51b7\u537b\u6642\u9593\u3002")]
    [SerializeField] private float dashCooldown = 0.45f;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u89d2\u8272\u5728\u7a7a\u4e2d\u4e5f\u53ef\u4ee5\u885d\u523a\u3002")]
    [SerializeField] private bool allowAirDash = true;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u885d\u523a\u671f\u9593\u6703\u66ab\u505c\u5782\u76f4\u901f\u5ea6\uff0c\u8b93\u89d2\u8272\u7b46\u76f4\u5f80\u524d\u885d\u3002")]
    [SerializeField] private bool flattenVerticalVelocityDuringDash = true;

    [Tooltip("\u885d\u523a\u4e2d\u8d77\u8df3\u6642\uff0c\u6c34\u5e73\u8df3\u8e8d\u8ddd\u96e2\u7684\u901f\u5ea6\u500d\u7387\u3002")]
    [SerializeField] private float dashJumpHorizontalMultiplier = 1.8f;

    [Tooltip("\u885d\u523a\u4e2d\u8d77\u8df3\u6642\uff0c\u81f3\u5c11\u4fdd\u7559\u591a\u5c11\u6bd4\u4f8b\u7684\u885d\u523a\u901f\u5ea6\u4f5c\u70ba\u52a9\u8dd1\u52d5\u91cf\u3002")]
    [SerializeField] private float dashJumpDashSpeedCarryMultiplier = 0.85f;

    [Tooltip("\u885d\u523a\u8df3\u8e8d\u5f8c\uff0c\u4fdd\u7559\u5411\u524d\u52d5\u91cf\u7684\u6642\u9593\u3002")]
    [SerializeField] private float dashJumpBoostSeconds = 0.35f;

    [Tooltip("\u7a7a\u4e2d\u885d\u523a\u6642\uff0cDash \u52d5\u4f5c\u81f3\u5c11\u4fdd\u7559\u7684\u6642\u9593\uff0c\u907f\u514d dash \u4f4d\u79fb\u592a\u77ed\u6642\u7acb\u523b\u88ab Jump_Down \u84cb\u6389\u3002")]
    [SerializeField] private float airDashAnimationMinSeconds = 0.18f;

    [Tooltip("Dash_End \u81f3\u5c11\u4fdd\u7559\u7684\u6642\u9593\uff0c\u907f\u514d\u885d\u523a\u7d50\u675f\u52d5\u4f5c\u592a\u5feb\u88ab\u5176\u4ed6\u72c0\u614b\u84cb\u6389\u3002")]
    [SerializeField] private float dashEndAnimationMinSeconds = 0.12f;

    [Header("\u885d\u523a\u6b98\u5f71")]
    [SerializeField] private bool enableDashAfterimage = true;
    [SerializeField] private Transform dashAfterimageVisualRoot;
    [SerializeField] private float dashAfterimageSpawnInterval = 0.035f;
    [SerializeField] private float dashAfterimageLifetime = 0.18f;
    [SerializeField] private Color dashAfterimageColor = new Color(0.25f, 0.55f, 1f, 0.45f);
    [SerializeField] private bool dashAfterimageIncludeMeshRenderers = true;
    [SerializeField] private bool dashAfterimageIncludeInactiveRenderers;

    [Header("\u52d5\u756b\u8a2d\u5b9a")]
    [Tooltip("SideScroller \u6a21\u5f0f\u4f7f\u7528\u7684 Animator Controller\u3002\u7559\u7a7a\u6642\u6703\u6cbf\u7528 Animator \u76ee\u524d\u7684\u8a2d\u5b9a\u3002")]
    [SerializeField] private RuntimeAnimatorController sideScrollerAnimatorController;

    [Tooltip("Free3D \u6a21\u5f0f\u4f7f\u7528\u7684 Animator Controller\u3002\u9810\u8a2d\u61c9\u6307\u5411 PlayerVisual_Free3D\uff0cmotion \u4f86\u6e90\u70ba TV_Man_3D.fbx\u3002")]
    [SerializeField] private RuntimeAnimatorController free3DAnimatorController;

    [Tooltip("\u8173\u672c\u5207\u63db Dash\u3001Dash_End\u3001Death \u7b49\u52d5\u4f5c state \u6642\u7684\u6de1\u5165\u6642\u9593\u3002\u8df3\u8e8d\u52d5\u4f5c\u4f7f\u7528\u4e0b\u65b9\u7368\u7acb\u8a2d\u5b9a\u3002")]
    [SerializeField] private float actionAnimationCrossFadeSeconds = 0.04f;

    [Tooltip("Jump_Up \u548c Jump_Down \u4e4b\u9593\u7684\u6de1\u5165\u6642\u9593\u3002\u6578\u503c\u8d8a\u5927\uff0c\u8df3\u8e8d\u4e0a\u5347\u5207\u5230\u4e0b\u843d\u8d8a\u67d4\u548c\u3002")]
    [SerializeField] private float jumpAnimationCrossFadeSeconds = 0.12f;

    [Header("\u5730\u9762\u5075\u6e2c")]
    [Tooltip("\u7528\u4f86\u5224\u65b7\u89d2\u8272\u662f\u5426\u7ad9\u5728\u5730\u9762\u7684\u5b9a\u4f4d\u9ede\u3002")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("\u6c92\u6709\u6307\u5b9a Ground Check \u6642\uff0c\u81ea\u52d5\u5efa\u7acb\u7684\u672c\u5730\u504f\u79fb\u4f4d\u7f6e\u3002")]
    [SerializeField] private Vector3 groundCheckLocalOffset = new Vector3(0f, -1.05f, 0f);

    [Tooltip("\u5730\u9762\u5075\u6e2c\u7403\u9ad4\u534a\u5f91\u3002")]
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Tooltip("\u54ea\u4e9b Layer \u6703\u88ab\u8996\u70ba\u5730\u9762\u3002")]
    [SerializeField] private LayerMask groundMask;

    [Tooltip("\u5099\u7528\u5730\u9762\u5075\u6e2c\u8ddd\u96e2\uff0c\u7528\u4f86\u964d\u4f4e\u843d\u5730\u5224\u65b7\u5931\u6548\u7684\u6a5f\u7387\u3002")]
    [SerializeField] private float groundFallbackDistance = 0.35f;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6c92\u6709\u8a2d\u5b9a Ground Layer \u6642\u6703\u7528\u4efb\u4f55\u53ef\u7ad9\u7acb\u5be6\u9ad4\u4f5c\u70ba\u5099\u7528\u5730\u9762\u3002")]
    [SerializeField] private bool useAnySolidGroundFallback = true;

    [Header("\u55ae\u5411\u8df3\u677f")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u53ef\u7531\u4e0b\u5f80\u4e0a\u7a7f\u8d8a OneWayPlatform3D\uff0c\u7ad9\u4e0a\u53bb\u5f8c\u6309\u4e0b\u52a0\u8df3\u8e8d\u53ef\u5f80\u4e0b\u7a7f\u8d8a\u3002")]
    [SerializeField] private bool enableOneWayPlatforms = true;

    [Tooltip("\u4e0b\u8df3\u6642\uff0c\u66ab\u6642\u5ffd\u7565\u55ae\u5411\u8df3\u677f\u78b0\u649e\u7684\u6642\u9593\u3002")]
    [SerializeField] private float dropThroughSeconds = 0.45f;

    [Tooltip("\u4e0b\u8df3\u6642\u7d66\u89d2\u8272\u7684\u8d77\u59cb\u4e0b\u843d\u901f\u5ea6\uff0c\u8b93\u89d2\u8272\u66f4\u5feb\u96e2\u958b\u8df3\u677f\u78b0\u649e\u7bc4\u570d\u3002")]
    [SerializeField] private float dropThroughStartSpeed = 1.2f;

    [Tooltip("\u6c92\u6709\u8a18\u9304\u76ee\u524d\u8df3\u677f\u6642\uff0c\u53ea\u6703\u641c\u5c0b\u8173\u4e0b\u9019\u6bb5\u8ddd\u96e2\u5167\u7684\u55ae\u5411\u8df3\u677f\uff0c\u907f\u514d\u8aa4\u5ffd\u7565\u4e0b\u5c64\u8df3\u677f\u3002")]
    [SerializeField] private float dropThroughPlatformSearchDistance = 0.18f;

    [Tooltip("\u624b\u628a\u6216\u9375\u76e4\u5782\u76f4\u8ef8\u4f4e\u65bc\u9019\u500b\u503c\u6642\uff0c\u6703\u8996\u70ba\u6309\u4f4f\u4e0b\u65b9\u5411\u3002")]
    [SerializeField] private float dropInputThreshold = 0.45f;

    [Tooltip("\u5f80\u4e0a\u7a7f\u8d8a\u8df3\u677f\u6642\uff0c\u73a9\u5bb6\u5468\u570d\u63d0\u524d\u5075\u6e2c\u55ae\u5411\u8df3\u677f\u7684\u9ad8\u5ea6\u3002")]
    [SerializeField] private float oneWayPlatformPrecheckHeight = 1.6f;

    [Tooltip("\u5f80\u4e0a\u7a7f\u8d8a\u8df3\u677f\u6642\uff0c\u73a9\u5bb6\u5468\u570d\u63d0\u524d\u5075\u6e2c\u55ae\u5411\u8df3\u677f\u7684\u6c34\u5e73\u5916\u64f4\u7bc4\u570d\u3002")]
    [SerializeField] private float oneWayPlatformPrecheckPadding = 0.12f;

    [Header("\u7246\u9762\u6ed1\u843d")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u89d2\u8272\u7a7a\u4e2d\u8cbc\u5230\u7246\u6642\u6703\u505c\u6b62\u6301\u7e8c\u5f80\u7246\u5167\u63a8\uff0c\u907f\u514d\u5361\u5728\u7246\u4e0a\u3002")]
    [SerializeField] private bool preventAirWallSticking = true;

    [Tooltip("\u5074\u9762\u78b0\u649e\u6cd5\u7dda\u5927\u65bc\u9019\u500b\u6578\u503c\u6642\uff0c\u6703\u88ab\u8996\u70ba\u7246\u9762\u3002")]
    [SerializeField] private float wallNormalThreshold = 0.55f;

    [Tooltip("\u96e2\u958b\u7246\u9762\u5f8c\uff0c\u7246\u9762\u963b\u64cb\u72c0\u614b\u4fdd\u7559\u7684\u77ed\u66ab\u6642\u9593\u3002")]
    [SerializeField] private float wallContactGraceSeconds = 0.08f;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6703\u81ea\u52d5\u8b93\u89d2\u8272\u4e3b Collider \u4f7f\u7528\u7121\u6469\u64e6\u6750\u8cea\uff0c\u6e1b\u5c11\u8cbc\u7246\u5361\u4f4f\u3002")]
    [SerializeField] private bool useNoFrictionColliderMaterial = true;

    [Header("\u53d7\u50b7\u5f48\u98db")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u73a9\u5bb6\u53d7\u50b7\u6642\u6703\u88ab\u5f48\u98db\u4e26\u77ed\u66ab\u9396\u5b9a\u64cd\u4f5c\u3002\u95dc\u9589\u5f8c\u53ea\u64ad\u653e\u53d7\u50b7\u9583\u720d\uff0c\u4e0d\u6539\u8b8a\u79fb\u52d5\u901f\u5ea6\u3002")]
    [SerializeField] private bool enableDamageKnockback = true;

    [Tooltip("\u73a9\u5bb6\u78b0\u5230\u6575\u4eba\u6642\u7684\u5f48\u98db\u529b\u9053\u3002X \u662f\u6c34\u5e73\u5f48\u958b\u901f\u5ea6\uff0cY \u662f\u5f80\u4e0a\u5f48\u8d77\u901f\u5ea6\u3002")]
    [FormerlySerializedAs("knockback")]
    [SerializeField] private Vector2 knockbackForce = new Vector2(8f, 4f);

    [Tooltip("\u5f48\u98db\u5f8c\u73a9\u5bb6\u4e0d\u80fd\u64cd\u4f5c\u89d2\u8272\u7684\u6700\u77ed\u6642\u9593\u3002")]
    [SerializeField] private float knockbackControlLockSeconds = 0.45f;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u5982\u679c\u73a9\u5bb6\u9084\u5728\u7a7a\u4e2d\uff0c\u6703\u6301\u7e8c\u9396\u4f4f\u64cd\u4f5c\u76f4\u5230\u843d\u5730\u3002")]
    [SerializeField] private bool lockControlUntilKnockbackLands = true;

    private Rigidbody body;
    private Animator animator;
    private PlayerCombat3D combat;
    private Health health;
    private PlayerDashAfterimage3D dashAfterimage;
    private Vector2 moveInput;
    private Vector3 moveDirection;
    private Vector3 lastMoveDirection = Vector3.right;
    private bool isDashing;
    private float dashUntil;
    private float nextDashTime;
    private Vector3 dashDirectionVector = Vector3.right;
    private bool dashStartedGrounded;
    private bool airDashConsumed;
    private bool wasGroundedLastFixedUpdate;
    private float dashJumpBoostUntil;
    private float dashJumpBoostStartedAt = -999f;
    private Vector3 dashJumpDirectionVector = Vector3.right;
    private float jumpQueuedUntil = -1f;
    private float lastGroundedTime = -999f;
    private int airJumpsUsed;
    private bool jumpedSinceGrounded;
    private bool facingRight = true;
    private float hurtLockedUntil;
    private float cornerTurnLockedUntil;
    private bool knockbackControlLocked;
    private float collisionGroundedUntil;
    private OneWayPlatform3D currentOneWayPlatform;
    private float currentOneWayPlatformUntil;
    private float lastOneWayPlatformGroundedTime = -999f;
    private float wallContactUntil;
    private Vector3 blockedWallNormal;
    private Collider bodyCollider;
    private Collider[] solidPlayerColliders = new Collider[0];
    private PhysicsMaterial noFrictionMaterial;
    private readonly Collider[] groundOverlapHits = new Collider[8];
    private readonly Collider[] oneWayPlatformHits = new Collider[16];
    private readonly RaycastHit[] oneWayPlatformCastHits = new RaycastHit[16];
    private readonly RaycastHit[] groundCastHits = new RaycastHit[8];
    private Vector3 depthAxis = Vector3.forward;
    private float lockedDepth;
    private Quaternion initialFacingRotation;
    private bool playedDeathAnimation;
    private string requestedAnimationState;
    private float actionAnimationLockUntil;
    private RuntimeAnimatorController defaultAnimatorController;
    private MovementMode appliedAnimatorMode;
    private bool hasAppliedAnimatorController;
    private RuntimeAnimatorController cachedDashingAnimatorController;
    private string cachedAnimatorBoolName;
    private bool cachedHasDashingParameter;
    private Vector3 forcedFree3DFacingDirection = Vector3.right;
    private float forcedFree3DFacingUntil;

    public bool IsGrounded { get; private set; }
    public bool IsOnOneWayPlatform => currentOneWayPlatform != null && Time.time <= currentOneWayPlatformUntil;
    public int FacingSign => facingRight ? 1 : -1;
    public bool UsesSideScrollerMovement => movementMode == MovementMode.SideScroller;
    public bool UsesFree3DMovement => movementMode == MovementMode.Free3D;
    public Vector3 MovementAxis => UsesSideScrollerMovement ? movementAxis : lastMoveDirection;
    public Vector3 GroundCheckPosition => groundCheck != null ? groundCheck.position : transform.TransformPoint(groundCheckLocalOffset);
    public Camera ResolveAimingCamera()
    {
        Transform cameraTransform = ResolveMovementCamera();
        if (cameraTransform != null && cameraTransform.TryGetComponent(out Camera camera))
        {
            return camera;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            return mainCamera;
        }

        return Object.FindFirstObjectByType<Camera>();
    }

    public bool WasRecentlyOnOneWayPlatform(float graceSeconds)
    {
        return Time.time <= lastOneWayPlatformGroundedTime + Mathf.Max(0f, graceSeconds);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        combat = GetComponent<PlayerCombat3D>();
        health = GetComponent<Health>();
        CacheMovementModeAnimatorControllers();
        ApplyMovementModeAnimatorController(true);
        EnsurePlayerEnemyLayerCollision();
        EnsureDashAfterimage();
        initialFacingRotation = transform.rotation;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        NormalizeMovementPlane(true);
        lastMoveDirection = movementAxis;
        dashDirectionVector = lastMoveDirection;
        dashJumpDirectionVector = lastMoveDirection;
        ApplyFacingRotation();
        if (UsesSideScrollerMovement)
        {
            SnapToMovementPlane();
        }
        EnsureGroundCheck();
        EnsureGroundMask();
        CacheSolidPlayerColliders();
        EnsureNoFrictionColliderMaterial();
    }

    private void EnsureDashAfterimage()
    {
        dashAfterimage = GetComponent<PlayerDashAfterimage3D>();
        if (!enableDashAfterimage)
        {
            if (dashAfterimage != null)
            {
                dashAfterimage.enabled = false;
            }

            return;
        }

        if (dashAfterimage == null)
        {
            dashAfterimage = gameObject.AddComponent<PlayerDashAfterimage3D>();
        }

        dashAfterimage.enabled = true;
        dashAfterimage.Configure(
            dashAfterimageVisualRoot,
            dashAfterimageSpawnInterval,
            dashAfterimageLifetime,
            dashAfterimageColor,
            dashAfterimageIncludeMeshRenderers,
            dashAfterimageIncludeInactiveRenderers);
    }

    private static void EnsurePlayerEnemyLayerCollision()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (playerLayer < 0 || enemyLayer < 0)
        {
            return;
        }

        if (Physics.GetIgnoreLayerCollision(playerLayer, enemyLayer))
        {
            Physics.IgnoreLayerCollision(playerLayer, enemyLayer, false);
        }
    }

    private void OnEnable()
    {
        health = health != null ? health : GetComponent<Health>();
        if (health != null)
        {
            health.Died += PlayDeathAnimation;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= PlayDeathAnimation;
        }
    }

    private void Update()
    {
        ApplyMovementModeAnimatorController();
        UpdateDashState();
        UpdateAirAnimation();
        bool inputLocked = IsInputLocked;
        bool movementLocked = inputLocked || IsDashing;
        Vector2 rawMoveInput = movementLocked ? Vector2.zero : ReadMoveInput();
        if (UsesSideScrollerMovement)
        {
            rawMoveInput.y = 0f;
        }

        Vector3 rawMoveDirection = movementLocked ? Vector3.zero : ResolveMoveDirection(rawMoveInput);
        bool forceFreeFacing = UsesFree3DMovement && Time.time < forcedFree3DFacingUntil;

        if (forceFreeFacing)
        {
            FaceFreeDirection(forcedFree3DFacingDirection, true);
        }
        else if (!inputLocked && !IsDashing && rawMoveDirection.sqrMagnitude > 0.0001f)
        {
            Face(rawMoveDirection);
        }

        if (!inputLocked && !IsDashing && WasDashPressed() && CanStartDash())
        {
            StartDash();
            movementLocked = true;
        }

        moveInput = movementLocked ? Vector2.zero : rawMoveInput;
        moveDirection = movementLocked ? Vector3.zero : rawMoveDirection;

        if (WasJumpPressed() && !inputLocked)
        {
            if (!IsDashing && enableOneWayPlatforms && IsDropInputHeld() && TryDropThroughOneWayPlatform())
            {
                jumpQueuedUntil = -1f;
            }
            else
            {
                jumpQueuedUntil = Time.time + jumpBufferSeconds;
            }
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", GetAnimatorMoveSpeed());
            animator.SetBool("Grounded", IsGrounded);
            animator.SetFloat("VerticalSpeed", body.linearVelocity.y);
            SetAnimatorDashing(IsDashing);
        }
    }

    private void FixedUpdate()
    {
        UpdateDashState();
        EnsureGroundCheck();
        EnsureGroundMask();
        UpdateOneWayPlatformPassThrough(body.linearVelocity.y);
        IsGrounded = CheckGrounded();
        if (IsGrounded)
        {
            if (!wasGroundedLastFixedUpdate)
            {
                airDashConsumed = false;
                airJumpsUsed = 0;
                jumpedSinceGrounded = false;
                requestedAnimationState = null;
            }

            lastGroundedTime = Time.time;
            if (knockbackControlLocked && Time.time >= hurtLockedUntil)
            {
                knockbackControlLocked = false;
            }
        }

        if (Time.time > currentOneWayPlatformUntil)
        {
            currentOneWayPlatform = null;
        }

        Vector3 velocity = body.linearVelocity;
        float control = IsGrounded ? 1f : airControl;
        bool dashLocked = IsDashing;
        bool hurtLocked = IsHurtLocked;
        bool attackLocked = IsAttackLocked;
        bool dashJumpQueued = dashLocked
            && Time.time <= jumpQueuedUntil
            && CanUseGroundJump()
            && !hurtLocked
            && !attackLocked;
        bool groundJumpQueued = !dashLocked
            && Time.time <= jumpQueuedUntil
            && CanUseGroundJump()
            && !hurtLocked
            && !attackLocked;
        bool airJumpQueued = !dashLocked
            && Time.time <= jumpQueuedUntil
            && CanUseAirJump()
            && !hurtLocked
            && !attackLocked;

        if (hurtLocked)
        {
            StopDash(false, true);
            dashLocked = false;
            jumpQueuedUntil = -1f;
        }
        else if (attackLocked)
        {
            StopDash(false, true);
            dashLocked = false;
            velocity = SetPlanarVelocity(velocity, Vector3.zero);
            jumpQueuedUntil = -1f;
        }
        else if (dashLocked && !dashJumpQueued)
        {
            if (IsDashDirectionBlocked())
            {
                StopDash(true, true);
                dashLocked = false;
                velocity = SetPlanarVelocity(velocity, Vector3.zero);
            }
            else
            {
                velocity = SetPlanarVelocity(velocity, dashDirectionVector * GetDashSpeed());
                if (flattenVerticalVelocityDuringDash)
                {
                    velocity.y = 0f;
                }
            }

            jumpQueuedUntil = -1f;
        }
        else
        {
            Vector3 inputDirection = preventAirWallSticking && IsPushingIntoWall(moveDirection) ? Vector3.zero : moveDirection;
            Vector3 planarVelocity = inputDirection * moveSpeed * control;
            if (IsDashJumpBoostActive())
            {
                planarVelocity = GetDashJumpPlanarVelocity(planarVelocity);
            }

            velocity = SetPlanarVelocity(velocity, planarVelocity);
        }

        if (groundJumpQueued || dashJumpQueued || airJumpQueued)
        {
            if (dashJumpQueued)
            {
                StopDash(false, true);
                dashLocked = false;
                airDashConsumed = true;
                jumpedSinceGrounded = true;
                dashJumpDirectionVector = dashDirectionVector;
                dashJumpBoostStartedAt = Time.time;
                dashJumpBoostUntil = Time.time + Mathf.Max(0f, dashJumpBoostSeconds);
                velocity = SetPlanarVelocity(velocity, GetDashJumpPlanarVelocity(dashDirectionVector * GetDashSpeed()));
            }
            else if (airJumpQueued)
            {
                airJumpsUsed++;
            }

            if (groundJumpQueued)
            {
                jumpedSinceGrounded = true;
            }

            float activeJumpForce = airJumpQueued ? jumpForce * Mathf.Max(0.1f, airJumpForceMultiplier) : jumpForce;
            velocity.y = Mathf.Max(velocity.y, activeJumpForce);
            jumpQueuedUntil = -1f;
            UpdateOneWayPlatformPassThrough(velocity.y);
            if (animator != null)
            {
                PlayJumpAnimation(JumpUpStateName);
            }
        }

        if (!dashLocked || !flattenVerticalVelocityDuringDash)
        {
            ApplyExtraGravity(ref velocity);
        }

        body.linearVelocity = velocity;

        if (UsesSideScrollerMovement && Mathf.Abs(Vector3.Dot(body.position, depthAxis) - lockedDepth) > 0.001f)
        {
            SnapToMovementPlane();
        }

        wasGroundedLastFixedUpdate = IsGrounded;
    }

    private bool IsHurtLocked => Time.time < hurtLockedUntil || (lockControlUntilKnockbackLands && knockbackControlLocked);
    private bool IsAttackLocked => combat != null && combat.IsAttackLocked;
    private bool IsCornerTurnLocked => Time.time < cornerTurnLockedUntil;
    public bool IsDashing => isDashing && Time.time < dashUntil;
    private bool IsInputLocked => IsDead || IsHurtLocked || IsAttackLocked || IsCornerTurnLocked;
    private bool IsDead => health != null && health.IsDead;

    private bool CanUseGroundJump()
    {
        return !jumpedSinceGrounded && Time.time <= lastGroundedTime + coyoteTimeSeconds;
    }

    private bool CanUseAirJump()
    {
        return jumpedSinceGrounded && airJumpsUsed < Mathf.Max(0, extraAirJumps);
    }

    private bool CanStartDash()
    {
        return Time.time >= nextDashTime
            && dashDistance > 0f
            && dashDuration > 0f
            && (IsGrounded || (allowAirDash && !airDashConsumed));
    }

    private void StartDash()
    {
        dashDirectionVector = ResolveDashDirection();
        FaceImmediate(dashDirectionVector);
        isDashing = true;
        dashStartedGrounded = !IsAirborneForAction();
        dashUntil = Time.time + Mathf.Max(0.01f, dashDuration);
        nextDashTime = Time.time + Mathf.Max(0f, dashCooldown);
        airDashConsumed = !dashStartedGrounded;
        actionAnimationLockUntil = Time.time + (dashStartedGrounded
            ? Mathf.Max(0.01f, dashDuration)
            : Mathf.Max(Mathf.Max(0.01f, dashDuration), Mathf.Max(0f, airDashAnimationMinSeconds)));
        jumpQueuedUntil = -1f;
        SetAnimatorDashing(true);
        PlayActionAnimation(DashStateName);
    }

    private void UpdateDashState()
    {
        if (isDashing && Time.time >= dashUntil)
        {
            StopDash(true, false);
        }
    }

    private void StopDash(bool playGroundEndAnimation, bool clearAnimationLock)
    {
        if (!isDashing)
        {
            return;
        }

        isDashing = false;
        dashUntil = Time.time;
        SetAnimatorDashing(false);

        if (clearAnimationLock)
        {
            actionAnimationLockUntil = Time.time;
        }

        if (playGroundEndAnimation && dashStartedGrounded && !IsDead && !IsAttackLocked && !IsHurtLocked)
        {
            PlayActionAnimation(DashEndStateName);
            actionAnimationLockUntil = Mathf.Max(
                actionAnimationLockUntil,
                Time.time + Mathf.Max(0f, dashEndAnimationMinSeconds));
        }
    }

    private void SetAnimatorDashing(bool value)
    {
        if (animator == null || !HasAnimatorBool(DashingParameterName))
        {
            return;
        }

        animator.SetBool(DashingParameterName, value);
    }

    private bool HasAnimatorBool(string parameterName)
    {
        if (cachedDashingAnimatorController == animator.runtimeAnimatorController
            && cachedAnimatorBoolName == parameterName)
        {
            return cachedHasDashingParameter;
        }

        cachedDashingAnimatorController = animator.runtimeAnimatorController;
        cachedAnimatorBoolName = parameterName;
        cachedHasDashingParameter = false;
        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = parameters[i];
            if (parameter.type == AnimatorControllerParameterType.Bool && parameter.name == parameterName)
            {
                cachedHasDashingParameter = true;
                return true;
            }
        }

        return false;
    }

    private void CacheMovementModeAnimatorControllers()
    {
        if (animator == null)
        {
            return;
        }

        defaultAnimatorController = animator.runtimeAnimatorController;
#if UNITY_EDITOR
        if (sideScrollerAnimatorController == null)
        {
            sideScrollerAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                DefaultSideScrollerAnimatorControllerPath);
        }

        if (free3DAnimatorController == null)
        {
            free3DAnimatorController = UnityEditor.AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                DefaultFree3DAnimatorControllerPath);
        }
#endif
        if (sideScrollerAnimatorController == null)
        {
            sideScrollerAnimatorController = defaultAnimatorController;
        }

        if (free3DAnimatorController == null)
        {
            free3DAnimatorController = defaultAnimatorController;
        }
    }

    private void ApplyMovementModeAnimatorController(bool force = false)
    {
        if (animator == null)
        {
            return;
        }

        MovementMode targetMode = movementMode;
        RuntimeAnimatorController targetController = targetMode == MovementMode.Free3D
            ? free3DAnimatorController
            : sideScrollerAnimatorController;

        if (targetController == null)
        {
            targetController = defaultAnimatorController != null
                ? defaultAnimatorController
                : animator.runtimeAnimatorController;
        }

        if (targetController == null)
        {
            return;
        }

        if (!force
            && hasAppliedAnimatorController
            && appliedAnimatorMode == targetMode
            && animator.runtimeAnimatorController == targetController)
        {
            return;
        }

        if (animator.runtimeAnimatorController != targetController)
        {
            animator.runtimeAnimatorController = targetController;
            requestedAnimationState = null;
            ResetAnimatorParameterCache();
        }

        appliedAnimatorMode = targetMode;
        hasAppliedAnimatorController = true;
    }

    private void ResetAnimatorParameterCache()
    {
        cachedDashingAnimatorController = null;
        cachedAnimatorBoolName = null;
        cachedHasDashingParameter = false;
    }

    private bool IsDashDirectionBlocked()
    {
        if (body == null)
        {
            return false;
        }

        Vector3 dashAxis = dashDirectionVector;
        float castDistance = Mathf.Max(0.01f, GetDashSpeed() * Time.fixedDeltaTime) + DashBlockCastSkin;
        RaycastHit[] hits = body.SweepTestAll(dashAxis, castDistance, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            RaycastHit hit = hits[i];
            if (IsDashBlockingHit(hit, dashAxis))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsDashBlockingHit(RaycastHit hit, Vector3 dashAxis)
    {
        Collider hitCollider = hit.collider;
        if (hitCollider == null || hitCollider.isTrigger || hitCollider.transform.IsChildOf(transform))
        {
            return false;
        }

        if (hitCollider.GetComponentInParent<OneWayPlatform3D>() != null)
        {
            return true;
        }

        if (!IsUsableGround(hitCollider))
        {
            return false;
        }

        return Vector3.Dot(hit.normal, dashAxis) <= -wallNormalThreshold;
    }

    private bool IsDashBlockingContact(ContactPoint contact)
    {
        Vector3 dashAxis = dashDirectionVector;
        return contact.normal.y < 0.45f && Vector3.Dot(contact.normal, dashAxis) <= -wallNormalThreshold;
    }

    private bool IsDashBlockingCollision(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (IsDashBlockingContact(collision.GetContact(i)))
            {
                return true;
            }
        }

        return false;
    }

    private float GetDashSpeed()
    {
        return dashDistance / Mathf.Max(0.01f, dashDuration);
    }

    private float GetDashJumpSpeed()
    {
        float runUpSpeed = moveSpeed * Mathf.Max(1f, dashJumpHorizontalMultiplier);
        float carriedDashSpeed = GetDashSpeed() * Mathf.Clamp01(dashJumpDashSpeedCarryMultiplier);
        return Mathf.Max(runUpSpeed, carriedDashSpeed);
    }

    private Vector3 GetDashJumpPlanarVelocity(Vector3 currentPlanarVelocity)
    {
        float boostDuration = Mathf.Max(0.01f, dashJumpBoostSeconds);
        float boostProgress = Mathf.Clamp01((Time.time - dashJumpBoostStartedAt) / boostDuration);
        float boostSpeed = Mathf.Lerp(GetDashJumpSpeed(), moveSpeed, boostProgress);
        Vector3 boostVelocity = dashJumpDirectionVector * boostSpeed;

        if (Vector3.Dot(currentPlanarVelocity, dashJumpDirectionVector) > boostSpeed)
        {
            return currentPlanarVelocity;
        }

        return boostVelocity;
    }

    private bool IsDashJumpBoostActive()
    {
        return Time.time < dashJumpBoostUntil;
    }

    private void UpdateAirAnimation()
    {
        if (animator == null || IsDead || IsDashing || IsGrounded || IsAttackLocked)
        {
            return;
        }

        if (requestedAnimationState == DashStateName && Time.time < actionAnimationLockUntil)
        {
            return;
        }

        PlayAirborneAnimation();
    }

    private void PlayDeathAnimation()
    {
        if (playedDeathAnimation)
        {
            return;
        }

        playedDeathAnimation = true;
        StopDash(false, true);
        jumpQueuedUntil = -1f;
        PlayActionAnimation(DeathStateName);
    }

    private void PlayJumpAnimation(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            return;
        }

        if (requestedAnimationState != stateName)
        {
            requestedAnimationState = stateName;
            animator.CrossFadeInFixedTime(stateName, Mathf.Max(0f, jumpAnimationCrossFadeSeconds), 0, 0f);
        }
    }

    private void PlayActionAnimation(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            return;
        }

        requestedAnimationState = stateName;
        animator.CrossFadeInFixedTime(stateName, Mathf.Max(0f, actionAnimationCrossFadeSeconds), 0, 0f);
    }

    private void PlayAirborneAnimation()
    {
        string stateName = body != null && body.linearVelocity.y < -0.05f ? JumpDownStateName : JumpUpStateName;
        PlayJumpAnimation(stateName);
    }

    private bool IsAirborneForAction()
    {
        if (!IsGrounded || jumpedSinceGrounded)
        {
            return true;
        }

        return body != null && Mathf.Abs(body.linearVelocity.y) > 0.1f;
    }

    private float GetAnimatorMoveSpeed()
    {
        return IsDashing ? 1f : Mathf.Clamp01(moveInput.magnitude);
    }

    private void SnapToMovementPlane()
    {
        Vector3 position = body.position;
        float depth = Vector3.Dot(position, depthAxis);
        position += depthAxis * (lockedDepth - depth);
        body.position = position;
    }

    private Vector3 SetPlanarVelocity(Vector3 velocity, Vector3 planarVelocity)
    {
        return FlattenHorizontalOrZero(planarVelocity) + Vector3.up * velocity.y;
    }

    private bool CheckGrounded()
    {
        if (jumpedSinceGrounded && body != null && body.linearVelocity.y > 0.1f)
        {
            return false;
        }

        Vector3 checkPosition = groundCheck != null ? groundCheck.position : transform.TransformPoint(groundCheckLocalOffset);
        int overlapCount = Physics.OverlapSphereNonAlloc(checkPosition, groundCheckRadius, groundOverlapHits, groundMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < overlapCount; i++)
        {
            Collider hitCollider = groundOverlapHits[i];
            if (IsUsableGround(hitCollider) && IsStandableGround(hitCollider))
            {
                return true;
            }
        }

        if (Time.time <= collisionGroundedUntil)
        {
            return true;
        }

        if (!useAnySolidGroundFallback)
        {
            return false;
        }

        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float castRadius = Mathf.Max(0.08f, groundCheckRadius);
        float castDistance = Mathf.Abs(groundCheckLocalOffset.y) + groundFallbackDistance;
        int hitCount = Physics.SphereCastNonAlloc(origin, castRadius, Vector3.down, groundCastHits, castDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = groundCastHits[i].collider;
            if (IsUsableGround(hitCollider) && IsStandableGround(hitCollider))
            {
                return true;
            }
        }

        return false;
    }

    private void UpdateOneWayPlatformPassThrough(float verticalVelocity)
    {
        if (!enableOneWayPlatforms)
        {
            return;
        }

        Collider actorCollider = GetBodyCollider();
        if (actorCollider == null)
        {
            return;
        }

        Bounds bounds = actorCollider.bounds;
        float precheckHeight = Mathf.Max(0.1f, oneWayPlatformPrecheckHeight);
        float padding = Mathf.Max(0f, oneWayPlatformPrecheckPadding);
        Vector3 center = bounds.center + Vector3.up * (precheckHeight * 0.5f);
        Vector3 halfExtents = new Vector3(
            Mathf.Max(bounds.extents.x + padding, groundCheckRadius),
            bounds.extents.y + precheckHeight * 0.5f,
            Mathf.Max(bounds.extents.z + padding, 0.1f));

        int mask = groundMask.value != 0 ? groundMask.value : Physics.DefaultRaycastLayers;
        int hitCount = Physics.OverlapBoxNonAlloc(center, halfExtents, oneWayPlatformHits, Quaternion.identity, mask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hitCount; i++)
        {
            TryPrepareOneWayPlatformPassThrough(oneWayPlatformHits[i], actorCollider, verticalVelocity);
        }

        if (verticalVelocity <= 0f)
        {
            return;
        }

        Vector3 castHalfExtents = new Vector3(
            Mathf.Max(bounds.extents.x + padding, groundCheckRadius),
            Mathf.Max(bounds.extents.y, 0.02f),
            Mathf.Max(bounds.extents.z + padding, 0.1f));
        float castDistance = Mathf.Max(precheckHeight, verticalVelocity * Time.fixedDeltaTime + precheckHeight * 0.25f);
        int castHitCount = Physics.BoxCastNonAlloc(bounds.center, castHalfExtents, Vector3.up, oneWayPlatformCastHits, Quaternion.identity, castDistance, mask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < castHitCount; i++)
        {
            TryPrepareOneWayPlatformPassThrough(oneWayPlatformCastHits[i].collider, actorCollider, verticalVelocity);
        }
    }

    private void TryPrepareOneWayPlatformPassThrough(Collider hitCollider, Collider actorCollider, float verticalVelocity)
    {
        if (!IsUsableGround(hitCollider))
        {
            return;
        }

        OneWayPlatform3D oneWayPlatform = hitCollider.GetComponentInParent<OneWayPlatform3D>();
        if (oneWayPlatform == null)
        {
            return;
        }

        if (oneWayPlatform.ShouldPreparePassThroughFromBelow(actorCollider, verticalVelocity))
        {
            oneWayPlatform.IgnoreCollidersUntilAbove(GetSolidPlayerColliders(), oneWayPlatform.UpwardPassThroughSeconds);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    private void HandleCollision(Collision collision)
    {
        OneWayPlatform3D oneWayPlatform = enableOneWayPlatforms
            ? collision.collider.GetComponentInParent<OneWayPlatform3D>()
            : null;

        if (IsDashing && oneWayPlatform != null && IsDashBlockingCollision(collision))
        {
            StopDash(true, true);
            body.linearVelocity = SetPlanarVelocity(body.linearVelocity, Vector3.zero);
            return;
        }

        if (!IsUsableGround(collision.collider))
        {
            return;
        }

        if (oneWayPlatform != null && oneWayPlatform.ShouldPassThroughFromBelow(GetBodyCollider(), body.linearVelocity.y))
        {
            oneWayPlatform.IgnoreCollidersUntilAbove(GetSolidPlayerColliders(), oneWayPlatform.UpwardPassThroughSeconds);
            collisionGroundedUntil = -1f;
            return;
        }

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            if (contact.normal.y > 0.45f)
            {
                if (oneWayPlatform == null || oneWayPlatform.CanStandOn(GetBodyCollider()))
                {
                    collisionGroundedUntil = Time.time + 0.08f;
                    if (oneWayPlatform != null)
                    {
                        currentOneWayPlatform = oneWayPlatform;
                        currentOneWayPlatformUntil = Time.time + 0.16f;
                        lastOneWayPlatformGroundedTime = Time.time;
                    }

                    return;
                }

                return;
            }

            if (preventAirWallSticking
                && contact.normal.y < 0.45f)
            {
                Vector3 planarNormal = FlattenHorizontalOrZero(contact.normal);
                if (planarNormal.sqrMagnitude > 0.0001f)
                {
                    blockedWallNormal = planarNormal;
                    wallContactUntil = Time.time + wallContactGraceSeconds;
                }
            }
        }
    }

    private bool IsPushingIntoWall(Vector3 inputDirection)
    {
        if (IsGrounded || inputDirection.sqrMagnitude < 0.0001f || Time.time > wallContactUntil)
        {
            return false;
        }

        return Vector3.Dot(inputDirection, blockedWallNormal) <= -wallNormalThreshold;
    }

    private bool IsUsableGround(Collider target)
    {
        if (target == null || target.isTrigger || target.transform.IsChildOf(transform))
        {
            return false;
        }

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int targetLayer = target.gameObject.layer;

        if (targetLayer == playerLayer || targetLayer == enemyLayer)
        {
            return false;
        }

        return true;
    }

    private bool IsStandableGround(Collider target)
    {
        if (!enableOneWayPlatforms)
        {
            return true;
        }

        OneWayPlatform3D oneWayPlatform = target.GetComponentInParent<OneWayPlatform3D>();
        if (oneWayPlatform == null)
        {
            return true;
        }

        return !oneWayPlatform.IsIgnoringAny(GetSolidPlayerColliders()) && oneWayPlatform.CanStandOn(GetBodyCollider());
    }

    private void EnsureGroundCheck()
    {
        if (groundCheck != null)
        {
            return;
        }

        Transform existing = transform.Find("GroundCheck");
        if (existing != null)
        {
            groundCheck = existing;
            return;
        }

        GameObject checkObject = new GameObject("GroundCheck");
        checkObject.transform.SetParent(transform, false);
        checkObject.transform.localPosition = groundCheckLocalOffset;
        groundCheck = checkObject.transform;
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

    private void EnsureNoFrictionColliderMaterial()
    {
        if (!useNoFrictionColliderMaterial)
        {
            return;
        }

        foreach (Collider targetCollider in GetSolidPlayerColliders())
        {
            targetCollider.material = GetNoFrictionMaterial();
        }
    }

    private bool TryDropThroughOneWayPlatform()
    {
        OneWayPlatform3D targetPlatform = currentOneWayPlatform != null
            ? currentOneWayPlatform
            : FindOneWayPlatformBelow();

        if (targetPlatform == null)
        {
            return false;
        }

        float ignoreSeconds = Mathf.Max(dropThroughSeconds, targetPlatform.DropThroughSeconds);
        targetPlatform.IgnoreCollidersUntilBelow(GetSolidPlayerColliders(), ignoreSeconds);
        currentOneWayPlatform = null;
        currentOneWayPlatformUntil = -1f;
        lastOneWayPlatformGroundedTime = -999f;
        collisionGroundedUntil = -1f;
        lastGroundedTime = -999f;
        IsGrounded = false;

        Vector3 velocity = body.linearVelocity;
        velocity.y = Mathf.Min(velocity.y, -Mathf.Abs(dropThroughStartSpeed));
        body.linearVelocity = velocity;
        return true;
    }

    private OneWayPlatform3D FindOneWayPlatformBelow()
    {
        Vector3 origin = GroundCheckPosition + Vector3.up * 0.05f;
        float castRadius = Mathf.Max(0.08f, groundCheckRadius);
        float castDistance = Mathf.Max(0.02f, dropThroughPlatformSearchDistance);
        int hitCount = Physics.SphereCastNonAlloc(origin, castRadius, Vector3.down, groundCastHits, castDistance, groundMask, QueryTriggerInteraction.Ignore);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = groundCastHits[i].collider;
            if (!IsUsableGround(hitCollider))
            {
                continue;
            }

            OneWayPlatform3D oneWayPlatform = hitCollider.GetComponentInParent<OneWayPlatform3D>();
            if (oneWayPlatform != null && oneWayPlatform.CanStandOn(GetBodyCollider()))
            {
                return oneWayPlatform;
            }
        }

        return null;
    }

    private Collider GetBodyCollider()
    {
        if (bodyCollider == null)
        {
            CacheSolidPlayerColliders();
        }

        return bodyCollider;
    }

    private Collider[] GetSolidPlayerColliders()
    {
        if (solidPlayerColliders == null || solidPlayerColliders.Length == 0)
        {
            CacheSolidPlayerColliders();
        }

        return solidPlayerColliders;
    }

    private void CacheSolidPlayerColliders()
    {
        List<Collider> colliders = new List<Collider>();
        foreach (Collider targetCollider in GetComponentsInChildren<Collider>(true))
        {
            if (targetCollider == null || targetCollider.isTrigger)
            {
                continue;
            }

            if (groundCheck != null && targetCollider.transform.IsChildOf(groundCheck))
            {
                continue;
            }

            colliders.Add(targetCollider);
            if (bodyCollider == null || targetCollider.attachedRigidbody == body)
            {
                bodyCollider = targetCollider;
            }
        }

        solidPlayerColliders = colliders.ToArray();
    }

    private PhysicsMaterial GetNoFrictionMaterial()
    {
        if (noFrictionMaterial != null)
        {
            return noFrictionMaterial;
        }

        noFrictionMaterial = new PhysicsMaterial("Player_No_Friction")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            bounciness = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };

        return noFrictionMaterial;
    }

    public void ApplyCornerTurn(float yawDegrees, float lockSeconds)
    {
        if (!UsesSideScrollerMovement)
        {
            return;
        }

        Quaternion rotation = Quaternion.AngleAxis(yawDegrees, Vector3.up);
        movementAxis = FlattenHorizontal(rotation * movementAxis);
        NormalizeMovementPlane(false);
        ApplyFacingRotation();
        cornerTurnLockedUntil = Mathf.Max(cornerTurnLockedUntil, Time.time + Mathf.Max(0f, lockSeconds));
        body.linearVelocity = SetPlanarVelocity(body.linearVelocity, Vector3.zero);
        SnapToMovementPlane();
    }

    public void LockForCornerTurn(float lockSeconds)
    {
        if (!UsesSideScrollerMovement)
        {
            return;
        }

        cornerTurnLockedUntil = Mathf.Max(cornerTurnLockedUntil, Time.time + Mathf.Max(0f, lockSeconds));
        body.linearVelocity = SetPlanarVelocity(body.linearVelocity, Vector3.zero);
    }

    private void NormalizeMovementPlane(bool useInitialLockedZ)
    {
        movementAxis = FlattenHorizontal(movementAxis);
        depthAxis = FlattenHorizontal(Vector3.Cross(movementAxis, Vector3.up));

        if (useInitialLockedZ)
        {
            lockedDepth = lockedZ;
        }
        else
        {
            lockedDepth = Vector3.Dot(body.position, depthAxis);
        }
    }

    private static Vector3 FlattenHorizontal(Vector3 value)
    {
        value.y = 0f;
        if (value.sqrMagnitude < 0.0001f)
        {
            return Vector3.right;
        }

        return value.normalized;
    }

    private static Vector3 FlattenHorizontalOrZero(Vector3 value)
    {
        value.y = 0f;
        return value.sqrMagnitude < 0.0001f ? Vector3.zero : value.normalized * value.magnitude;
    }

    public void ApplyKnockback(Vector3 damageSourcePosition)
    {
        if (!enableDamageKnockback)
        {
            if (TryGetComponent(out PlayerDamageFlash disabledKnockbackDamageFlash))
            {
                disabledKnockbackDamageFlash.PlayFlash();
            }

            return;
        }

        Vector3 knockbackDirection = ResolveDamageDirection(damageSourcePosition);
        body.linearVelocity = knockbackDirection * knockbackForce.x + Vector3.up * knockbackForce.y;
        hurtLockedUntil = Time.time + Mathf.Max(knockbackControlLockSeconds, 0f);
        knockbackControlLocked = true;

        if (TryGetComponent(out PlayerDamageFlash damageFlash))
        {
            damageFlash.PlayFlash();
        }
    }

    private void ApplyExtraGravity(ref Vector3 velocity)
    {
        if (IsGrounded)
        {
            return;
        }

        float multiplier = velocity.y < 0f ? fallGravityMultiplier : upwardGravityMultiplier;
        if (multiplier > 1f)
        {
            velocity += Physics.gravity * ((multiplier - 1f) * Time.fixedDeltaTime);
        }

        if (velocity.y < -maxFallSpeed)
        {
            velocity.y = -maxFallSpeed;
        }
    }

    public void ApplyHurtNudge(Vector3 damageSourcePosition, float horizontalSpeed, float lockSeconds)
    {
        Vector3 hurtDirection = ResolveDamageDirection(damageSourcePosition);
        Vector3 velocity = body.linearVelocity;
        velocity = SetPlanarVelocity(velocity, hurtDirection * horizontalSpeed);
        body.linearVelocity = velocity;
        hurtLockedUntil = Time.time + lockSeconds;

        if (TryGetComponent(out PlayerDamageFlash damageFlash))
        {
            damageFlash.PlayFlash();
        }
    }

    private void Face(bool right)
    {
        facingRight = right;
        lastMoveDirection = movementAxis * (right ? 1f : -1f);
        ApplyFacingRotation();
    }

    private void Face(Vector3 direction)
    {
        FaceFreeDirection(direction, false);
    }

    private void FaceImmediate(Vector3 direction)
    {
        if (UsesSideScrollerMovement)
        {
            Face(Vector3.Dot(direction, movementAxis) >= 0f);
            return;
        }

        FaceFreeDirection(direction, true);
    }

    private void FaceFreeDirection(Vector3 direction, bool immediate)
    {
        direction = FlattenHorizontal(direction);
        if (UsesSideScrollerMovement)
        {
            Face(Vector3.Dot(direction, movementAxis) >= 0f);
            return;
        }

        lastMoveDirection = direction;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x);
        transform.localScale = scale;

        Quaternion targetRotation = Quaternion.FromToRotation(Vector3.right, direction) * initialFacingRotation;
        float maxDegreesDelta = Mathf.Max(0f, freeTurnSpeed) * Time.deltaTime;
        transform.rotation = !immediate && maxDegreesDelta > 0f
            ? Quaternion.RotateTowards(transform.rotation, targetRotation, maxDegreesDelta)
            : targetRotation;
    }

    public void FaceTowardWorldPoint(Vector3 worldPoint, bool immediate = true)
    {
        Vector3 direction = worldPoint - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        forcedFree3DFacingDirection = FlattenHorizontal(direction);
        forcedFree3DFacingUntil = Time.time + 0.18f;
        FaceFreeDirection(direction, immediate);
    }

    private void ApplyFacingRotation()
    {
        if (!UsesSideScrollerMovement)
        {
            Face(lastMoveDirection);
            return;
        }

        transform.rotation = Quaternion.FromToRotation(Vector3.right, movementAxis) * initialFacingRotation;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 checkPosition = groundCheck != null ? groundCheck.position : transform.TransformPoint(groundCheckLocalOffset);
        Gizmos.DrawWireSphere(checkPosition, groundCheckRadius);
    }

    private Vector3 ResolveMoveDirection(Vector2 input)
    {
        input = Vector2.ClampMagnitude(input, 1f);
        if (input.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        if (UsesSideScrollerMovement)
        {
            return Mathf.Abs(input.x) < 0.01f
                ? Vector3.zero
                : movementAxis * Mathf.Clamp(input.x, -1f, 1f);
        }

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;
        if (useCameraRelativeMovement)
        {
            Transform cameraTransform = ResolveMovementCamera();
            if (cameraTransform != null)
            {
                forward = FlattenHorizontalOrZero(cameraTransform.forward);
                right = FlattenHorizontalOrZero(cameraTransform.right);
            }
        }

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        if (right.sqrMagnitude < 0.0001f)
        {
            right = Vector3.right;
        }

        Vector3 direction = right.normalized * input.x + forward.normalized * input.y;
        return direction.sqrMagnitude < 0.0001f ? Vector3.zero : Vector3.ClampMagnitude(direction, 1f);
    }

    private Transform ResolveMovementCamera()
    {
        if (movementCamera != null)
        {
            return movementCamera;
        }

        Camera mainCamera = Camera.main;
        return mainCamera != null ? mainCamera.transform : null;
    }

    private Vector3 ResolveDashDirection()
    {
        if (UsesSideScrollerMovement)
        {
            return movementAxis * FacingSign;
        }

        Vector3 currentFacingDirection = FlattenHorizontalOrZero(transform.TransformDirection(Vector3.right));
        if (currentFacingDirection.sqrMagnitude > 0.0001f)
        {
            return currentFacingDirection.normalized;
        }

        return lastMoveDirection.sqrMagnitude > 0.0001f ? FlattenHorizontal(lastMoveDirection) : Vector3.right;
    }

    private Vector3 ResolveDamageDirection(Vector3 damageSourcePosition)
    {
        if (UsesSideScrollerMovement)
        {
            float direction = Vector3.Dot(transform.position - damageSourcePosition, movementAxis) >= 0f ? 1f : -1f;
            return movementAxis * direction;
        }

        Vector3 directionFromSource = FlattenHorizontalOrZero(transform.position - damageSourcePosition);
        if (directionFromSource.sqrMagnitude > 0.0001f)
        {
            return directionFromSource.normalized;
        }

        return lastMoveDirection.sqrMagnitude > 0.0001f ? -lastMoveDirection.normalized : -transform.right;
    }

    private Vector2 ReadMoveInput()
    {
#if ENABLE_INPUT_SYSTEM
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                input.x -= 1f;
            }

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                input.x += 1f;
            }

            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            {
                input.y -= 1f;
            }

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            {
                input.y += 1f;
            }
        }

        if (Gamepad.current != null)
        {
            input += Gamepad.current.leftStick.ReadValue();
        }

        return Vector2.ClampMagnitude(input, 1f);
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Vector2.ClampMagnitude(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")), 1f);
#else
        return Vector2.zero;
#endif
    }

    private bool WasJumpPressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool keyboardPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (UsesSideScrollerMovement && Keyboard.current != null)
        {
            keyboardPressed = keyboardPressed
                || Keyboard.current.wKey.wasPressedThisFrame
                || Keyboard.current.upArrowKey.wasPressedThisFrame;
        }

        bool gamepadPressed = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
        return keyboardPressed || gamepadPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Jump")
            || (UsesSideScrollerMovement && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)));
#else
        return false;
#endif
    }

    private bool WasDashPressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool keyboardPressed = Keyboard.current != null
            && (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame);
        bool gamepadPressed = Gamepad.current != null
            && (Gamepad.current.leftStickButton.wasPressedThisFrame || Gamepad.current.rightShoulder.wasPressedThisFrame);
        return keyboardPressed || gamepadPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.LeftShift)
            || Input.GetKeyDown(KeyCode.RightShift)
            || Input.GetKeyDown(KeyCode.JoystickButton5)
            || Input.GetKeyDown(KeyCode.JoystickButton8);
#else
        return false;
#endif
    }

    private bool IsDropInputHeld()
    {
#if ENABLE_INPUT_SYSTEM
        bool keyboardHeld = Keyboard.current != null
            && (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed);
        bool gamepadHeld = Gamepad.current != null
            && (Gamepad.current.dpad.down.isPressed || Gamepad.current.leftStick.ReadValue().y <= -dropInputThreshold);
        return keyboardHeld || gamepadHeld;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetAxisRaw("Vertical") <= -dropInputThreshold
            || Input.GetKey(KeyCode.S)
            || Input.GetKey(KeyCode.DownArrow);
#else
        return false;
#endif
    }
}
