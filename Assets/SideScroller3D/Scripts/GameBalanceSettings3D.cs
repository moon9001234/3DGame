using System;
using UnityEngine;

public class GameBalanceSettings3D : MonoBehaviour
{
    public enum BalanceMovementMode
    {
        Free3D,
        SideScroller
    }

    public enum BalanceAttackMode
    {
        Melee,
        Ranged,
        Boss
    }

    [Header("Scan")]
    [InspectorName("\u8b80\u53d6\u505c\u7528\u4e2d\u7684\u7269\u4ef6")]
    public bool includeInactiveObjects = true;

    [InspectorName("\u6575\u4eba\u540d\u7a31")]
    public string[] enemyNameKeys = { "Enemy_A", "Enemy_B" };

    [InspectorName("\u6b66\u5668\u6839\u7269\u4ef6\u540d\u7a31")]
    public string playerWeaponRootName = "Player_Weapon";

    [Header("Player")]
    [InspectorName("\u73a9\u5bb6\u7269\u4ef6\u540d\u7a31")]
    public string playerObjectName;

    [InspectorName("\u73a9\u5bb6\u79fb\u52d5\u6578\u503c")]
    public PlayerMotorTuning playerMotor = new PlayerMotorTuning();

    [InspectorName("\u73a9\u5bb6\u6230\u9b25\u6578\u503c")]
    public PlayerCombatTuning playerCombat = new PlayerCombatTuning();

    [Header("Camera")]
    [InspectorName("\u651d\u5f71\u6a5f\u7269\u4ef6\u540d\u7a31")]
    public string cameraObjectName;

    [InspectorName("\u651d\u5f71\u6a5f\u9707\u52d5\u6578\u503c")]
    public CameraShakeTuning cameraShake = new CameraShakeTuning();

    [Header("Enemies")]
    [InspectorName("\u6575\u4eba\u6578\u503c")]
    public EnemyTuningEntry[] enemies = new EnemyTuningEntry[0];

    [Header("Weapons")]
    [InspectorName("\u6b66\u5668\u6578\u503c")]
    public WeaponTuningEntry[] weapons = new WeaponTuningEntry[0];

    [Serializable]
    public class PlayerMotorTuning
    {
        [InspectorName("\u540c\u6b65\u9019\u7d44\u6578\u503c")]
        public bool sync = true;

        [InspectorName("\u79fb\u52d5\u6a21\u5f0f")]
        public BalanceMovementMode movementMode = BalanceMovementMode.Free3D;
        public float moveSpeed = 6f;
        public float airControl = 0.65f;
        public bool useCameraRelativeMovement = true;
        public float freeTurnSpeed = 720f;

        [Header("Side Scroller")]
        public float lockedZ;
        public Vector3 movementAxis = Vector3.right;

        [Header("Jump")]
        public float jumpForce = 8f;
        public float upwardGravityMultiplier = 1.2f;
        public float fallGravityMultiplier = 2.6f;
        public float maxFallSpeed = 18f;
        public float jumpBufferSeconds = 0.12f;
        public float coyoteTimeSeconds = 0.08f;
        public int extraAirJumps = 1;
        public float airJumpForceMultiplier = 1f;

        [Header("Dash")]
        public float dashDistance = 4f;
        public float dashDuration = 0.12f;
        public float dashCooldown = 0.45f;
        public bool allowAirDash = true;
        public bool flattenVerticalVelocityDuringDash = true;
        public float dashJumpHorizontalMultiplier = 1.8f;
        public float dashJumpDashSpeedCarryMultiplier = 0.85f;
        public float dashJumpBoostSeconds = 0.35f;
        public float airDashAnimationMinSeconds = 0.18f;
        public float dashEndAnimationMinSeconds = 0.12f;

        [Header("Dash Afterimage")]
        public bool enableDashAfterimage = true;
        public float dashAfterimageSpawnInterval = 0.035f;
        public float dashAfterimageLifetime = 0.18f;
        public Color dashAfterimageColor = new Color(0.25f, 0.55f, 1f, 0.45f);
        public bool dashAfterimageIncludeMeshRenderers = true;
        public bool dashAfterimageIncludeInactiveRenderers;

        [Header("Animation")]
        public float actionAnimationCrossFadeSeconds = 0.04f;
        public float jumpAnimationCrossFadeSeconds = 0.12f;

        [Header("Ground")]
        public Vector3 groundCheckLocalOffset = new Vector3(0f, -1.05f, 0f);
        public float groundCheckRadius = 0.2f;
        public LayerMask groundMask;
        public float groundFallbackDistance = 0.35f;
        public bool useAnySolidGroundFallback = true;

