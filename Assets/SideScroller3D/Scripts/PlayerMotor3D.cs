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

    private enum MovementMode
    {
        Free3D,
        SideScroller
    }

    [Header("移動設定")]
    [SerializeField] private MovementMode movementMode = MovementMode.Free3D;

    [Tooltip("角色水平移動速度。")]
    [SerializeField] private float moveSpeed = 6f;

    [Tooltip("空中可控制移動的比例。0 代表空中不能轉向，1 代表和地面一樣好控制。")]
    [SerializeField] private float airControl = 0.65f;

    [Tooltip("開啟後，WASD 會依照相機水平朝向移動。")]
    [SerializeField] private bool useCameraRelativeMovement = true;

    [Tooltip("指定用來計算移動方向的相機。留空時會使用 Main Camera。")]
    [SerializeField] private Transform movementCamera;

    [Tooltip("Free3D 模式下，角色轉向移動方向的速度。0 代表立即轉向。")]
    [SerializeField] private float freeTurnSpeed = 720f;

    [Header("橫向卷軸限制")]
    [Tooltip("只有 SideScroller 模式會使用，用來把角色鎖在固定深度平面。")]
    [SerializeField] private float lockedZ = 0f;

    [Tooltip("只有 SideScroller 模式會使用，用來指定舊橫向卷軸的移動軸。")]
    [SerializeField] private Vector3 movementAxis = Vector3.right;

    [Header("跳躍設定")]
    [Tooltip("角色跳躍初速度。數值越高跳得越高。")]
    [SerializeField] private float jumpForce = 8f;

    [Tooltip("上升時額外重力倍率。數值越高，跳躍上升時間越短。")]
    [SerializeField] private float upwardGravityMultiplier = 1.2f;

    [Tooltip("下落時額外重力倍率。數值越高，落下越快。")]
    [SerializeField] private float fallGravityMultiplier = 2.6f;

    [Tooltip("最大下落速度。避免角色下墜過快。")]
    [SerializeField] private float maxFallSpeed = 18f;

    [Tooltip("提前按跳躍時的緩衝時間。")]
    [SerializeField] private float jumpBufferSeconds = 0.12f;

    [Tooltip("離開地面後仍允許跳躍的寬容時間。")]
    [SerializeField] private float coyoteTimeSeconds = 0.08f;

    [Tooltip("離地後可額外跳躍的次數。1 代表可二段跳。")]
    [SerializeField] private int extraAirJumps = 1;

    [Tooltip("二段跳的垂直力道倍率。1 代表和普通跳一樣高。")]
    [SerializeField] private float airJumpForceMultiplier = 1f;

    [Header("衝刺設定")]
    [Tooltip("按下衝刺鍵時，角色往面向方向快速位移的距離。")]
    [SerializeField] private float dashDistance = 4f;

    [Tooltip("衝刺位移完成所需時間。數值越小，衝刺越瞬間。")]
    [SerializeField] private float dashDuration = 0.12f;

    [Tooltip("兩次衝刺之間的冷卻時間。")]
    [SerializeField] private float dashCooldown = 0.45f;

    [Tooltip("開啟後，角色在空中也可以衝刺。")]
    [SerializeField] private bool allowAirDash = true;

    [Tooltip("開啟後，衝刺期間會暫停垂直速度，讓角色筆直往前衝。")]
    [SerializeField] private bool flattenVerticalVelocityDuringDash = true;

    [Tooltip("衝刺中起跳時，水平跳躍距離的速度倍率。")]
    [SerializeField] private float dashJumpHorizontalMultiplier = 1.8f;

    [Tooltip("衝刺中起跳時，至少保留多少比例的衝刺速度作為助跑動量。")]
    [SerializeField] private float dashJumpDashSpeedCarryMultiplier = 0.85f;

    [Tooltip("衝刺跳躍後，保留向前動量的時間。")]
    [SerializeField] private float dashJumpBoostSeconds = 0.35f;

    [Tooltip("空中衝刺時，Dash 動作至少保留的時間，避免 dash 位移太短時立刻被 Jump_Down 蓋掉。")]
    [SerializeField] private float airDashAnimationMinSeconds = 0.18f;

    [Tooltip("Dash_End 至少保留的時間，避免衝刺結束動作太快被其他狀態蓋掉。")]
    [SerializeField] private float dashEndAnimationMinSeconds = 0.12f;

    [Header("衝刺殘影")]
    [SerializeField] private bool enableDashAfterimage = true;
    [SerializeField] private Transform dashAfterimageVisualRoot;
    [SerializeField] private float dashAfterimageSpawnInterval = 0.035f;
    [SerializeField] private float dashAfterimageLifetime = 0.18f;
    [SerializeField] private Color dashAfterimageColor = new Color(0.25f, 0.55f, 1f, 0.45f);
    [SerializeField] private bool dashAfterimageIncludeMeshRenderers = true;
    [SerializeField] private bool dashAfterimageIncludeInactiveRenderers;

    [Header("動畫設定")]
    [Tooltip("腳本切換 Dash、Dash_End、Death 等動作 state 時的淡入時間。跳躍動作使用下方獨立設定。")]
    [SerializeField] private float actionAnimationCrossFadeSeconds = 0.04f;

    [Tooltip("Jump_Up 和 Jump_Down 之間的淡入時間。數值越大，跳躍上升切到下落越柔和。")]
    [SerializeField] private float jumpAnimationCrossFadeSeconds = 0.12f;

    [Header("地面偵測")]
    [Tooltip("用來判斷角色是否站在地面的定位點。")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("沒有指定 Ground Check 時，自動建立的本地偏移位置。")]
    [SerializeField] private Vector3 groundCheckLocalOffset = new Vector3(0f, -1.05f, 0f);

    [Tooltip("地面偵測球體半徑。")]
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Tooltip("哪些 Layer 會被視為地面。")]
    [SerializeField] private LayerMask groundMask;

    [Tooltip("備用地面偵測距離，用來降低落地判斷失效的機率。")]
    [SerializeField] private float groundFallbackDistance = 0.35f;

    [Tooltip("開啟後，沒有設定 Ground Layer 時會用任何可站立實體作為備用地面。")]
    [SerializeField] private bool useAnySolidGroundFallback = true;

    [Header("單向跳板")]
    [Tooltip("開啟後，可由下往上穿越 OneWayPlatform3D，站上去後按下加跳躍可往下穿越。")]
    [SerializeField] private bool enableOneWayPlatforms = true;

    [Tooltip("下跳時，暫時忽略單向跳板碰撞的時間。")]
    [SerializeField] private float dropThroughSeconds = 0.45f;

    [Tooltip("下跳時給角色的起始下落速度，讓角色更快離開跳板碰撞範圍。")]
    [SerializeField] private float dropThroughStartSpeed = 1.2f;

    [Tooltip("沒有記錄目前跳板時，只會搜尋腳下這段距離內的單向跳板，避免誤忽略下層跳板。")]
    [SerializeField] private float dropThroughPlatformSearchDistance = 0.18f;

    [Tooltip("手把或鍵盤垂直軸低於這個值時，會視為按住下方向。")]
    [SerializeField] private float dropInputThreshold = 0.45f;

    [Tooltip("往上穿越跳板時，玩家周圍提前偵測單向跳板的高度。")]
    [SerializeField] private float oneWayPlatformPrecheckHeight = 1.6f;

    [Tooltip("往上穿越跳板時，玩家周圍提前偵測單向跳板的水平外擴範圍。")]
    [SerializeField] private float oneWayPlatformPrecheckPadding = 0.12f;

    [Header("牆面滑落")]
    [Tooltip("開啟後，角色空中貼到牆時會停止持續往牆內推，避免卡在牆上。")]
    [SerializeField] private bool preventAirWallSticking = true;

    [Tooltip("側面碰撞法線大於這個數值時，會被視為牆面。")]
    [SerializeField] private float wallNormalThreshold = 0.55f;

    [Tooltip("離開牆面後，牆面阻擋狀態保留的短暫時間。")]
    [SerializeField] private float wallContactGraceSeconds = 0.08f;

    [Tooltip("開啟後，會自動讓角色主 Collider 使用無摩擦材質，減少貼牆卡住。")]
    [SerializeField] private bool useNoFrictionColliderMaterial = true;

    [Header("受傷彈飛")]
    [Tooltip("開啟後，玩家受傷時會被彈飛並短暫鎖定操作。關閉後只播放受傷閃爍，不改變移動速度。")]
    [SerializeField] private bool enableDamageKnockback = true;

    [Tooltip("玩家碰到敵人時的彈飛力道。X 是水平彈開速度，Y 是往上彈起速度。")]
    [FormerlySerializedAs("knockback")]
    [SerializeField] private Vector2 knockbackForce = new Vector2(8f, 4f);

    [Tooltip("彈飛後玩家不能操作角色的最短時間。")]
    [SerializeField] private float knockbackControlLockSeconds = 0.45f;

    [Tooltip("開啟後，如果玩家還在空中，會持續鎖住操作直到落地。")]
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
    private RuntimeAnimatorController cachedDashingAnimatorController;
    private string cachedAnimatorBoolName;
    private bool cachedHasDashingParameter;

    public bool IsGrounded { get; private set; }
    public bool IsOnOneWayPlatform => currentOneWayPlatform != null && Time.time <= currentOneWayPlatformUntil;
    public int FacingSign => facingRight ? 1 : -1;
    public bool UsesSideScrollerMovement => movementMode == MovementMode.SideScroller;
    public Vector3 MovementAxis => UsesSideScrollerMovement ? movementAxis : lastMoveDirection;
    public Vector3 GroundCheckPosition => groundCheck != null ? groundCheck.position : transform.TransformPoint(groundCheckLocalOffset);

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

        if (!inputLocked && !IsDashing && rawMoveDirection.sqrMagnitude > 0.0001f)
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
