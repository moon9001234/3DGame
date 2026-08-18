using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDefinition3D", menuName = "Side Scroller 3D/Enemy Definition")]
public class EnemyDefinition3D : ScriptableObject
{
    [Header("Type")]
    public EnemyPatrol3D.AttackMode attackMode = EnemyPatrol3D.AttackMode.Melee;
    public EnemyPatrol3D.MovementMode movementMode = EnemyPatrol3D.MovementMode.Free3D;

    [Header("Patrol")]
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

    [Header("Melee")]
    public float attackRange = 1.45f;
    public float meleeAttackHeight = 2.5f;
    public int attackDamage = 1;
    public AudioClip meleeHitSound;
    [Range(0f, 1f)] public float meleeHitSoundVolume = 1f;

    [Header("Projectile")]
    public int projectileDamage = 1;
    public float projectileSpeed = 5.5f;
    public float projectileLifetime = 5f;
    public AudioClip projectileHitSound;
    [Range(0f, 1f)] public float projectileHitSoundVolume = 1f;
    public Vector3 projectileLocalOffset = new Vector3(0.65f, 0.95f, 0f);
    public float returnSpeed = 1.8f;

    [Header("Boss")]
    public float bossRangedDistance = 4.5f;
    public float bossRangedDistanceTolerance = 0.35f;
    public bool bossContactDamageEnabled = true;
    public int bossContactDamage = 1;
    public float bossContactDamageCooldown = 0.8f;
    public Vector3 bossContactDamageBoxSize = new Vector3(1.7f, 2.2f, 1.2f);
    public Vector3 bossContactDamageBoxCenter = new Vector3(0f, 1f, 0f);
    public LayerMask bossContactDamageTargetMask;
    public EnemyBossProjectileDefinition3D[] bossProjectileTypes =
    {
        new EnemyBossProjectileDefinition3D("Fireball", false, 5.8f),
        new EnemyBossProjectileDefinition3D("IronBall", true, 4f)
    };

    [Header("Attack Timing")]
    public float attackCooldown = 1.25f;
    public bool useRangedAttackRhythm;
    public float[] rangedAttackRhythm = { 1.25f };
    public float rangedAttackGroupCooldown;
    public float attackWindup = 0.25f;
    public float attackLockSeconds = 0.55f;

    [Header("Death")]
    public bool launchAwayOnDeath = true;
    public float deathLaunchSpeed = 8f;
    public float deathLaunchUpSpeed = 5f;
    public float deathSpinDegreesPerSecond = 720f;
    public float deathDestroyDelay = 1.25f;

    [Header("Damage Knockback")]
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
public class EnemyBossProjectileDefinition3D
{
    public string name = "Projectile";
    public Transform visualTemplate;
    public bool canBeReflected = true;
    public int projectileDamage = 1;
    public float projectileSpeed = 5.5f;
    public float projectileLifetime = 5f;
    public float rangedDistance = 4.5f;
    public AudioClip hitSound;
    [Range(0f, 1f)] public float hitSoundVolume = 1f;

    public EnemyBossProjectileDefinition3D()
    {
    }

    public EnemyBossProjectileDefinition3D(string name, bool canBeReflected, float rangedDistance)
    {
        this.name = name;
        this.canBeReflected = canBeReflected;
        this.rangedDistance = rangedDistance;
    }
}
