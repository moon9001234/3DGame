using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class EnemyPatrol3D : MonoBehaviour
{
    private const string ContactDamageObjectName = "Enemy_ContactDamage";
    private const string DefaultHitSoundPath = "Assets/Art/Sound/Hit.wav";
    private const float EdgeHysteresis = 0.35f;

    public enum AttackMode
    {
        Melee,
        Ranged,
        Boss
    }

    private enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        ReturnHome
    }

    public enum MovementMode
    {
        Free3D,
        SideScroller
    }

    [System.Serializable]
    private class BossProjectileType
    {
        [Tooltip("Name used for the spawned projectile object.")]
        [SerializeField] private string name = "Projectile";

        [Tooltip("Optional visual object for this projectile. If empty, the same index child under Shoot will be used.")]
        [SerializeField] private Transform visualTemplate;

        [Tooltip("If enabled, the player can reflect this projectile with a counter attack.")]
        [SerializeField] private bool canBeReflected = true;

        [Tooltip("Damage dealt by this Boss projectile.")]
        [FormerlySerializedAs("damageOverride")]
        [SerializeField] private int projectileDamage = 1;

        [Tooltip("Flying speed for this Boss projectile.")]
        [FormerlySerializedAs("speedOverride")]
        [SerializeField] private float projectileSpeed = 5.5f;

        [Tooltip("Seconds before this Boss projectile is destroyed automatically.")]
        [FormerlySerializedAs("lifetimeOverride")]
        [SerializeField] private float projectileLifetime = 5f;

        [Tooltip("Boss target distance from the player before firing this projectile.")]
        [SerializeField] private float rangedDistance = 4.5f;

        [Tooltip("Optional hit sound for this Boss projectile. If empty, the enemy Projectile Hit Sound is used.")]
        [SerializeField] private AudioClip hitSound;

        [Tooltip("Volume for this Boss projectile hit sound.")]
        [SerializeField, Range(0f, 1f)] private float hitSoundVolume = 1f;

        public BossProjectileType()
        {
        }

        public BossProjectileType(string name, bool canBeReflected)
        {
            this.name = name;
            this.canBeReflected = canBeReflected;
        }

        public BossProjectileType(string name, bool canBeReflected, float rangedDistance)
        {
            this.name = name;
            this.canBeReflected = canBeReflected;
            this.rangedDistance = rangedDistance;
        }

        public BossProjectileType(EnemyBossProjectileDefinition3D definition)
        {
            ApplyDefinition(definition);
        }

        public string Name => string.IsNullOrEmpty(name) ? "BossProjectile" : name;
        public Transform VisualTemplate => visualTemplate;
        public bool CanBeReflected => canBeReflected;

        public int ResolveDamage(int fallback)
        {
            return projectileDamage >= 0 ? projectileDamage : fallback;
        }

        public float ResolveSpeed(float fallback)
        {
            return projectileSpeed > 0f ? projectileSpeed : fallback;
        }

        public float ResolveLifetime(float fallback)
        {
            return projectileLifetime > 0f ? projectileLifetime : fallback;
        }

        public float ResolveRangedDistance(float fallback)
        {
            return rangedDistance > 0f ? rangedDistance : fallback;
        }

        public AudioClip ResolveHitSound(AudioClip fallback)
        {
            return hitSound != null ? hitSound : fallback;
        }

        public float ResolveHitSoundVolume(float fallback)
        {
            return hitSound != null ? Mathf.Clamp01(hitSoundVolume) : Mathf.Clamp01(fallback);
        }

        public void Validate(int defaultDamage, float defaultSpeed, float defaultLifetime, float defaultRangedDistance)
        {
            if (projectileDamage < 0)
            {
                projectileDamage = Mathf.Max(0, defaultDamage);
            }

            if (projectileSpeed <= 0f)
            {
                projectileSpeed = Mathf.Max(0.1f, defaultSpeed);
            }

            if (projectileLifetime <= 0f)
            {
                projectileLifetime = Mathf.Max(0.1f, defaultLifetime);
            }

            if (rangedDistance <= 0f)
            {
                string projectileName = string.IsNullOrEmpty(name) ? string.Empty : name.ToLowerInvariant();
                rangedDistance = projectileName.Contains("fire")
                    ? Mathf.Max(0.1f, defaultRangedDistance + 1.3f)
                    : Mathf.Max(0.1f, defaultRangedDistance);
            }

            hitSoundVolume = Mathf.Clamp01(hitSoundVolume);
        }

        public void ApplyDefinition(EnemyBossProjectileDefinition3D definition)
        {
            if (definition == null)
            {
                return;
            }

            name = definition.name;
            if (definition.visualTemplate != null)
            {
                visualTemplate = definition.visualTemplate;
            }
            canBeReflected = definition.canBeReflected;
            projectileDamage = definition.projectileDamage;
            projectileSpeed = definition.projectileSpeed;
            projectileLifetime = definition.projectileLifetime;
            rangedDistance = definition.rangedDistance;
            hitSound = definition.hitSound;
            hitSoundVolume = definition.hitSoundVolume;
        }

        public EnemyBossProjectileDefinition3D ToDefinition()
        {
            return new EnemyBossProjectileDefinition3D
            {
                name = name,
                visualTemplate = visualTemplate,
                canBeReflected = canBeReflected,
                projectileDamage = projectileDamage,
                projectileSpeed = projectileSpeed,
                projectileLifetime = projectileLifetime,
                rangedDistance = rangedDistance,
                hitSound = hitSound,
                hitSoundVolume = hitSoundVolume
            };
        }
    }

    [Header("Definition")]
    [Tooltip("Optional reusable enemy data asset. When assigned, this enemy can load its tuning from the asset.")]
    [SerializeField] private EnemyDefinition3D enemyDefinition;

    [Tooltip("Apply Enemy Definition values when the enemy starts playing.")]
    [SerializeField] private bool applyDefinitionOnAwake = true;

    [Tooltip("Apply Enemy Definition values in edit mode during validation. Keep disabled when locally tweaking a prefab override.")]
    [SerializeField] private bool applyDefinitionInEditor;

    [Header("\u6575\u4eba\u985e\u578b")]
    [Tooltip("\u6575\u4eba\u7684\u653b\u64ca\u6a21\u5f0f\u3002Melee \u662f\u8fd1\u6230\uff0cRanged \u662f\u9060\u7a0b\u706b\u7403\u3002")]
    [SerializeField] private AttackMode attackMode = AttackMode.Melee;

    [Header("\u5de1\u908f")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6575\u4eba\u6703\u7528\u81ea\u8eab Transform \u7684 Right \u65b9\u5411\u7576\u4f5c\u79fb\u52d5\u3001\u8ffd\u64ca\u8207\u653b\u64ca\u5224\u5b9a\u65b9\u5411\u3002")]
    [SerializeField] private MovementMode movementMode = MovementMode.Free3D;

    [SerializeField] private bool useTransformRightAsMovementAxis = true;

    [Tooltip("\u95dc\u9589\u81ea\u52d5\u6293\u53d6\u6642\uff0c\u624b\u52d5\u6307\u5b9a\u6575\u4eba\u7684\u79fb\u52d5\u8ef8\u3002")]
    [SerializeField] private Vector3 movementAxis = Vector3.right;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6575\u4eba\u6703\u88ab\u9650\u5236\u5728\u79fb\u52d5\u8ef8\u5f62\u6210\u7684\u5e73\u9762\u4e0a\uff0c\u907f\u514d\u8d70\u5230\u932f\u8aa4\u6df1\u5ea6\u3002")]
    [SerializeField] private bool lockDepthToMovementPlane = true;

    [Tooltip("\u6575\u4eba\u8ffd\u64ca\u73a9\u5bb6\u6642\u7684\u79fb\u52d5\u901f\u5ea6\u3002")]
    [SerializeField] private float moveSpeed = 2f;
    [Tooltip("\u6575\u4eba\u5de1\u908f\u6642\u7684\u79fb\u52d5\u901f\u5ea6\u3002")]
    [SerializeField] private float patrolMoveSpeed = 1.6f;
    [Tooltip("\u6575\u4eba\u8fd4\u56de\u51fa\u751f\u9ede\u6642\uff0c\u8ddd\u96e2\u5c0f\u65bc\u9019\u500b\u503c\u5c31\u8996\u70ba\u5df2\u7d93\u56de\u5230\u539f\u4f4d\u3002")]
    [SerializeField] private float homeStopDistance = 0.12f;
    [Tooltip("\u6c92\u6709\u53ef\u7528\u5de1\u908f\u9ede\u6642\uff0c\u6575\u4eba\u6703\u4ee5\u51fa\u751f\u9ede\u5de6\u53f3\u9019\u500b\u8ddd\u96e2\u4f5c\u70ba\u5de1\u908f\u7bc4\u570d\u3002")]
    [SerializeField] private float fallbackPatrolHalfWidth = 2f;

    [SerializeField] private float patrolRadius = 3f;

    [SerializeField] private float patrolDestinationReachDistance = 0.25f;

    [SerializeField] private float patrolDestinationMinDistance = 1f;
    [Tooltip("\u53ef\u9078\u7684\u5de1\u908f\u969c\u7919 Layer \u7be9\u9078\u3002\u53ea\u6709\u958b\u555f Use Patrol Obstacle Mask \u6642\u624d\u6703\u5957\u7528\u3002")]
    [SerializeField] private LayerMask patrolObstacleMask;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u524d\u65b9\u969c\u7919\u5075\u6e2c\u53ea\u6703\u5075\u6e2c Patrol Obstacle Mask \u6307\u5b9a\u7684 Layer\u3002\u95dc\u9589\u6642\u6703\u5075\u6e2c\u6240\u6709\u6703\u963b\u64cb\u79fb\u52d5\u7684\u5be6\u9ad4 Collider\u3002")]
    [SerializeField] private bool usePatrolObstacleMask;
    [Tooltip("\u5de1\u908f\u65b9\u5411\u524d\u65b9\u7528\u4f86\u5075\u6e2c\u969c\u7919\u7269\u7684\u8ddd\u96e2\u3002")]
    [SerializeField] private float patrolObstacleCheckDistance = 0.35f;
    [Tooltip("\u5de1\u908f\u969c\u7919\u5075\u6e2c\u5c04\u7dda\u7684\u4e09\u500b\u9ad8\u5ea6\uff0cX/Y/Z \u5206\u5225\u4ee3\u8868\u4f4e\u3001\u4e2d\u3001\u9ad8\u5c04\u7dda\u3002")]
    [SerializeField] private Vector3 patrolObstacleRayHeights = new Vector3(0.2f, 0.65f, 1.1f);

    [Header("\u5075\u6e2c")]
    [Tooltip("\u73a9\u5bb6\u9032\u5165\u9019\u500b\u5075\u6e2c\u76d2\u7684\u6c34\u5e73\u7bc4\u570d\u5f8c\uff0c\u6575\u4eba\u6703\u958b\u59cb\u8ffd\u64ca\u6216\u9032\u5165\u6230\u9b25\u3002")]
    [SerializeField] private float searchRange = 7f;
    [Tooltip("\u73a9\u5bb6\u96e2\u958b\u9019\u500b\u653e\u68c4\u5075\u6e2c\u76d2\u5f8c\uff0c\u6575\u4eba\u6703\u505c\u6b62\u8ffd\u64ca\u4e26\u8fd4\u56de\u51fa\u751f\u9ede\u3002")]
    [SerializeField] private float giveUpRange = 9f;
    [Tooltip("\u5075\u6e2c\u76d2\u76f8\u5c0d\u65bc\u6575\u4eba\u6839\u7269\u4ef6\u7684\u4f4d\u7f6e\u504f\u79fb\u3002Y \u53ef\u63a7\u5236\u5075\u6e2c\u76d2\u9ad8\u5ea6\u4e2d\u5fc3\u3002")]
    [SerializeField] private Vector3 detectionBoxOffset = new Vector3(0f, 0.75f, 0f);
    [Tooltip("\u5075\u6e2c\u76d2\u7684\u9ad8\u5ea6\u3002\u8abf\u5c0f\u53ef\u4ee5\u907f\u514d\u73a9\u5bb6\u5728\u6575\u4eba\u982d\u9802\u4e5f\u89f8\u767c\u8ffd\u64ca\u3002")]
    [SerializeField] private float detectionBoxHeight = 1.6f;
    [Tooltip("\u5075\u6e2c\u76d2\u5728\u6df1\u5ea6\u65b9\u5411\u7684\u539a\u5ea6\u3002")]
    [SerializeField] private float detectionBoxDepth = 2f;
    [Tooltip("\u653e\u68c4\u8ffd\u64ca\u7684\u5075\u6e2c\u76d2\u984d\u5916\u5916\u64f4\u91cf\uff0c\u907f\u514d\u73a9\u5bb6\u525b\u5230\u908a\u754c\u5c31\u8b93\u6575\u4eba\u53cd\u8986\u5207\u63db\u72c0\u614b\u3002")]
    [SerializeField] private float giveUpBoxPadding = 1f;
    [Tooltip("Show the enemy detection box in the Scene view.")]
    [SerializeField] private bool showDetectionBoxGizmo = true;
    [Tooltip("If enabled, the detection box is shown only when this enemy is selected.")]
    [SerializeField] private bool onlyShowDetectionBoxWhenSelected;

    [Header("\u8fd1\u6230\u653b\u64ca")]
    [Tooltip("\u8fd1\u6230\u653b\u64ca\u5411\u524d\u6253\u51fa\u7684\u8ddd\u96e2\uff0c\u4e5f\u5c31\u662f\u7d05\u8272\u653b\u64ca\u6846\u7684\u524d\u5f8c\u9577\u5ea6\u3002")]
    [SerializeField] private float attackRange = 1.45f;
    [Tooltip("\u8fd1\u6230\u653b\u64ca\u6846\u7684\u9ad8\u5ea6\u3002\u8abf\u5c0f\u53ef\u4ee5\u907f\u514d\u6253\u5230\u6575\u4eba\u982d\u9802\u592a\u9ad8\u8655\u7684\u73a9\u5bb6\u3002")]
    [FormerlySerializedAs("verticalSearchRange")]
    [SerializeField] private float meleeAttackHeight = 2.5f;
    [Tooltip("\u8fd1\u6230\u653b\u64ca\u547d\u4e2d\u73a9\u5bb6\u6642\u9020\u6210\u7684\u50b7\u5bb3\u3002")]
    [SerializeField] private int attackDamage = 1;
    [Tooltip("Sound played when this enemy melee attack hits the player.")]
    [SerializeField] private AudioClip meleeHitSound;
    [Tooltip("Volume for the enemy melee hit sound.")]
    [SerializeField, Range(0f, 1f)] private float meleeHitSoundVolume = 1f;
    [Header("\u9060\u7a0b\u653b\u64ca")]
    [Tooltip("\u9060\u7a0b\u706b\u7403\u547d\u4e2d\u73a9\u5bb6\u6216\u88ab\u73a9\u5bb6\u53cd\u64ca\u5f8c\u547d\u4e2d\u6575\u4eba\u6642\u9020\u6210\u7684\u50b7\u5bb3\u3002")]
    [SerializeField] private int projectileDamage = 1;
    [Tooltip("\u9060\u7a0b\u706b\u7403\u7684\u98db\u884c\u901f\u5ea6\u3002")]
    [SerializeField] private float projectileSpeed = 5.5f;
    [Tooltip("\u9060\u7a0b\u706b\u7403\u5b58\u5728\u5e7e\u79d2\u5f8c\u81ea\u52d5\u6d88\u5931\u3002")]
    [SerializeField] private float projectileLifetime = 5f;
    [Tooltip("Sound played when this enemy projectile hits its target.")]
    [SerializeField] private AudioClip projectileHitSound;
    [Tooltip("Volume for the enemy projectile hit sound.")]
    [SerializeField, Range(0f, 1f)] private float projectileHitSoundVolume = 1f;
    [Tooltip("\u6c92\u6709 Shoot \u767c\u5c04\u9ede\u6642\uff0c\u706b\u7403\u6703\u5f9e\u6575\u4eba\u8eab\u4e0a\u9019\u500b\u672c\u5730\u504f\u79fb\u4f4d\u7f6e\u767c\u5c04\u3002")]
    [SerializeField] private Vector3 projectileLocalOffset = new Vector3(0.65f, 0.95f, 0f);
    [Tooltip("\u9060\u7a0b\u6575\u4eba\u8fd4\u56de\u51fa\u751f\u9ede\u6642\u7684\u79fb\u52d5\u901f\u5ea6\u3002")]
    [SerializeField] private float returnSpeed = 1.8f;

    [Header("Boss \u653b\u64ca")]
    [Tooltip("Boss \u9060\u7a0b\u653b\u64ca\u524d\u6703\u548c\u73a9\u5bb6\u4fdd\u6301\u7684\u6c34\u5e73\u8ddd\u96e2\u3002\u8ddd\u96e2\u592a\u8fd1\u6703\u5f8c\u9000\uff0c\u592a\u9060\u6703\u9760\u8fd1\u3002")]
    [SerializeField] private float bossRangedDistance = 4.5f;

    [Tooltip("Boss \u9060\u7a0b\u8ddd\u96e2\u5141\u8a31\u7684\u8aa4\u5dee\u7bc4\u570d\u3002\u73a9\u5bb6\u8ddd\u96e2\u843d\u5728 Boss Ranged Distance \u6b63\u8ca0\u9019\u500b\u503c\u5167\u624d\u6703\u9060\u653b\u3002")]
    [SerializeField] private float bossRangedDistanceTolerance = 0.35f;

    [Header("Boss Contact Damage")]
    [Tooltip("Enable damage when the player touches the Boss body.")]
    [SerializeField] private bool bossContactDamageEnabled = true;
    [Tooltip("Damage applied when the player touches the Boss.")]
    [SerializeField] private int bossContactDamage = 1;
    [Tooltip("Seconds before Boss contact damage can hit the same target again.")]
    [SerializeField] private float bossContactDamageCooldown = 0.8f;
    [Tooltip("Local box size used by the generated Boss contact damage trigger.")]
    [SerializeField] private Vector3 bossContactDamageBoxSize = new Vector3(1.7f, 2.2f, 1.2f);
    [Tooltip("Local center used by the generated Boss contact damage trigger.")]
    [SerializeField] private Vector3 bossContactDamageBoxCenter = new Vector3(0f, 1f, 0f);
    [Tooltip("Layers damaged by Boss contact damage. Leave empty to use Player.")]
    [SerializeField] private LayerMask bossContactDamageTargetMask;

    [Header("Boss Remote Projectiles")]
    [Tooltip("Boss projectile list. If Visual Template is empty, the same index child under Shoot will be used. Example: Element 0 Fireball cannot reflect, Element 1 IronBall can reflect.")]
    [SerializeField] private BossProjectileType[] bossProjectileTypes =
    {
        new BossProjectileType("Fireball", false, 5.8f),
        new BossProjectileType("IronBall", true, 4f)
    };

    [Header("\u653b\u64ca\u6642\u9593")]
    [Tooltip("\u5169\u6b21\u653b\u64ca\u4e4b\u9593\u7684\u6700\u77ed\u9593\u9694\u79d2\u6578\u3002")]
    [SerializeField] private float attackCooldown = 1.25f;
    [Tooltip("When enabled, projectile attacks use Ranged Attack Rhythm instead of the fixed Attack Cooldown.")]
    [SerializeField] private bool useRangedAttackRhythm;
    [Tooltip("Projectile attack intervals inside one group. The first value is the delay before the first projectile, then each following value delays the next projectile in the group.")]
    [SerializeField] private float[] rangedAttackRhythm = { 1.25f };
    [Tooltip("Extra cooldown after one full ranged attack rhythm group finishes before the rhythm starts again.")]
    [SerializeField] private float rangedAttackGroupCooldown;
    [Tooltip("\u958b\u59cb\u653b\u64ca\u5f8c\uff0c\u5ef6\u9072\u5e7e\u79d2\u624d\u771f\u6b63\u9020\u6210\u50b7\u5bb3\u6216\u5c04\u51fa\u706b\u7403\u3002")]
    [SerializeField] private float attackWindup = 0.25f;
    [Tooltip("\u653b\u64ca\u52d5\u4f5c\u671f\u9593\u6575\u4eba\u4e0d\u80fd\u79fb\u52d5\u7684\u79d2\u6578\u3002")]
    [SerializeField] private float attackLockSeconds = 0.55f;

    [Header("\u6b7b\u4ea1\u8868\u6f14")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6575\u4eba\u6b7b\u4ea1\u6642\u6703\u88ab\u5f48\u98db\u51fa\u756b\u9762\uff0c\u800c\u4e0d\u662f\u7acb\u523b\u6d88\u5931\u3002")]
    [SerializeField] private bool launchAwayOnDeath = true;
    [Tooltip("\u6575\u4eba\u6b7b\u4ea1\u5f48\u98db\u7684\u6c34\u5e73\u901f\u5ea6\u3002")]
    [SerializeField] private float deathLaunchSpeed = 8f;
    [Tooltip("\u6575\u4eba\u6b7b\u4ea1\u5f48\u98db\u7684\u5411\u4e0a\u901f\u5ea6\u3002")]
    [SerializeField] private float deathLaunchUpSpeed = 5f;
    [Tooltip("\u6575\u4eba\u6b7b\u4ea1\u5f48\u98db\u6642\u6bcf\u79d2\u65cb\u8f49\u7684\u89d2\u5ea6\u3002")]
    [SerializeField] private float deathSpinDegreesPerSecond = 720f;
    [Tooltip("\u6575\u4eba\u6b7b\u4ea1\u5f8c\u7b49\u5f85\u5e7e\u79d2\u624d\u96b1\u85cf\uff0c\u63a5\u8457\u9032\u5165\u91cd\u751f\u7b49\u5f85\u3002")]
    [SerializeField] private float deathDestroyDelay = 1.25f;

    [Header("\u53d7\u50b7\u64ca\u9000")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6575\u4eba\u53d7\u5230\u50b7\u5bb3\u6642\u6703\u5f80\u50b7\u5bb3\u4f86\u6e90\u7684\u53cd\u65b9\u5411\u5f48\u958b\u3002")]
    [SerializeField] private bool knockbackOnDamage = true;

    [Tooltip("\u6575\u4eba\u53d7\u50b7\u64ca\u9000\u529b\u9053\u3002X \u662f\u6c34\u5e73\u5f48\u958b\u901f\u5ea6\uff0cY \u662f\u5411\u4e0a\u5f48\u8d77\u901f\u5ea6\u3002")]
    [SerializeField] private Vector2 damageKnockbackForce = new Vector2(3.2f, 1.2f);

    [Tooltip("\u53d7\u50b7\u64ca\u9000\u5f8c\uff0c\u6575\u4eba\u66ab\u505c\u5de1\u908f/\u8ffd\u64ca\u63a7\u5236\u7684\u79d2\u6578\u3002")]
    [SerializeField] private float damageKnockbackLockSeconds = 0.18f;

    [Tooltip("\u53d7\u64ca\u52d5\u4f5c\u5728\u7a7a\u4e2d\u8981\u66ab\u505c\u7684\u6642\u9593\u9ede\u30020.5 \u4ee3\u8868 290~300 \u4e4b\u9593\u7684 295\u3002")]
    [SerializeField] private float airborneHitPauseNormalizedTime = 0.5f;

    [Tooltip("\u6575\u4eba\u53d7\u64ca\u843d\u5730\u5f8c\uff0c\u7b49\u5f85\u5e7e\u79d2\u624d\u6062\u5fa9\u5de1\u908f\u6216\u8ffd\u64ca\u3002")]
    [SerializeField] private float damageLandingRecoverySeconds = 0.5f;

    [Tooltip("\u5224\u65b7\u53d7\u64ca\u5f8c\u662f\u5426\u843d\u5730\u7684\u984d\u5916\u8ddd\u96e2\u3002")]
    [SerializeField] private float damageGroundCheckDistance = 0.08f;

    [Tooltip("\u53d7\u64ca\u843d\u5730\u5224\u65b7\u4f7f\u7528\u7684\u5730\u9762 Layer\u3002\u7559\u7a7a\u6642\u4f7f\u7528 Ground layer\u3002")]
    [SerializeField] private LayerMask damageGroundMask;

    [Header("\u91cd\u751f")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u6575\u4eba\u6b7b\u4ea1\u4e14\u651d\u5f71\u6a5f\u96e2\u958b\u51fa\u751f\u9ede\u4e00\u6bb5\u6642\u9593\u5f8c\u6703\u91cd\u751f\u3002")]
    [SerializeField] private bool respawnAfterCameraLeaves = true;
    [Tooltip("\u651d\u5f71\u6a5f\u96e2\u958b\u6575\u4eba\u51fa\u751f\u9ede\u5f8c\uff0c\u8981\u7b49\u5f85\u5e7e\u79d2\u624d\u91cd\u751f\u3002")]
    [SerializeField] private float respawnCameraAwaySeconds = 5f;
    [Tooltip("\u5224\u65b7\u51fa\u751f\u9ede\u662f\u5426\u96e2\u958b\u651d\u5f71\u6a5f\u756b\u9762\u6642\uff0c\u984d\u5916\u52a0\u4e0a\u7684\u756b\u9762\u5916\u7de9\u885d\u3002")]
    [SerializeField] private float respawnViewportPadding = 0.1f;
    [Tooltip("\u6575\u4eba\u6700\u8fd1\u5b58\u5728\u65bc\u5075\u6e2c\u7bc4\u570d\u5167\u7684\u4fdd\u7559\u6642\u9593\uff0c\u7528\u4f86\u907f\u514d\u908a\u754c\u4e0a\u4f86\u56de\u5207\u63db\u8ffd\u64ca/\u8fd4\u5bb6\u3002")]
    [SerializeField] private float targetLostGraceSeconds = 0.35f;

    private Rigidbody body;
    private Collider bodyCollider;
    private Health health;
    private Transform target;
    private Transform cachedTargetColliderRoot;
    private Collider[] targetBodyColliders = new Collider[0];
    private bool playerCollisionIgnoreApplied;
    private Transform projectileSpawn;
    private Transform projectileVisualTemplate;
    private Transform[] projectileVisualTemplates;
    private EnemyVisualAnimator visualAnimator;
    private EnemyGrounder3D grounder;
    private EnemyState state = EnemyState.Patrol;
    private int direction = 1;
    private float leftPatrolDistance;
    private float rightPatrolDistance;
    private Vector3 homePosition;
    private Quaternion spawnRotation;
    private Vector3 spawnScale;
    private int spawnDirection;
    private RigidbodyConstraints aliveConstraints;
    private bool aliveUseGravity;
    private bool aliveIsKinematic;
    private Vector3 depthAxis = Vector3.forward;
    private float lockedDepth;
    private float nextAttackTime;
    private float attackResolveTime;
    private float attackEndTime;
    private bool attackResolved = true;
    private bool currentAttackUsesRanged;
    private float queuedRangedAttackInterval = -1f;
    private int rangedAttackRhythmIndex;
    private bool rangedAttackRhythmInitialDelayScheduled;
    private bool rangedAttackGroupPlaybackActive;
    private bool deathSequenceStarted;
    private int selectedBossProjectileIndex = -1;
    private Collider[] deathDisabledColliders;
    private bool[] deathDisabledColliderStates;
    private Renderer[] deathHiddenRenderers;
    private bool[] deathHiddenRendererStates;
    private readonly RaycastHit[] obstacleHits = new RaycastHit[8];
    private readonly RaycastHit[] damageGroundHits = new RaycastHit[8];
    private float damageKnockbackLockedUntil;
    private float damageLandingRecoveryUntil;
    private float damageGroundContactUntil;
    private bool waitingForDamageLanding;
    private Vector3 patrolDestination;
    private bool hasPatrolDestination;
    private bool damageGrounderStateStored;
    private bool grounderWasEnabledBeforeDamage;
    private bool damageGravityStateStored;
    private bool useGravityBeforeDamage;
    private float targetLostUntil = -1f;

    public AttackMode Mode => attackMode;
    public EnemyDefinition3D Definition => enemyDefinition;
    private bool UsesFree3DMovement => movementMode == MovementMode.Free3D;

    public void ApplyDefinition()
    {
        ApplyDefinition(enemyDefinition);
    }

    public void ApplyDefinition(EnemyDefinition3D definition)
    {
        if (definition == null)
        {
            return;
        }

        attackMode = definition.attackMode;
        movementMode = definition.movementMode;
        useTransformRightAsMovementAxis = definition.useTransformRightAsMovementAxis;
        movementAxis = definition.movementAxis;
        lockDepthToMovementPlane = definition.lockDepthToMovementPlane;
        moveSpeed = definition.moveSpeed;
        patrolMoveSpeed = definition.patrolMoveSpeed;
        homeStopDistance = definition.homeStopDistance;
        fallbackPatrolHalfWidth = definition.fallbackPatrolHalfWidth;
        patrolRadius = definition.patrolRadius;
        patrolDestinationReachDistance = definition.patrolDestinationReachDistance;
        patrolDestinationMinDistance = definition.patrolDestinationMinDistance;
        patrolObstacleMask = definition.patrolObstacleMask;
        usePatrolObstacleMask = definition.usePatrolObstacleMask;
        patrolObstacleCheckDistance = definition.patrolObstacleCheckDistance;
        patrolObstacleRayHeights = definition.patrolObstacleRayHeights;

        searchRange = definition.searchRange;
        giveUpRange = definition.giveUpRange;
        detectionBoxOffset = definition.detectionBoxOffset;
        detectionBoxHeight = definition.detectionBoxHeight;
        detectionBoxDepth = definition.detectionBoxDepth;
        giveUpBoxPadding = definition.giveUpBoxPadding;
        showDetectionBoxGizmo = definition.showDetectionBoxGizmo;
        onlyShowDetectionBoxWhenSelected = definition.onlyShowDetectionBoxWhenSelected;

        attackRange = definition.attackRange;
        meleeAttackHeight = definition.meleeAttackHeight;
        attackDamage = definition.attackDamage;
        meleeHitSound = definition.meleeHitSound;
        meleeHitSoundVolume = definition.meleeHitSoundVolume;

        projectileDamage = definition.projectileDamage;
        projectileSpeed = definition.projectileSpeed;
        projectileLifetime = definition.projectileLifetime;
        projectileHitSound = definition.projectileHitSound;
        projectileHitSoundVolume = definition.projectileHitSoundVolume;
        projectileLocalOffset = definition.projectileLocalOffset;
        returnSpeed = definition.returnSpeed;

        bossRangedDistance = definition.bossRangedDistance;
        bossRangedDistanceTolerance = definition.bossRangedDistanceTolerance;
        bossContactDamageEnabled = definition.bossContactDamageEnabled;
        bossContactDamage = definition.bossContactDamage;
        bossContactDamageCooldown = definition.bossContactDamageCooldown;
        bossContactDamageBoxSize = definition.bossContactDamageBoxSize;
        bossContactDamageBoxCenter = definition.bossContactDamageBoxCenter;
        bossContactDamageTargetMask = definition.bossContactDamageTargetMask;
        bossProjectileTypes = CloneBossProjectileTypes(definition.bossProjectileTypes);

        attackCooldown = definition.attackCooldown;
        useRangedAttackRhythm = definition.useRangedAttackRhythm;
        rangedAttackRhythm = CloneFloatArray(definition.rangedAttackRhythm);
        rangedAttackGroupCooldown = definition.rangedAttackGroupCooldown;
        attackWindup = definition.attackWindup;
        attackLockSeconds = definition.attackLockSeconds;

        launchAwayOnDeath = definition.launchAwayOnDeath;
        deathLaunchSpeed = definition.deathLaunchSpeed;
        deathLaunchUpSpeed = definition.deathLaunchUpSpeed;
        deathSpinDegreesPerSecond = definition.deathSpinDegreesPerSecond;
        deathDestroyDelay = definition.deathDestroyDelay;

        knockbackOnDamage = definition.knockbackOnDamage;
        damageKnockbackForce = definition.damageKnockbackForce;
        damageKnockbackLockSeconds = definition.damageKnockbackLockSeconds;
        airborneHitPauseNormalizedTime = definition.airborneHitPauseNormalizedTime;
        damageLandingRecoverySeconds = definition.damageLandingRecoverySeconds;
        damageGroundCheckDistance = definition.damageGroundCheckDistance;
        damageGroundMask = definition.damageGroundMask;

        respawnAfterCameraLeaves = definition.respawnAfterCameraLeaves;
        respawnCameraAwaySeconds = definition.respawnCameraAwaySeconds;
        respawnViewportPadding = definition.respawnViewportPadding;

        selectedBossProjectileIndex = -1;
        ResetRangedAttackRhythm();
        ValidateRangedAttackRhythm();
        ValidateBossProjectileTypes();
        ValidateBossContactDamageSettings();
    }

    public void SaveToDefinition()
    {
        SaveToDefinition(enemyDefinition);
    }

    public void SaveToDefinition(EnemyDefinition3D definition)
    {
        if (definition == null)
        {
            return;
        }

        definition.attackMode = attackMode;
        definition.movementMode = movementMode;
        definition.useTransformRightAsMovementAxis = useTransformRightAsMovementAxis;
        definition.movementAxis = movementAxis;
        definition.lockDepthToMovementPlane = lockDepthToMovementPlane;
        definition.moveSpeed = moveSpeed;
        definition.patrolMoveSpeed = patrolMoveSpeed;
        definition.homeStopDistance = homeStopDistance;
        definition.fallbackPatrolHalfWidth = fallbackPatrolHalfWidth;
        definition.patrolRadius = patrolRadius;
        definition.patrolDestinationReachDistance = patrolDestinationReachDistance;
        definition.patrolDestinationMinDistance = patrolDestinationMinDistance;
        definition.patrolObstacleMask = patrolObstacleMask;
        definition.usePatrolObstacleMask = usePatrolObstacleMask;
        definition.patrolObstacleCheckDistance = patrolObstacleCheckDistance;
        definition.patrolObstacleRayHeights = patrolObstacleRayHeights;

        definition.searchRange = searchRange;
        definition.giveUpRange = giveUpRange;
        definition.detectionBoxOffset = detectionBoxOffset;
        definition.detectionBoxHeight = detectionBoxHeight;
        definition.detectionBoxDepth = detectionBoxDepth;
        definition.giveUpBoxPadding = giveUpBoxPadding;
        definition.showDetectionBoxGizmo = showDetectionBoxGizmo;
        definition.onlyShowDetectionBoxWhenSelected = onlyShowDetectionBoxWhenSelected;

        definition.attackRange = attackRange;
        definition.meleeAttackHeight = meleeAttackHeight;
        definition.attackDamage = attackDamage;
        definition.meleeHitSound = meleeHitSound;
        definition.meleeHitSoundVolume = meleeHitSoundVolume;

        definition.projectileDamage = projectileDamage;
        definition.projectileSpeed = projectileSpeed;
        definition.projectileLifetime = projectileLifetime;
        definition.projectileHitSound = projectileHitSound;
        definition.projectileHitSoundVolume = projectileHitSoundVolume;
        definition.projectileLocalOffset = projectileLocalOffset;
        definition.returnSpeed = returnSpeed;

        definition.bossRangedDistance = bossRangedDistance;
        definition.bossRangedDistanceTolerance = bossRangedDistanceTolerance;
        definition.bossContactDamageEnabled = bossContactDamageEnabled;
        definition.bossContactDamage = bossContactDamage;
        definition.bossContactDamageCooldown = bossContactDamageCooldown;
        definition.bossContactDamageBoxSize = bossContactDamageBoxSize;
        definition.bossContactDamageBoxCenter = bossContactDamageBoxCenter;
        definition.bossContactDamageTargetMask = bossContactDamageTargetMask;
        definition.bossProjectileTypes = CloneBossProjectileDefinitions(bossProjectileTypes);

        definition.attackCooldown = attackCooldown;
        definition.useRangedAttackRhythm = useRangedAttackRhythm;
        definition.rangedAttackRhythm = CloneFloatArray(rangedAttackRhythm);
        definition.rangedAttackGroupCooldown = rangedAttackGroupCooldown;
        definition.attackWindup = attackWindup;
        definition.attackLockSeconds = attackLockSeconds;

        definition.launchAwayOnDeath = launchAwayOnDeath;
        definition.deathLaunchSpeed = deathLaunchSpeed;
        definition.deathLaunchUpSpeed = deathLaunchUpSpeed;
        definition.deathSpinDegreesPerSecond = deathSpinDegreesPerSecond;
        definition.deathDestroyDelay = deathDestroyDelay;

        definition.knockbackOnDamage = knockbackOnDamage;
        definition.damageKnockbackForce = damageKnockbackForce;
        definition.damageKnockbackLockSeconds = damageKnockbackLockSeconds;
        definition.airborneHitPauseNormalizedTime = airborneHitPauseNormalizedTime;
        definition.damageLandingRecoverySeconds = damageLandingRecoverySeconds;
        definition.damageGroundCheckDistance = damageGroundCheckDistance;
        definition.damageGroundMask = damageGroundMask;

        definition.respawnAfterCameraLeaves = respawnAfterCameraLeaves;
        definition.respawnCameraAwaySeconds = respawnCameraAwaySeconds;
        definition.respawnViewportPadding = respawnViewportPadding;
    }

    public void SetAttackMode(AttackMode mode)
    {
        attackMode = mode;
        selectedBossProjectileIndex = -1;
        ResetRangedAttackRhythm();
        if (UsesProjectileAttack())
        {
            EnsureProjectileSpawn();
            EnsureProjectileVisualTemplate();
        }
    }

    private void OnValidate()
    {
        if (applyDefinitionInEditor)
        {
            ApplyDefinition();
        }

        AssignDefaultHitSoundsIfNeeded();
        meleeHitSoundVolume = Mathf.Clamp01(meleeHitSoundVolume);
        projectileHitSoundVolume = Mathf.Clamp01(projectileHitSoundVolume);
        ValidateRangedAttackRhythm();
        EnsureDefaultBossProjectileTypes();
        ValidateBossProjectileTypes();
        ValidateBossContactDamageSettings();
        if (Application.isPlaying)
        {
            ConfigureBossContactDamage();
        }
    }

    private void AssignDefaultHitSoundsIfNeeded()
    {
#if UNITY_EDITOR
        AudioClip defaultHitSound = null;
        if (meleeHitSound == null || projectileHitSound == null)
        {
            defaultHitSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultHitSoundPath);
        }

        if (meleeHitSound == null)
        {
            meleeHitSound = defaultHitSound;
        }

        if (projectileHitSound == null)
        {
            projectileHitSound = defaultHitSound;
        }
#endif
    }

    private void Awake()
    {
        if (applyDefinitionOnAwake)
        {
            ApplyDefinition();
        }

        AssignDefaultHitSoundsIfNeeded();
        body = GetComponent<Rigidbody>();
        EnsureBodyCollider();
        health = GetComponent<Health>();
        visualAnimator = GetComponent<EnemyVisualAnimator>();
        if (visualAnimator == null)
        {
            visualAnimator = gameObject.AddComponent<EnemyVisualAnimator>();
        }
        SyncVisualAnimatorMovementMode();

        grounder = GetComponent<EnemyGrounder3D>();
        if (grounder == null)
        {
            grounder = gameObject.AddComponent<EnemyGrounder3D>();
        }

        NormalizeMovementPlane();
        body.constraints = RigidbodyConstraints.FreezeRotation;
        homePosition = transform.position;
        spawnRotation = transform.rotation;
        spawnScale = transform.localScale;
        spawnDirection = direction;
        aliveConstraints = body.constraints;
        aliveUseGravity = body.useGravity;
        aliveIsKinematic = body.isKinematic;
        EnsureDefaultBossProjectileTypes();
        ConfigureBossContactDamage();
        EnsureBossClearOnDeath();
        ConfigurePatrolBounds();
        if (UsesProjectileAttack())
        {
            EnsureProjectileSpawn();
            EnsureProjectileVisualTemplate();
        }

        FindTarget();
        ApplyPlayerCollisionIgnore();
    }

    private static BossProjectileType[] CloneBossProjectileTypes(EnemyBossProjectileDefinition3D[] definitions)
    {
        if (definitions == null)
        {
            return null;
        }

        BossProjectileType[] clones = new BossProjectileType[definitions.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            clones[i] = new BossProjectileType(definitions[i]);
        }

        return clones;
    }

    private static float[] CloneFloatArray(float[] source)
    {
        if (source == null)
        {
            return null;
        }

        float[] clone = new float[source.Length];
        source.CopyTo(clone, 0);
        return clone;
    }

    private static EnemyBossProjectileDefinition3D[] CloneBossProjectileDefinitions(BossProjectileType[] source)
    {
        if (source == null)
        {
            return null;
        }

        EnemyBossProjectileDefinition3D[] clone = new EnemyBossProjectileDefinition3D[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            clone[i] = source[i] != null ? source[i].ToDefinition() : null;
        }

        return clone;
    }

    private void OnEnable()
    {
        if (health != null)
        {
            health.Damaged += ApplyDamageKnockback;
            health.Died += Die;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Damaged -= ApplyDamageKnockback;
            health.Died -= Die;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        RegisterDamageGroundContact(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        RegisterDamageGroundContact(collision);
    }

    private void FixedUpdate()
    {
        SyncVisualAnimatorMovementMode();
        if (deathSequenceStarted)
        {
            return;
        }

        if (health != null && health.IsDead)
        {
            StopMoving();
            return;
        }

        if (UpdateDamageRecoveryLock())
        {
            return;
        }

        if (target == null)
        {
            FindTarget();
        }

        ApplyPlayerCollisionIgnore();

        UpdateState();
        visualAnimator?.SetCombatMode(state == EnemyState.Attack || IsTargetInAttackRange(state == EnemyState.Attack));

        switch (state)
        {
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
            case EnemyState.ReturnHome:
                ReturnHome();
                break;
            default:
                Patrol();
                break;
        }

        grounder?.SnapToGround();
    }

    private void SyncVisualAnimatorMovementMode()
    {
        visualAnimator?.SetUseFree3DAnimations(UsesFree3DMovement);
    }

    private void UpdateState()
    {
        if (state == EnemyState.Attack && Time.time < attackEndTime)
        {
            return;
        }

        if (target == null)
        {
            selectedBossProjectileIndex = -1;
            ResetRangedAttackRhythm();
            state = EnemyState.Patrol;
            return;
        }

        if (state == EnemyState.Attack && IsRangedAttackGroupPlaybackInProgress())
        {
            return;
        }

        bool isEngaged = state == EnemyState.Chase || state == EnemyState.Attack;
        bool canNoticeTarget = IsTargetInsideDetectionBox(searchRange, detectionBoxHeight, detectionBoxDepth, isEngaged);
        if (canNoticeTarget)
        {
            targetLostUntil = Time.time + Mathf.Max(0f, targetLostGraceSeconds);
        }

        bool targetRecentlyVisible = canNoticeTarget || Time.time <= targetLostUntil;
        bool shouldGiveUp = !IsTargetInsideDetectionBox(
            giveUpRange,
            detectionBoxHeight + giveUpBoxPadding,
            detectionBoxDepth + giveUpBoxPadding,
            isEngaged);
        bool targetInAttackRange = IsTargetInAttackRange(state == EnemyState.Attack);

        if (shouldGiveUp && isEngaged && !targetRecentlyVisible)
        {
            selectedBossProjectileIndex = -1;
            ResetRangedAttackRhythm();
            state = EnemyState.ReturnHome;
            return;
        }

        if (targetRecentlyVisible)
        {
            if (targetInAttackRange && attackMode != AttackMode.Ranged)
            {
                state = EnemyState.Attack;
                return;
            }

            if (attackMode == AttackMode.Ranged)
            {
                state = EnemyState.Attack;
                return;
            }

            if (attackMode == AttackMode.Boss)
            {
                state = IsBossReadyToAttack(state == EnemyState.Attack) ? EnemyState.Attack : EnemyState.Chase;
                return;
            }

            state = targetInAttackRange ? EnemyState.Attack : EnemyState.Chase;
            return;
        }

        if (isEngaged)
        {
            state = EnemyState.Chase;
            return;
        }

        if (state == EnemyState.ReturnHome)
        {
            if (GetHorizontalDistanceTo(homePosition) <= homeStopDistance)
            {
                state = EnemyState.Patrol;
            }

            return;
        }

        state = EnemyState.Patrol;
        selectedBossProjectileIndex = -1;
        ResetRangedAttackRhythm();
    }

    private void Patrol()
    {
        if (UsesFree3DMovement)
        {
            PatrolFree3D();
            return;
        }

        if (HasPatrolObstacleAhead())
        {
            TurnAround();
        }

        Move(direction, GetPatrolSpeed());

        float patrolDistance = GetAxisDeltaFromHome(transform.position);
        if (direction < 0 && patrolDistance <= leftPatrolDistance)
        {
            TurnAround();
        }
        else if (direction > 0 && patrolDistance >= rightPatrolDistance)
        {
            TurnAround();
        }
    }

    private void PatrolFree3D()
    {
        if (!hasPatrolDestination
            || GetPlanarDistanceTo(patrolDestination) <= Mathf.Max(0.05f, patrolDestinationReachDistance)
            || HasPatrolObstacleAhead(GetPlanarDirectionTo(patrolDestination)))
        {
            PickPatrolDestination();
        }

        Move(GetPlanarDirectionTo(patrolDestination), GetPatrolSpeed());
    }

    private void Chase()
    {
        if (target == null)
        {
            state = EnemyState.ReturnHome;
            return;
        }

        Vector3 targetDirection = GetMoveDirectionToTarget();
        if (attackMode == AttackMode.Boss)
        {
            Move(GetBossChaseDirection(targetDirection, GetHorizontalDistanceToTarget()), moveSpeed * 1.25f);
            return;
        }

        Move(targetDirection, moveSpeed * 1.25f);
    }

    private void Attack()
    {
        StopMoving();

        FaceTarget();
        ScheduleInitialRangedAttackDelayIfNeeded();

        if (!attackResolved && Time.time >= attackResolveTime)
        {
            if (currentAttackUsesRanged)
            {
                FireProjectile();
                ScheduleNextRangedAttackFromProjectileTime();
            }
            else
            {
                ResolveMeleeAttackHit();
            }

            attackResolved = true;
        }

        if (Time.time >= nextAttackTime && Time.time >= attackEndTime)
        {
            FaceTarget();
            visualAnimator = visualAnimator != null ? visualAnimator : GetComponent<EnemyVisualAnimator>();
            currentAttackUsesRanged = IsRangedAttackGroupPlaybackInProgress() || ShouldUseRangedAttackNow();
            float cooldown = currentAttackUsesRanged ? ConsumeQueuedRangedAttackInterval() : attackCooldown;
            float attackAnimationLength = visualAnimator != null ? visualAnimator.GetAttackAnimationLength() : 0f;
            float attackSpeedMultiplier = GetAttackSpeedMultiplier(currentAttackUsesRanged, cooldown, attackAnimationLength);
            float animationLockSeconds = GetScaledAttackTime(attackAnimationLength, attackSpeedMultiplier);
            float windupSeconds = GetScaledAttackTime(attackWindup, attackSpeedMultiplier);
            float lockSeconds = Mathf.Max(GetScaledAttackTime(attackLockSeconds, attackSpeedMultiplier), animationLockSeconds, windupSeconds);
            if (visualAnimator != null)
            {
                visualAnimator.PlayAttack(attackSpeedMultiplier, lockSeconds);
            }

            attackResolved = false;
            attackResolveTime = Time.time + windupSeconds;
            attackEndTime = Time.time + lockSeconds;
            if (currentAttackUsesRanged && useRangedAttackRhythm)
            {
                nextAttackTime = float.PositiveInfinity;
            }
            else
            {
                nextAttackTime = Time.time + GetNextAttackInterval(currentAttackUsesRanged, cooldown, lockSeconds);
            }
        }

        StopMoving();
    }

    private float GetAttackSpeedMultiplier(bool isRangedAttack, float attackInterval, float attackAnimationLength)
    {
        if (!isRangedAttack || !useRangedAttackRhythm || attackInterval <= 0f)
        {
            return 1f;
        }

        float baseAttackCycle = Mathf.Max(attackCooldown, attackLockSeconds, attackWindup, attackAnimationLength);
        if (baseAttackCycle <= 0f)
        {
            return 1f;
        }

        return Mathf.Max(1f, baseAttackCycle / Mathf.Max(0.01f, attackInterval));
    }

    private float GetScaledAttackTime(float seconds, float attackSpeedMultiplier)
    {
        return Mathf.Max(0f, seconds) / Mathf.Max(0.01f, attackSpeedMultiplier);
    }

    private float GetNextAttackInterval(bool isRangedAttack, float cooldown, float lockSeconds)
    {
        if (isRangedAttack && useRangedAttackRhythm)
        {
            return Mathf.Max(0.01f, cooldown);
        }

        return Mathf.Max(cooldown, lockSeconds);
    }

    private void ReturnHome()
    {
        if (GetHorizontalDistanceTo(homePosition) <= homeStopDistance)
        {
            StopMoving();
            state = EnemyState.Patrol;
            selectedBossProjectileIndex = -1;
            ResetRangedAttackRhythm();
            hasPatrolDestination = false;
            return;
        }

        Move(GetMoveDirectionTo(homePosition), UsesProjectileAttack() ? returnSpeed : moveSpeed);
    }

    private void ResolveMeleeAttackHit()
    {
        if (target == null)
        {
            return;
        }

        if (!IsTargetInMeleeStrikeRange())
        {
            return;
        }

        if (target.TryGetComponent(out Health targetHealth))
        {
            if (targetHealth.TryTakeDamage(attackDamage, transform.position))
            {
                PlayHitSound(meleeHitSound, meleeHitSoundVolume, target.position);
            }
        }
    }

    private void FireProjectile()
    {
        FaceTarget();
        EnsureProjectileSpawn();
        EnsureProjectileVisualTemplate();

        int projectileIndex;
        BossProjectileType bossProjectileType = ResolveBossProjectileType(out projectileIndex);
        Transform visualTemplate = ResolveProjectileTemplate(bossProjectileType, projectileIndex);
        bool hasVisualTemplate = visualTemplate != null;
        string projectileName = bossProjectileType != null ? bossProjectileType.Name : "Projectile";
        GameObject projectileObject = new GameObject(string.IsNullOrEmpty(projectileName) ? "Projectile" : projectileName);
        projectileObject.transform.position = projectileSpawn.position;
        projectileObject.transform.rotation = Quaternion.identity;
        projectileObject.transform.localScale = hasVisualTemplate ? Vector3.one : new Vector3(0.42f, 0.42f, 0.42f);

        ReflectableProjectile3D projectile = projectileObject.AddComponent<ReflectableProjectile3D>();
        if (hasVisualTemplate)
        {
            projectile.UseVisualTemplate(visualTemplate);
        }

        int resolvedDamage = bossProjectileType != null ? bossProjectileType.ResolveDamage(projectileDamage) : projectileDamage;
        float resolvedSpeed = bossProjectileType != null ? bossProjectileType.ResolveSpeed(projectileSpeed) : projectileSpeed;
        float resolvedLifetime = bossProjectileType != null ? bossProjectileType.ResolveLifetime(projectileLifetime) : projectileLifetime;
        bool canBeReflected = bossProjectileType == null || bossProjectileType.CanBeReflected;
        projectile.Launch(GetProjectileLaunchDirection(), gameObject, resolvedDamage, resolvedSpeed, resolvedLifetime, canBeReflected);
        projectile.ConfigureHitSound(
            bossProjectileType != null ? bossProjectileType.ResolveHitSound(projectileHitSound) : projectileHitSound,
            bossProjectileType != null ? bossProjectileType.ResolveHitSoundVolume(projectileHitSoundVolume) : projectileHitSoundVolume);
        selectedBossProjectileIndex = -1;
    }

    private float ConsumeQueuedRangedAttackInterval()
    {
        if (!useRangedAttackRhythm)
        {
            return attackCooldown;
        }

        if (queuedRangedAttackInterval > 0f)
        {
            float interval = queuedRangedAttackInterval;
            queuedRangedAttackInterval = -1f;
            return interval;
        }

        return PeekNextRangedAttackInterval();
    }

    private void ScheduleInitialRangedAttackDelayIfNeeded()
    {
        if (rangedAttackRhythmInitialDelayScheduled
            || !useRangedAttackRhythm
            || !UsesProjectileAttack()
            || !ShouldUseRangedAttackNow())
        {
            return;
        }

        float interval = GetNextRangedAttackInterval();
        queuedRangedAttackInterval = interval;
        visualAnimator = visualAnimator != null ? visualAnimator : GetComponent<EnemyVisualAnimator>();
        float attackAnimationLength = visualAnimator != null ? visualAnimator.GetAttackAnimationLength() : 0f;
        float attackSpeedMultiplier = GetAttackSpeedMultiplier(true, interval, attackAnimationLength);
        float windupSeconds = GetScaledAttackTime(attackWindup, attackSpeedMultiplier);
        nextAttackTime = Time.time + Mathf.Max(0.01f, interval - windupSeconds);
        rangedAttackRhythmInitialDelayScheduled = true;
        rangedAttackGroupPlaybackActive = true;
    }

    private void ScheduleNextRangedAttackFromProjectileTime()
    {
        if (!useRangedAttackRhythm)
        {
            return;
        }

        bool currentShotFinishedGroup = IsRangedAttackRhythmAtGroupStart();
        float groupCooldown = currentShotFinishedGroup
            ? Mathf.Max(0f, rangedAttackGroupCooldown)
            : 0f;
        float interval = GetNextRangedAttackInterval();
        queuedRangedAttackInterval = interval;
        visualAnimator = visualAnimator != null ? visualAnimator : GetComponent<EnemyVisualAnimator>();
        float attackAnimationLength = visualAnimator != null ? visualAnimator.GetAttackAnimationLength() : 0f;
        float attackSpeedMultiplier = GetAttackSpeedMultiplier(true, interval, attackAnimationLength);
        float windupSeconds = GetScaledAttackTime(attackWindup, attackSpeedMultiplier);
        nextAttackTime = Time.time + groupCooldown + Mathf.Max(0.01f, interval - windupSeconds);
        rangedAttackGroupPlaybackActive = !currentShotFinishedGroup;
    }

    private float PeekNextRangedAttackInterval()
    {
        if (!useRangedAttackRhythm || rangedAttackRhythm == null || rangedAttackRhythm.Length == 0)
        {
            return attackCooldown;
        }

        if (rangedAttackRhythmIndex < 0 || rangedAttackRhythmIndex >= rangedAttackRhythm.Length)
        {
            rangedAttackRhythmIndex = 0;
        }

        for (int i = 0; i < rangedAttackRhythm.Length; i++)
        {
            int index = (rangedAttackRhythmIndex + i) % rangedAttackRhythm.Length;
            float interval = rangedAttackRhythm[index];
            if (interval > 0f)
            {
                return interval;
            }
        }

        return attackCooldown;
    }

    private float GetNextRangedAttackInterval()
    {
        if (!useRangedAttackRhythm || rangedAttackRhythm == null || rangedAttackRhythm.Length == 0)
        {
            return attackCooldown;
        }

        if (rangedAttackRhythmIndex < 0 || rangedAttackRhythmIndex >= rangedAttackRhythm.Length)
        {
            rangedAttackRhythmIndex = 0;
        }

        for (int i = 0; i < rangedAttackRhythm.Length; i++)
        {
            int index = rangedAttackRhythmIndex;
            rangedAttackRhythmIndex = (rangedAttackRhythmIndex + 1) % rangedAttackRhythm.Length;

            float interval = rangedAttackRhythm[index];
            if (interval > 0f)
            {
                return interval;
            }
        }

        return attackCooldown;
    }

    private void ResetRangedAttackRhythm()
    {
        rangedAttackRhythmIndex = 0;
        queuedRangedAttackInterval = -1f;
        currentAttackUsesRanged = false;
        rangedAttackRhythmInitialDelayScheduled = false;
        rangedAttackGroupPlaybackActive = false;
    }

    private bool IsRangedAttackRhythmAtGroupStart()
    {
        if (!rangedAttackRhythmInitialDelayScheduled)
        {
            return false;
        }

        if (rangedAttackRhythm == null || rangedAttackRhythm.Length == 0)
        {
            return true;
        }

        return rangedAttackRhythmIndex == 0;
    }

    private bool IsRangedAttackGroupPlaybackInProgress()
    {
        return useRangedAttackRhythm
            && UsesProjectileAttack()
            && (rangedAttackGroupPlaybackActive || (currentAttackUsesRanged && !attackResolved));
    }

    private void ValidateRangedAttackRhythm()
    {
        rangedAttackGroupCooldown = Mathf.Max(0f, rangedAttackGroupCooldown);

        if (rangedAttackRhythm == null)
        {
            return;
        }

        for (int i = 0; i < rangedAttackRhythm.Length; i++)
        {
            rangedAttackRhythm[i] = Mathf.Max(0.01f, rangedAttackRhythm[i]);
        }
    }

    private Vector3 GetProjectileLaunchDirection()
    {
        if (target != null)
        {
            Vector3 targetDirection = GetMoveDirectionToTarget();
            if (targetDirection.sqrMagnitude > 0.0001f)
            {
                return targetDirection;
            }
        }

        return GetFacingDirection();
    }

    private static void PlayHitSound(AudioClip clip, float volume, Vector3 position)
    {
        if (clip == null || volume <= 0f)
        {
            return;
        }

        SideScrollerSfxPlayer.PlayOneShot(clip, volume);
    }

    private bool IsTargetInAttackRange(bool isCurrentlyAttacking)
    {
        if (target == null)
        {
            return false;
        }

        float horizontalDistance = GetHorizontalDistanceToTarget();
        if (!IsTargetWithinVerticalRange(transform.position.y, meleeAttackHeight))
        {
            return false;
        }

        if (attackMode == AttackMode.Ranged)
        {
            return IsTargetInsideDetectionBox(searchRange, detectionBoxHeight, detectionBoxDepth, isCurrentlyAttacking);
        }

        if (attackMode == AttackMode.Boss)
        {
            if (state != EnemyState.Chase && state != EnemyState.Attack)
            {
                return horizontalDistance <= GetBufferedAttackRange(isCurrentlyAttacking);
            }

            return horizontalDistance <= GetBufferedAttackRange(isCurrentlyAttacking) || IsBossAtRangedDistance(horizontalDistance, isCurrentlyAttacking);
        }

        return horizontalDistance <= GetBufferedAttackRange(isCurrentlyAttacking);
    }

    private float GetPatrolSpeed()
    {
        return UsesProjectileAttack() ? patrolMoveSpeed : moveSpeed;
    }

    private bool UsesProjectileAttack()
    {
        return attackMode == AttackMode.Ranged || attackMode == AttackMode.Boss;
    }

    private bool ShouldUseRangedAttackNow()
    {
        if (attackMode == AttackMode.Ranged)
        {
            return true;
        }

        if (attackMode != AttackMode.Boss || target == null)
        {
            return false;
        }

        bool shouldUseRanged = GetHorizontalDistanceToTarget() > attackRange;
        if (!shouldUseRanged)
        {
            selectedBossProjectileIndex = -1;
        }

        return shouldUseRanged;
    }

    private bool IsBossReadyToAttack(bool isCurrentlyAttacking)
    {
        if (target == null)
        {
            return false;
        }

        float horizontalDistance = GetHorizontalDistanceToTarget();
        if (horizontalDistance <= GetBufferedAttackRange(isCurrentlyAttacking))
        {
            return true;
        }

        return IsBossAtRangedDistance(horizontalDistance, isCurrentlyAttacking);
    }

    private bool IsBossAtRangedDistance(float horizontalDistance, bool isCurrentlyAttacking)
    {
        float desiredDistance = Mathf.Max(attackRange + 0.05f, GetCurrentBossRangedDistance());
        float tolerance = Mathf.Max(0.01f, bossRangedDistanceTolerance + (isCurrentlyAttacking ? EdgeHysteresis : -EdgeHysteresis));
        return Mathf.Abs(horizontalDistance - desiredDistance) <= tolerance;
    }

    private Vector3 GetBossChaseDirection(Vector3 targetDirection, float horizontalDistance)
    {
        if (targetDirection.sqrMagnitude < 0.0001f)
        {
            targetDirection = GetFacingDirection();
        }

        float desiredDistance = Mathf.Max(attackRange + 0.05f, GetCurrentBossRangedDistance());
        float tolerance = Mathf.Max(0.01f, bossRangedDistanceTolerance);
        if (horizontalDistance > desiredDistance + tolerance)
        {
            return targetDirection;
        }

        if (horizontalDistance < desiredDistance - tolerance)
        {
            return -targetDirection;
        }

        return Vector3.zero;
    }

    private BossProjectileType ResolveBossProjectileType(out int projectileIndex)
    {
        projectileIndex = -1;
        if (attackMode != AttackMode.Boss)
        {
            return null;
        }

        EnsureDefaultBossProjectileTypes();
        if (bossProjectileTypes == null || bossProjectileTypes.Length == 0)
        {
            return null;
        }

        projectileIndex = EnsureSelectedBossProjectileIndex();
        return bossProjectileTypes[projectileIndex];
    }

    private Transform ResolveProjectileTemplate(BossProjectileType bossProjectileType, int projectileIndex)
    {
        if (attackMode != AttackMode.Boss)
        {
            return projectileVisualTemplate;
        }

        if (bossProjectileType != null && bossProjectileType.VisualTemplate != null)
        {
            return bossProjectileType.VisualTemplate;
        }

        if (projectileVisualTemplates == null || projectileVisualTemplates.Length == 0)
        {
            return projectileVisualTemplate;
        }

        int templateIndex = Mathf.Max(0, projectileIndex % projectileVisualTemplates.Length);
        return projectileVisualTemplates[templateIndex];
    }

    private float GetCurrentBossRangedDistance()
    {
        if (attackMode != AttackMode.Boss)
        {
            return bossRangedDistance;
        }

        BossProjectileType bossProjectileType = GetSelectedBossProjectileType();
        return bossProjectileType != null ? bossProjectileType.ResolveRangedDistance(bossRangedDistance) : bossRangedDistance;
    }

    private BossProjectileType GetSelectedBossProjectileType()
    {
        EnsureDefaultBossProjectileTypes();
        if (bossProjectileTypes == null || bossProjectileTypes.Length == 0)
        {
            return null;
        }

        int index = EnsureSelectedBossProjectileIndex();
        return bossProjectileTypes[index];
    }

    private int EnsureSelectedBossProjectileIndex()
    {
        EnsureDefaultBossProjectileTypes();
        if (bossProjectileTypes == null || bossProjectileTypes.Length == 0)
        {
            selectedBossProjectileIndex = -1;
            return -1;
        }

        if (selectedBossProjectileIndex < 0 || selectedBossProjectileIndex >= bossProjectileTypes.Length)
        {
            selectedBossProjectileIndex = Random.Range(0, bossProjectileTypes.Length);
        }

        return selectedBossProjectileIndex;
    }

    private void EnsureDefaultBossProjectileTypes()
    {
        if (bossProjectileTypes != null && bossProjectileTypes.Length > 0)
        {
            return;
        }

        bossProjectileTypes = new[]
        {
            new BossProjectileType("Fireball", false, 5.8f),
            new BossProjectileType("IronBall", true, 4f)
        };
    }

    private void ValidateBossProjectileTypes()
    {
        if (bossProjectileTypes == null)
        {
            return;
        }

        for (int i = 0; i < bossProjectileTypes.Length; i++)
        {
            if (bossProjectileTypes[i] != null)
            {
                bossProjectileTypes[i].Validate(projectileDamage, projectileSpeed, projectileLifetime, bossRangedDistance);
            }
        }
    }

    private void ConfigureBossContactDamage()
    {
        if (attackMode != AttackMode.Boss)
        {
            return;
        }

        ValidateBossContactDamageSettings();

        Transform contactTransform = transform.Find(ContactDamageObjectName);
        GameObject contactObject;
        if (contactTransform != null)
        {
            contactObject = contactTransform.gameObject;
        }
        else
        {
            contactObject = new GameObject(ContactDamageObjectName);
            contactTransform = contactObject.transform;
            contactTransform.SetParent(transform, false);
        }

        int defaultLayer = LayerMask.NameToLayer("Default");
        contactObject.layer = defaultLayer >= 0 ? defaultLayer : gameObject.layer;
        contactTransform.localPosition = Vector3.zero;
        contactTransform.localRotation = Quaternion.identity;
        contactTransform.localScale = Vector3.one;

        BoxCollider contactCollider = contactObject.GetComponent<BoxCollider>();
        if (contactCollider == null)
        {
            contactCollider = contactObject.AddComponent<BoxCollider>();
        }

        contactCollider.isTrigger = true;
        contactCollider.size = bossContactDamageBoxSize;
        contactCollider.center = bossContactDamageBoxCenter;

        DamageOnTouch damageOnTouch = contactObject.GetComponent<DamageOnTouch>();
        if (damageOnTouch == null)
        {
            damageOnTouch = contactObject.AddComponent<DamageOnTouch>();
        }

        damageOnTouch.Configure(
            bossContactDamageEnabled,
            bossContactDamage,
            ResolveBossContactDamageTargetMask(),
            bossContactDamageCooldown);
    }

    private void ValidateBossContactDamageSettings()
    {
        bossContactDamage = Mathf.Max(0, bossContactDamage);
        bossContactDamageCooldown = Mathf.Max(0.01f, bossContactDamageCooldown);
        bossContactDamageBoxSize = new Vector3(
            Mathf.Max(0.1f, Mathf.Abs(bossContactDamageBoxSize.x)),
            Mathf.Max(0.1f, Mathf.Abs(bossContactDamageBoxSize.y)),
            Mathf.Max(0.1f, Mathf.Abs(bossContactDamageBoxSize.z)));

        if (bossContactDamageTargetMask.value == 0)
        {
            bossContactDamageTargetMask = ResolvePlayerLayerMask();
        }
    }

    private LayerMask ResolveBossContactDamageTargetMask()
    {
        return bossContactDamageTargetMask.value != 0 ? bossContactDamageTargetMask : ResolvePlayerLayerMask();
    }

    private static LayerMask ResolvePlayerLayerMask()
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        return playerLayer >= 0 ? 1 << playerLayer : 1 << 8;
    }

    private void EnsureBossClearOnDeath()
    {
        if (attackMode != AttackMode.Boss || GetComponent<BossClearOnDeath3D>() != null)
        {
            return;
        }

        gameObject.AddComponent<BossClearOnDeath3D>();
    }

    private bool IsDamageKnockbackLocked => waitingForDamageLanding
        || Time.time < damageKnockbackLockedUntil
        || Time.time < damageLandingRecoveryUntil;

    private void ApplyDamageKnockback(int amount, Vector3 damageSourcePosition)
    {
        if (amount <= 0 || body == null || deathSequenceStarted)
        {
            return;
        }

        Vector3 faceDirection = GetMoveDirectionTo(damageSourcePosition);
        if (faceDirection.sqrMagnitude < 0.0001f)
        {
            faceDirection = GetFacingDirection();
        }

        Vector3 knockbackDirection = -faceDirection;
        Face(faceDirection);
        visualAnimator?.ResumeHit();
        visualAnimator?.PlayHit();
        visualAnimator?.PauseHitAtNormalizedTime(airborneHitPauseNormalizedTime);

        if (!knockbackOnDamage)
        {
            return;
        }

        float horizontalSpeed = Mathf.Abs(damageKnockbackForce.x);
        float upwardSpeed = Mathf.Abs(damageKnockbackForce.y);
        Vector3 velocity = body.linearVelocity;
        velocity = UsesFree3DMovement ? RemoveHorizontalVelocity(velocity) : RemoveAxisVelocity(velocity, movementAxis);
        velocity += knockbackDirection * horizontalSpeed;
        velocity.y = Mathf.Max(velocity.y, upwardSpeed);
        body.linearVelocity = velocity;

        damageKnockbackLockedUntil = Time.time + Mathf.Max(0f, damageKnockbackLockSeconds);
        damageLandingRecoveryUntil = 0f;
        damageGroundContactUntil = 0f;
        waitingForDamageLanding = true;
        SuspendGrounderForDamage();
        EnableGravityForDamage();
        attackResolved = true;
        attackEndTime = Mathf.Max(attackEndTime, damageKnockbackLockedUntil);
    }

    private bool UpdateDamageRecoveryLock()
    {
        if (!IsDamageKnockbackLocked)
        {
            RestoreGrounderAfterDamage();
            visualAnimator?.ResumeHit();
            return false;
        }

        SnapToMovementPlane();
        if (!waitingForDamageLanding)
        {
            return true;
        }

        if (!IsGroundedForDamageRecovery())
        {
            return true;
        }

        waitingForDamageLanding = false;
        damageLandingRecoveryUntil = Time.time + Mathf.Max(0f, damageLandingRecoverySeconds);
        damageKnockbackLockedUntil = Mathf.Max(damageKnockbackLockedUntil, damageLandingRecoveryUntil);
        RestoreGrounderAfterDamage();
        visualAnimator?.ResumeHit();
        StopMoving();
        return true;
    }

    private void EnableGravityForDamage()
    {
        if (body == null)
        {
            return;
        }

        if (!damageGravityStateStored)
        {
            useGravityBeforeDamage = body.useGravity;
            damageGravityStateStored = true;
        }

        body.useGravity = true;
    }

    private void RegisterDamageGroundContact(Collision collision)
    {
        if (!waitingForDamageLanding || collision == null)
        {
            return;
        }

        EnsureBodyCollider();
        Bounds bounds = bodyCollider != null ? bodyCollider.bounds : new Bounds(transform.position, Vector3.one);
        float bottomContactY = bounds.min.y + Mathf.Max(0.02f, damageGroundCheckDistance + 0.08f);
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            bool isFloorNormal = contact.normal.y > 0.35f;
            bool isNearBottom = contact.point.y <= bottomContactY;
            if (contact.otherCollider != null
                && !contact.otherCollider.transform.IsChildOf(transform)
                && (isFloorNormal || isNearBottom))
            {
                damageGroundContactUntil = Time.time + 0.12f;
                return;
            }
        }
    }

    private void SuspendGrounderForDamage()
    {
        if (grounder == null)
        {
            return;
        }

        if (!damageGrounderStateStored)
        {
            grounderWasEnabledBeforeDamage = grounder.enabled;
            damageGrounderStateStored = true;
        }

        grounder.enabled = false;
    }

    private void RestoreGrounderAfterDamage()
    {
        if (grounder != null && damageGrounderStateStored)
        {
            grounder.enabled = grounderWasEnabledBeforeDamage;
        }

        damageGrounderStateStored = false;
        RestoreGravityAfterDamage();
    }

    private void RestoreGravityAfterDamage()
    {
        if (body != null && damageGravityStateStored)
        {
            body.useGravity = useGravityBeforeDamage;
        }

        damageGravityStateStored = false;
    }

    private bool IsGroundedForDamageRecovery()
    {
        if (Time.time <= damageGroundContactUntil)
        {
            return body == null || body.linearVelocity.y <= 0.1f;
        }

        EnsureDamageGroundMask();
        EnsureBodyCollider();
        if (bodyCollider == null)
        {
            return false;
        }

        Bounds bounds = bodyCollider.bounds;
        Vector3 origin = bounds.center + Vector3.up * 0.05f;
        float rayDistance = bounds.extents.y + Mathf.Max(0.01f, damageGroundCheckDistance) + 0.05f;
        return HasDamageGroundBelow(origin, rayDistance, damageGroundMask)
            || HasDamageGroundBelow(origin, rayDistance, Physics.DefaultRaycastLayers);
    }

    private bool HasDamageGroundBelow(Vector3 origin, float rayDistance, int layerMask)
    {
        int hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, damageGroundHits, rayDistance, layerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = damageGroundHits[i].collider;
            if (hitCollider != null && !hitCollider.transform.IsChildOf(transform) && !hitCollider.isTrigger)
            {
                return body == null || body.linearVelocity.y <= 0.1f;
            }
        }

        return false;
    }

    private void EnsureDamageGroundMask()
    {
        if (damageGroundMask.value != 0)
        {
            return;
        }

        int groundLayer = LayerMask.NameToLayer("Ground");
        damageGroundMask = groundLayer >= 0 ? LayerMask.GetMask("Ground") : Physics.DefaultRaycastLayers;
    }

    private void ConfigurePatrolBounds()
    {
        float halfWidth = Mathf.Max(0.25f, fallbackPatrolHalfWidth);
        leftPatrolDistance = -halfWidth;
        rightPatrolDistance = halfWidth;
        hasPatrolDestination = false;
    }

    private void NormalizeMovementPlane()
    {
        if (useTransformRightAsMovementAxis)
        {
            movementAxis = transform.right;
        }

        movementAxis = FlattenHorizontal(movementAxis);
        depthAxis = FlattenHorizontal(Vector3.Cross(movementAxis, Vector3.up));
        lockedDepth = Vector3.Dot(transform.position, depthAxis);
    }

    private void PickPatrolDestination()
    {
        float radius = Mathf.Max(0.25f, patrolRadius);
        float minDistance = Mathf.Clamp(patrolDestinationMinDistance, 0f, radius);
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDistance, radius);
        if (randomCircle.sqrMagnitude < 0.0001f)
        {
            randomCircle = Vector2.right * radius;
        }

        patrolDestination = homePosition + new Vector3(randomCircle.x, 0f, randomCircle.y);
        patrolDestination.y = transform.position.y;
        hasPatrolDestination = true;
    }

    private void EnsureBodyCollider()
    {
        if (bodyCollider != null && bodyCollider.enabled && !bodyCollider.isTrigger)
        {
            return;
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider candidate = colliders[i];
            if (candidate != null && candidate.enabled && !candidate.isTrigger)
            {
                bodyCollider = candidate;
                return;
            }
        }
    }

    private bool HasPatrolObstacleAhead()
    {
        return HasPatrolObstacleAhead(movementAxis * direction);
    }

    private bool HasPatrolObstacleAhead(Vector3 moveDirection)
    {
        EnsureBodyCollider();
        if (bodyCollider == null)
        {
            return false;
        }

        Bounds bounds = bodyCollider.bounds;
        moveDirection = FlattenHorizontalOrZero(moveDirection);
        if (moveDirection.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        float forwardExtent = Mathf.Abs(bounds.extents.x * moveDirection.x)
            + Mathf.Abs(bounds.extents.z * moveDirection.z);
        float rayDistance = Mathf.Max(0.02f, patrolObstacleCheckDistance);
        Vector3 baseOrigin = bounds.center + moveDirection * (forwardExtent + 0.02f);

        return CastObstacleRay(baseOrigin, patrolObstacleRayHeights.x, rayDistance, moveDirection, bounds)
            || CastObstacleRay(baseOrigin, patrolObstacleRayHeights.y, rayDistance, moveDirection, bounds)
            || CastObstacleRay(baseOrigin, patrolObstacleRayHeights.z, rayDistance, moveDirection, bounds);
    }

    private bool CastObstacleRay(Vector3 baseOrigin, float heightRatio, float rayDistance, Vector3 moveDirection, Bounds bounds)
    {
        float clampedRatio = Mathf.Clamp01(heightRatio);
        Vector3 origin = baseOrigin;
        origin.y = Mathf.Lerp(bounds.min.y + 0.05f, bounds.max.y - 0.05f, clampedRatio);
        int layerMask = usePatrolObstacleMask ? patrolObstacleMask.value : Physics.DefaultRaycastLayers;
        int hitCount = Physics.RaycastNonAlloc(origin, moveDirection, obstacleHits, rayDistance, layerMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hitCollider = obstacleHits[i].collider;
            if (IsBlockingPatrolObstacle(hitCollider))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsBlockingPatrolObstacle(Collider hitCollider)
    {
        if (hitCollider == null || hitCollider.isTrigger || hitCollider.transform.IsChildOf(transform))
        {
            return false;
        }

        return true;
    }

    private bool IsTargetInsideDetectionBox(float axisRange, float boxHeight, float boxDepth, bool isCurrentlyEngaged)
    {
        if (target == null)
        {
            return false;
        }

        Vector3 center = GetDetectionBoxCenter();
        Vector3 delta = target.position - center;
        float halfHeight = Mathf.Max(0.01f, boxHeight * 0.5f);
        if (UsesFree3DMovement)
        {
            return GetTargetHorizontalDistanceFrom(center) <= GetBufferedAxisRange(axisRange, isCurrentlyEngaged)
                && IsTargetWithinVerticalRange(center.y, halfHeight);
        }

        float halfAxis = Mathf.Max(0.01f, axisRange);
        float halfDepth = Mathf.Max(0.01f, boxDepth * 0.5f);

        return Mathf.Abs(Vector3.Dot(delta, movementAxis)) <= halfAxis
            && Mathf.Abs(delta.y) <= halfHeight
            && Mathf.Abs(Vector3.Dot(delta, depthAxis)) <= halfDepth;
    }

    private float GetBufferedAxisRange(float axisRange, bool expand)
    {
        float buffer = expand ? EdgeHysteresis : -EdgeHysteresis;
        return Mathf.Max(0.01f, axisRange + buffer);
    }

    private float GetBufferedAttackRange(bool isCurrentlyAttacking)
    {
        return Mathf.Max(0.01f, attackRange + (isCurrentlyAttacking ? EdgeHysteresis : -EdgeHysteresis));
    }

    private Vector3 GetDetectionBoxCenter()
    {
        if (UsesFree3DMovement)
        {
            return transform.position
                + Vector3.up * detectionBoxOffset.y;
        }

        return transform.position
            + movementAxis * detectionBoxOffset.x
            + Vector3.up * detectionBoxOffset.y
            + depthAxis * detectionBoxOffset.z;
    }

    private float GetPlanarDistanceTo(Vector3 worldPosition)
    {
        Vector3 delta = worldPosition - transform.position;
        delta.y = 0f;
        return delta.magnitude;
    }

    private float GetHorizontalDistanceTo(Vector3 worldPosition)
    {
        return UsesFree3DMovement ? GetPlanarDistanceTo(worldPosition) : Mathf.Abs(GetAxisDelta(worldPosition));
    }

    private float GetHorizontalDistanceToTarget()
    {
        return GetTargetHorizontalDistanceFrom(transform.position);
    }

    private float GetTargetHorizontalDistanceFrom(Vector3 origin)
    {
        if (TryGetTargetFocusPoint(out Vector3 targetPoint))
        {
            return UsesFree3DMovement ? GetPlanarDistanceBetween(origin, targetPoint) : Mathf.Abs(Vector3.Dot(targetPoint - origin, movementAxis));
        }

        return target != null ? GetHorizontalDistanceTo(target.position) : float.MaxValue;
    }

    private static float GetPlanarDistanceBetween(Vector3 a, Vector3 b)
    {
        Vector3 delta = b - a;
        delta.y = 0f;
        return delta.magnitude;
    }

    private Vector3 GetPlanarDirectionTo(Vector3 worldPosition)
    {
        return FlattenHorizontalOrZero(worldPosition - transform.position);
    }

    private Vector3 GetMoveDirectionToTarget()
    {
        if (target != null && TryGetTargetFocusPoint(out Vector3 targetPoint))
        {
            Vector3 direction = UsesFree3DMovement
                ? FlattenHorizontalOrZero(targetPoint - transform.position)
                : movementAxis * Mathf.Sign(Vector3.Dot(targetPoint - transform.position, movementAxis));
            if (direction.sqrMagnitude > 0.0001f)
            {
                return direction;
            }
        }

        return target != null ? GetMoveDirectionTo(target.position) : Vector3.zero;
    }

    private Vector3 GetMoveDirectionTo(Vector3 worldPosition)
    {
        if (UsesFree3DMovement)
        {
            return GetPlanarDirectionTo(worldPosition);
        }

        float axisDelta = GetAxisDelta(worldPosition);
        if (Mathf.Abs(axisDelta) < 0.01f)
        {
            return Vector3.zero;
        }

        return movementAxis * Mathf.Sign(axisDelta);
    }

    private bool TryGetTargetFocusPoint(out Vector3 focusPoint)
    {
        focusPoint = default;
        if (target == null)
        {
            return false;
        }

        if (TryGetTargetBodyBounds(out Bounds bounds))
        {
            focusPoint = bounds.center;
            return true;
        }

        focusPoint = target.position;
        return true;
    }

    private Vector3 GetFacingDirection()
    {
        if (UsesFree3DMovement)
        {
            Vector3 facing = FlattenHorizontalOrZero(transform.TransformDirection(Vector3.right));
            return facing.sqrMagnitude > 0.0001f ? facing : movementAxis * direction;
        }

        return movementAxis * direction;
    }

    private bool IsTargetInMeleeStrikeRange()
    {
        return target != null
            && GetHorizontalDistanceToTarget() <= attackRange
            && IsTargetWithinVerticalRange(transform.position.y, meleeAttackHeight);
    }

    private bool IsTargetWithinVerticalRange(float centerY, float halfHeight)
    {
        float minY = centerY - Mathf.Max(0.01f, halfHeight);
        float maxY = centerY + Mathf.Max(0.01f, halfHeight);
        if (TryGetTargetBodyBounds(out Bounds bounds))
        {
            return bounds.max.y >= minY && bounds.min.y <= maxY;
        }

        return target != null && target.position.y >= minY && target.position.y <= maxY;
    }

    private bool TryGetClosestTargetBodyPoint(Vector3 origin, out Vector3 closestPoint)
    {
        closestPoint = default;
        EnsureTargetBodyColliders();
        bool found = false;
        float closestDistance = float.MaxValue;
        for (int i = 0; i < targetBodyColliders.Length; i++)
        {
            Collider targetCollider = targetBodyColliders[i];
            if (!IsUsableTargetBodyCollider(targetCollider))
            {
                continue;
            }

            Vector3 point = targetCollider.ClosestPoint(origin);
            float distance = (point - origin).sqrMagnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = point;
                found = true;
            }
        }

        return found;
    }

    private bool TryGetTargetBodyBounds(out Bounds bounds)
    {
        bounds = default;
        EnsureTargetBodyColliders();
        bool found = false;
        for (int i = 0; i < targetBodyColliders.Length; i++)
        {
            Collider targetCollider = targetBodyColliders[i];
            if (!IsUsableTargetBodyCollider(targetCollider))
            {
                continue;
            }

            if (!found)
            {
                bounds = targetCollider.bounds;
                found = true;
                continue;
            }

            bounds.Encapsulate(targetCollider.bounds);
        }

        return found;
    }

    private void EnsureTargetBodyColliders()
    {
        if (target == null)
        {
            cachedTargetColliderRoot = null;
            targetBodyColliders = new Collider[0];
            return;
        }

        if (cachedTargetColliderRoot == target && targetBodyColliders.Length > 0)
        {
            return;
        }

        cachedTargetColliderRoot = target;
        Collider[] colliders = target.GetComponentsInChildren<Collider>(true);
        List<Collider> usableColliders = new List<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider targetCollider = colliders[i];
            if (IsUsableTargetBodyCollider(targetCollider))
            {
                usableColliders.Add(targetCollider);
            }
        }

        targetBodyColliders = usableColliders.ToArray();
    }

    private bool IsUsableTargetBodyCollider(Collider targetCollider)
    {
        return targetCollider != null
            && targetCollider.enabled
            && !targetCollider.isTrigger
            && !targetCollider.transform.IsChildOf(transform)
            && targetCollider.GetComponentInParent<PlayerWeaponHitbox>() == null;
    }

    private float GetAxisDelta(Vector3 worldPosition)
    {
        return Vector3.Dot(worldPosition - transform.position, movementAxis);
    }

    private float GetAxisDeltaFromHome(Vector3 worldPosition)
    {
        return Vector3.Dot(worldPosition - homePosition, movementAxis);
    }

    private void SnapToMovementPlane()
    {
        if (UsesFree3DMovement || !lockDepthToMovementPlane)
        {
            return;
        }

        Vector3 position = body.position;
        float depth = Vector3.Dot(position, depthAxis);
        body.position = position + depthAxis * (lockedDepth - depth);
    }

    private void EnsureProjectileSpawn()
    {
        Transform preferredSpawn = FindChildByName(transform, "Shoot");
        if (preferredSpawn != null)
        {
            projectileSpawn = preferredSpawn;
            return;
        }

        if (projectileSpawn != null && projectileSpawn.name == "Shoot")
        {
            return;
        }

        projectileSpawn = FindChildByName(transform, "ProjectileSpawn");
        bool createdSpawn = false;
        if (projectileSpawn == null)
        {
            GameObject spawnObject = new GameObject("Shoot");
            spawnObject.transform.SetParent(transform, false);
            projectileSpawn = spawnObject.transform;
            createdSpawn = true;
        }

        if (createdSpawn)
        {
            projectileSpawn.localPosition = projectileLocalOffset;
            projectileSpawn.localRotation = Quaternion.identity;
            projectileSpawn.localScale = Vector3.one;
        }
    }

    private void EnsureProjectileVisualTemplate()
    {
        projectileVisualTemplate = null;
        projectileVisualTemplates = null;
        if (projectileSpawn == null)
        {
            return;
        }

        projectileVisualTemplates = FindTemplateChildren(projectileSpawn);
        projectileVisualTemplate = projectileVisualTemplates.Length > 0 ? projectileVisualTemplates[0] : null;
        if (projectileVisualTemplate != null)
        {
            for (int i = 0; i < projectileVisualTemplates.Length; i++)
            {
                if (projectileVisualTemplates[i] != null)
                {
                    projectileVisualTemplates[i].gameObject.SetActive(false);
                }
            }
        }
    }

    private static Transform[] FindTemplateChildren(Transform root)
    {
        Transform[] children = new Transform[root.childCount];
        int index = 0;
        foreach (Transform child in root)
        {
            children[index] = child;
            index++;
        }

        return children;
    }

    private static Transform FindChildByName(Transform root, string targetName)
    {
        foreach (Transform child in root)
        {
            if (child.name == targetName)
            {
                return child;
            }

            Transform match = FindChildByName(child, targetName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private void Move(float moveDirection, float speed)
    {
        Move(movementAxis * moveDirection, speed);
    }

    private void Move(Vector3 moveDirection, float speed)
    {
        moveDirection = FlattenHorizontalOrZero(moveDirection);
        if (moveDirection.sqrMagnitude < 0.0001f)
        {
            StopMoving();
            return;
        }

        direction = Vector3.Dot(moveDirection, movementAxis) >= 0f ? 1 : -1;
        Face(moveDirection);

        Vector3 velocity = body.linearVelocity;
        velocity = UsesFree3DMovement ? RemoveHorizontalVelocity(velocity) : RemoveAxisVelocity(velocity, movementAxis);
        velocity += moveDirection * speed;
        body.linearVelocity = velocity;
        SnapToMovementPlane();
    }

    private void StopMoving()
    {
        Vector3 velocity = body.linearVelocity;
        velocity = UsesFree3DMovement ? RemoveHorizontalVelocity(velocity) : RemoveAxisVelocity(velocity, movementAxis);
        body.linearVelocity = velocity;
        SnapToMovementPlane();
    }

    private void TurnAround()
    {
        direction *= -1;
        Face(direction);
    }

    private void Face(float faceDirection)
    {
        if (Mathf.Abs(faceDirection) < 0.01f)
        {
            return;
        }

        Face(movementAxis * faceDirection);
    }

    private void Face(Vector3 faceDirection)
    {
        faceDirection = FlattenHorizontalOrZero(faceDirection);
        if (faceDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        direction = Vector3.Dot(faceDirection, movementAxis) >= 0f ? 1 : -1;
        if (UsesFree3DMovement)
        {
            transform.rotation = Quaternion.FromToRotation(Vector3.right, faceDirection) * spawnRotation;
            return;
        }

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * direction;
        transform.localScale = scale;
    }

    private bool FaceTarget()
    {
        if (target == null)
        {
            return false;
        }

        Vector3 targetDirection = GetMoveDirectionToTarget();
        if (targetDirection.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        Face(targetDirection);
        return true;
    }

    private void FindTarget()
    {
        PlayerMotor3D player = Object.FindFirstObjectByType<PlayerMotor3D>();
        if (player != null)
        {
            target = player.transform;
        }
    }

    private void ApplyPlayerCollisionIgnore()
    {
        if (playerCollisionIgnoreApplied)
        {
            return;
        }

        EnsureBodyCollider();
        if (bodyCollider == null)
        {
            return;
        }

        PlayerMotor3D player = target != null ? target.GetComponentInParent<PlayerMotor3D>() : Object.FindFirstObjectByType<PlayerMotor3D>();
        if (player == null)
        {
            return;
        }

        Collider[] enemyColliders = GetComponentsInChildren<Collider>(true);
        Collider[] playerColliders = player.GetComponentsInChildren<Collider>(true);
        bool ignoredAnyCollision = false;

        for (int i = 0; i < enemyColliders.Length; i++)
        {
            Collider enemyCollider = enemyColliders[i];
            if (enemyCollider == null || enemyCollider.isTrigger || !enemyCollider.enabled)
            {
                continue;
            }

            for (int j = 0; j < playerColliders.Length; j++)
            {
                Collider playerCollider = playerColliders[j];
                if (playerCollider == null || playerCollider.isTrigger || !playerCollider.enabled)
                {
                    continue;
                }

                Physics.IgnoreCollision(enemyCollider, playerCollider, true);
                ignoredAnyCollision = true;
            }
        }

        playerCollisionIgnoreApplied = ignoredAnyCollision;
    }

    private void Die()
    {
        if (deathSequenceStarted)
        {
            return;
        }

        deathSequenceStarted = true;
        attackResolved = true;
        visualAnimator?.SetCombatMode(false);

        if (grounder != null)
        {
            grounder.enabled = false;
        }

        DisableDeathColliders();

        if (launchAwayOnDeath && body != null)
        {
            body.isKinematic = false;
            body.useGravity = true;
            body.constraints = RigidbodyConstraints.None;
            body.linearVelocity = GetDeathLaunchDirection() * deathLaunchSpeed + Vector3.up * deathLaunchUpSpeed;
            body.angularVelocity = depthAxis * (deathSpinDegreesPerSecond * Mathf.Deg2Rad);
        }

        StartCoroutine(HandleDeathPerformanceAndRespawn());
    }

    private IEnumerator HandleDeathPerformanceAndRespawn()
    {
        yield return new WaitForSeconds(Mathf.Max(0.05f, deathDestroyDelay));

        HideEnemyAfterDeath();
        if (!respawnAfterCameraLeaves)
        {
#if UNITY_EDITOR
            gameObject.SetActive(false);
#else
            Destroy(gameObject);
#endif
            yield break;
        }

        yield return WaitUntilSpawnPointLeavesCamera();
        Respawn();
    }

    private IEnumerator WaitUntilSpawnPointLeavesCamera()
    {
        float awayTimer = 0f;
        while (awayTimer < Mathf.Max(0.05f, respawnCameraAwaySeconds))
        {
            awayTimer = IsSpawnPointVisibleByCamera() ? 0f : awayTimer + Time.deltaTime;
            yield return null;
        }
    }

    private bool IsSpawnPointVisibleByCamera()
    {
        Camera targetCamera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        if (targetCamera == null)
        {
            return false;
        }

        Vector3 viewportPosition = targetCamera.WorldToViewportPoint(homePosition);
        float padding = Mathf.Max(0f, respawnViewportPadding);
        return viewportPosition.z > 0f
            && viewportPosition.x >= -padding
            && viewportPosition.x <= 1f + padding
            && viewportPosition.y >= -padding
            && viewportPosition.y <= 1f + padding;
    }

    private Vector3 GetDeathLaunchDirection()
    {
        if (target != null)
        {
            Vector3 awayFromTarget = UsesFree3DMovement
                ? FlattenHorizontalOrZero(transform.position - target.position)
                : -GetMoveDirectionTo(target.position);
            if (awayFromTarget.sqrMagnitude > 0.0001f)
            {
                return awayFromTarget;
            }
        }

        return -GetFacingDirection();
    }

    private void DisableDeathColliders()
    {
        deathDisabledColliders = GetComponentsInChildren<Collider>(true);
        deathDisabledColliderStates = new bool[deathDisabledColliders.Length];
        for (int i = 0; i < deathDisabledColliders.Length; i++)
        {
            Collider deathCollider = deathDisabledColliders[i];
            if (deathCollider != null)
            {
                deathDisabledColliderStates[i] = deathCollider.enabled;
                deathCollider.enabled = false;
            }
        }
    }

    private void HideEnemyAfterDeath()
    {
        deathHiddenRenderers = GetComponentsInChildren<Renderer>(true);
        deathHiddenRendererStates = new bool[deathHiddenRenderers.Length];
        for (int i = 0; i < deathHiddenRenderers.Length; i++)
        {
            Renderer renderer = deathHiddenRenderers[i];
            if (renderer != null)
            {
                deathHiddenRendererStates[i] = renderer.enabled;
                renderer.enabled = false;
            }
        }

        if (body != null)
        {
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            body.useGravity = false;
        }
    }

    private void Respawn()
    {
        transform.position = homePosition;
        transform.rotation = spawnRotation;
        transform.localScale = spawnScale;
        direction = spawnDirection;
        state = EnemyState.Patrol;
        selectedBossProjectileIndex = -1;
        ResetRangedAttackRhythm();
        targetLostUntil = -1f;
        attackResolved = true;
        nextAttackTime = Time.time + 0.25f;
        attackResolveTime = 0f;
        attackEndTime = 0f;
        deathSequenceStarted = false;
        damageKnockbackLockedUntil = 0f;
        damageLandingRecoveryUntil = 0f;
        damageGroundContactUntil = 0f;
        waitingForDamageLanding = false;
        RestoreGrounderAfterDamage();
        visualAnimator?.ResumeHit();

        if (body != null)
        {
            body.isKinematic = aliveIsKinematic;
            body.useGravity = aliveUseGravity;
            body.constraints = aliveConstraints;
            body.position = homePosition;
            body.rotation = spawnRotation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        RestoreDeathRenderers();
        RestoreDeathColliders();

        if (grounder != null)
        {
            grounder.enabled = true;
        }

        NormalizeMovementPlane();
        health?.ReviveFull();
        FindTarget();
        visualAnimator?.SetCombatMode(false);
    }

    private void RestoreDeathRenderers()
    {
        if (deathHiddenRenderers == null || deathHiddenRendererStates == null)
        {
            return;
        }

        for (int i = 0; i < deathHiddenRenderers.Length && i < deathHiddenRendererStates.Length; i++)
        {
            if (deathHiddenRenderers[i] != null)
            {
                deathHiddenRenderers[i].enabled = deathHiddenRendererStates[i];
            }
        }
    }

    private void RestoreDeathColliders()
    {
        if (deathDisabledColliders == null || deathDisabledColliderStates == null)
        {
            return;
        }

        for (int i = 0; i < deathDisabledColliders.Length && i < deathDisabledColliderStates.Length; i++)
        {
            if (deathDisabledColliders[i] != null)
            {
                deathDisabledColliders[i].enabled = deathDisabledColliderStates[i];
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!onlyShowDetectionBoxWhenSelected)
        {
            DrawDetectionBoxGizmo();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (onlyShowDetectionBoxWhenSelected)
        {
            DrawDetectionBoxGizmo();
        }

        DrawAttackGizmo();
    }

    private void DrawDetectionBoxGizmo()
    {
        if (!showDetectionBoxGizmo)
        {
            return;
        }

        Vector3 gizmoMovementAxis = useTransformRightAsMovementAxis ? transform.right : movementAxis;
        gizmoMovementAxis = FlattenHorizontal(gizmoMovementAxis);
        Vector3 gizmoDepthAxis = FlattenHorizontal(Vector3.Cross(gizmoMovementAxis, Vector3.up));
        Vector3 detectionCenter = transform.position
            + gizmoMovementAxis * detectionBoxOffset.x
            + Vector3.up * detectionBoxOffset.y
            + gizmoDepthAxis * detectionBoxOffset.z;

        if (movementMode == MovementMode.Free3D)
        {
            Vector3 gizmoHomePosition = Application.isPlaying ? homePosition : transform.position;
            detectionCenter = GetDetectionBoxCenter();
            Gizmos.color = new Color(1f, 0.85f, 0f, 0.12f);
            Gizmos.DrawSphere(detectionCenter, searchRange);
            Gizmos.color = new Color(1f, 0.85f, 0f, 0.75f);
            Gizmos.DrawWireSphere(detectionCenter, searchRange);
            Gizmos.color = new Color(0.2f, 1f, 0.6f, 0.75f);
            Gizmos.DrawWireSphere(gizmoHomePosition, Mathf.Max(0.25f, patrolRadius));
            return;
        }

        Vector3 detectionSize = new Vector3(searchRange * 2f, detectionBoxHeight, detectionBoxDepth);
        Gizmos.matrix = Matrix4x4.TRS(detectionCenter, Quaternion.LookRotation(gizmoDepthAxis, Vector3.up), Vector3.one);
        Gizmos.color = new Color(1f, 0.85f, 0f, 0.08f);
        Gizmos.DrawCube(Vector3.zero, detectionSize);
        Gizmos.color = new Color(1f, 0.85f, 0f, 0.75f);
        Gizmos.DrawWireCube(Vector3.zero, detectionSize);
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void DrawAttackGizmo()
    {
        Gizmos.color = attackMode == AttackMode.Ranged
            ? new Color(0.2f, 0.7f, 1f, 0.5f)
            : new Color(1f, 0.1f, 0.05f, 0.45f);

        if (attackMode == AttackMode.Ranged)
        {
            if (movementMode == MovementMode.Free3D)
            {
                Gizmos.DrawWireSphere(transform.position, searchRange);
            }
            else
            {
                Vector3 gizmoHomePosition = Application.isPlaying ? homePosition : transform.position;
                Vector3 left = gizmoHomePosition + movementAxis * leftPatrolDistance;
                Vector3 right = gizmoHomePosition + movementAxis * rightPatrolDistance;
                Gizmos.DrawLine(left, right);
            }
        }
        else
        {
            if (movementMode == MovementMode.Free3D)
            {
                Gizmos.DrawWireSphere(transform.position, attackRange);
                return;
            }

            Gizmos.matrix = Matrix4x4.TRS(transform.position + movementAxis * direction * attackRange * 0.5f, Quaternion.LookRotation(depthAxis, Vector3.up), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(attackRange, meleeAttackHeight * 2f, 1f));
            Gizmos.matrix = Matrix4x4.identity;
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
        if (value.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        return value.normalized;
    }

    private static Vector3 RemoveAxisVelocity(Vector3 velocity, Vector3 axis)
    {
        return velocity - axis * Vector3.Dot(velocity, axis);
    }

    private static Vector3 RemoveHorizontalVelocity(Vector3 velocity)
    {
        velocity.x = 0f;
        velocity.z = 0f;
        return velocity;
    }
}