        [Header("One Way Platform")]
        public bool enableOneWayPlatforms = true;
        public float dropThroughSeconds = 0.45f;
        public float dropThroughStartSpeed = 1.2f;
        public float dropThroughPlatformSearchDistance = 0.18f;
        public float dropInputThreshold = 0.45f;
        public float oneWayPlatformPrecheckHeight = 1.6f;
        public float oneWayPlatformPrecheckPadding = 0.12f;

        [Header("Wall")]
        public bool preventAirWallSticking = true;
        public float wallNormalThreshold = 0.55f;
        public float wallContactGraceSeconds = 0.08f;
        public bool useNoFrictionColliderMaterial = true;

        [Header("Damage Knockback")]
        public bool enableDamageKnockback = true;
        public Vector2 knockbackForce = new Vector2(8f, 4f);
        public float knockbackControlLockSeconds = 0.45f;
        public bool lockControlUntilKnockbackLands = true;
    }

    [Serializable]
    public class PlayerCombatTuning
    {
        [InspectorName("\u540c\u6b65\u9019\u7d44\u6578\u503c")]
        public bool sync = true;

        public float combatDetectionRange = 5f;
        public float combatVerticalRange = 2.5f;
        public float combatMemorySeconds = 2f;
    }

    [Serializable]
    public class CameraShakeTuning
    {
        [InspectorName("\u540c\u6b65\u9019\u7d44\u6578\u503c")]
        public bool sync = true;

        public float defaultAmplitude = 0.08f;
        public float defaultDuration = 0.08f;
        public float defaultFrequency = 35f;
        public bool useUnscaledTime = true;
    }

    [Serializable]
    public class EnemyTuningEntry
    {
        [InspectorName("\u540c\u6b65\u9019\u7d44\u6578\u503c")]
        public bool sync = true;

        [InspectorName("\u6575\u4eba\u540d\u7a31")]
        public string enemyNameKey;

        [InspectorName("\u8b80\u53d6\u5230\u7684\u6578\u91cf")]
        public int sceneObjectCount;

        [InspectorName("\u5834\u666f\u4e2d\u6578\u503c\u4e0d\u4e00\u81f4")]
        public bool valuesDifferInScene;

        public BalanceAttackMode attackMode = BalanceAttackMode.Melee;
        public BalanceMovementMode movementMode = BalanceMovementMode.Free3D;
        public bool useTransformRightAsMovementAxis = true;
        public Vector3 movementAxis = Vector3.right;
        public bool lockDepthToMovementPlane = true;
        public float moveSpeed = 2f;
        public float patrolMoveSpeed = 1.6f;
        public float homeStopDistance = 0.12f;
        public float fallbackPatrolHalfWidth = 2f;
        public float patrolRadius = 3f;
        public float patrolDestinationReachDistance = 0.25f;
        public float patrolDestinationMinDistance = 1f;
        public LayerMask patrolObstacleMask;
        public bool usePatrolObstacleMask;
        public float patrolObstacleCheckDistance = 0.35f;
        public Vector3 patrolObstacleRayHeights = new Vector3(0.2f, 0.65f, 1.1f);

        [Header("Detection")]
        public float searchRange = 7f;
        public float giveUpRange = 9f;
        public Vector3 detectionBoxOffset = new Vector3(0f, 0.75f, 0f);
        public float detectionBoxHeight = 1.6f;
        public float detectionBoxDepth = 2f;
        public float giveUpBoxPadding = 1f;
        public bool showDetectionBoxGizmo = true;
        public bool onlyShowDetectionBoxWhenSelected;

        [Header("Attack")]
        public float attackRange = 1.45f;
        public float meleeAttackHeight = 2.5f;
        public int attackDamage = 1;
        public float meleeHitSoundVolume = 1f;
        public int projectileDamage = 1;
        public float projectileSpeed = 5.5f;
        public float projectileLifetime = 5f;
        public float projectileHitSoundVolume = 1f;
        public Vector3 projectileLocalOffset = new Vector3(0.65f, 0.95f, 0f);
        public float returnSpeed = 1.8f;
        public float bossRangedDistance = 4.5f;
        public float bossRangedDistanceTolerance = 0.35f;
        public bool bossContactDamageEnabled = true;
        public int bossContactDamage = 1;
        public float bossContactDamageCooldown = 0.8f;
        public Vector3 bossContactDamageBoxSize = new Vector3(1.7f, 2.2f, 1.2f);
        public Vector3 bossContactDamageBoxCenter = new Vector3(0f, 1f, 0f);
        public LayerMask bossContactDamageTargetMask;
        public float attackCooldown = 1.25f;
        public bool useRangedAttackRhythm;
        public float[] rangedAttackRhythm = { 1.25f };
        public float rangedAttackGroupCooldown;
        public float attackWindup = 0.25f;
        public float attackLockSeconds = 0.55f;

