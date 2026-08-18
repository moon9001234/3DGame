using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDefinition3D", menuName = "Side Scroller 3D/Weapon Definition")]
public class WeaponDefinition3D : ScriptableObject
{
    [Header("Attack Profile")]
    public bool applyAttackProfile = true;
    public float attackCooldown = 0.45f;
    public float attackMoveLockSeconds = 0.35f;
    public bool useAttackAnimationLength = true;
    public float attackSpeedMultiplier = 1.5f;
    public float attackCrossFadeSeconds = 0.03f;
    public bool allowAirAttacks;
    public LayerMask targetMask = 512;
    public PlayerWeaponAttackStep[] attacks =
    {
        new PlayerWeaponAttackStep("Attack_01", "", "", 1, 0.1f),
        new PlayerWeaponAttackStep("Attack_02", "", "Attack2", 1, 0.1f),
        new PlayerWeaponAttackStep("Attack_03", "", "Attack3", 1, 0f),
    };
    public AudioClip attackHitSound;
    [Range(0f, 1f)] public float attackHitSoundVolume = 1f;
    public PlayerHitSoundRule[] targetHitSounds;

    [Header("Hitbox")]
    public bool applyHitbox = true;
    public Vector3 weaponSize = new Vector3(1.35f, 0.16f, 0.16f);
    public Color weaponColor = new Color(0.85f, 0.82f, 0.72f, 1f);
    public bool useModelBoundsForHitbox;
    public bool updateColliderDuringPlay;
    public Vector3 modelBoundsPadding = new Vector3(0.04f, 0.04f, 0.04f);
    public Vector3 projectileReflectExtraRange = new Vector3(0.45f, 0.3f, 0.3f);
    public GameObject projectileReflectEffectPrefab;
    public float projectileReflectEffectScale = 1f;
    public float projectileReflectEffectFallbackLifetime = 2f;
}