        [Header("Damage And Death")]
        [InspectorName("\u53d7\u64ca\u7279\u6548")]
        public EnemyHitEffectTuning hitEffect = new EnemyHitEffectTuning();
        public bool launchAwayOnDeath = true;
        public float deathLaunchSpeed = 8f;
        public float deathLaunchUpSpeed = 5f;
        public float deathSpinDegreesPerSecond = 720f;
        public float deathDestroyDelay = 1.25f;
        public bool knockbackOnDamage = true;
        public Vector2 damageKnockbackForce = new Vector2(3.2f, 1.2f);
        public float damageKnockbackLockSeconds = 0.18f;
        public float airborneHitPauseNormalizedTime = 0.5f;
        public float damageLandingRecoverySeconds = 0.5f;
        public float damageGroundCheckDistance = 0.08f;
        public LayerMask damageGroundMask;

        [Header("Respawn")]
        public bool respawnAfterCameraLeaves = true;
        public float respawnCameraAwaySeconds = 5f;
        public float respawnViewportPadding = 0.1f;
    }

    [Serializable]
    public class EnemyHitEffectTuning
    {
        [InspectorName("\u540c\u6b65\u53d7\u64ca\u7279\u6548")]
        public bool sync = true;

        public GameObject effectPrefab;
        public string effectAnchorName = "EF_Hit";
        public bool stopEffectOnAwake = true;
    }

    [Serializable]
    public class WeaponTuningEntry
    {
        [InspectorName("\u540c\u6b65\u9019\u628a\u6b66\u5668")]
        public bool sync = true;

        [InspectorName("\u6b66\u5668\u540d\u7a31")]
        public string weaponNameKey;

        [InspectorName("\u8b80\u53d6\u5230\u7684\u6578\u91cf")]
        public int sceneObjectCount;

        [InspectorName("\u5834\u666f\u4e2d\u6578\u503c\u4e0d\u4e00\u81f4")]
        public bool valuesDifferInScene;

        [InspectorName("\u653b\u64ca\u8a2d\u5b9a")]
        public WeaponProfileTuning attackProfile = new WeaponProfileTuning();

        [InspectorName("\u6b66\u5668\u5224\u5b9a")]
        public WeaponHitboxTuning hitbox = new WeaponHitboxTuning();
    }

    [Serializable]
    public class WeaponProfileTuning
    {
        [InspectorName("\u540c\u6b65\u653b\u64ca\u8a2d\u5b9a")]
        public bool sync = true;

        public float attackCooldown = 0.45f;
        public float attackMoveLockSeconds = 0.35f;
        public bool useAttackAnimationLength = true;
        public float attackSpeedMultiplier = 1.5f;
        public float attackCrossFadeSeconds = 0.03f;
        public bool allowAirAttacks;
        public LayerMask targetMask = 512;
        public PlayerWeaponAttackStep[] attacks = new PlayerWeaponAttackStep[0];
        public AudioClip attackHitSound;
        public float attackHitSoundVolume = 1f;
        public PlayerHitSoundRule[] targetHitSounds = new PlayerHitSoundRule[0];
    }

    [Serializable]
    public class WeaponHitboxTuning
    {
        [InspectorName("\u540c\u6b65\u6b66\u5668\u5224\u5b9a")]
        public bool sync = true;

        public Vector3 weaponSize = new Vector3(1.35f, 0.16f, 0.16f);
        public Color weaponColor = new Color(0.85f, 0.82f, 0.72f, 1f);
        public Transform weaponModelRoot;
        public bool useModelBoundsForHitbox;
        public bool updateColliderDuringPlay;
        public Vector3 modelBoundsPadding = new Vector3(0.04f, 0.04f, 0.04f);
        public Vector3 projectileReflectExtraRange = new Vector3(0.45f, 0.3f, 0.3f);
        public GameObject projectileReflectEffectPrefab;
        public float projectileReflectEffectScale = 1f;
        public float projectileReflectEffectFallbackLifetime = 2f;
    }
}
