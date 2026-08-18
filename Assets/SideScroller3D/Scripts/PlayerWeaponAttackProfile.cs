using System;
using UnityEngine;

[Serializable]
public class PlayerWeaponAttackStep
{
    [Tooltip("Animator state to play for this attack step, for example Attack_01.")]
    [SerializeField] private string animatorStateName;

    [Tooltip("Animation clip for this attack step. If this is empty, this attack step will not play.")]
    [SerializeField] private AnimationClip animationClip;

    [SerializeField, HideInInspector] private string animationClipName;

    [Tooltip("Animator trigger used to enter this step from the previous attack. Usually empty for the first step.")]
    [SerializeField] private string triggerName;

    [Tooltip("Damage dealt by this attack step.")]
    [SerializeField] private int damage = 1;

    [Tooltip("Seconds after this step starts where the next combo input is accepted. Use 0 for the final step.")]
    [SerializeField] private float nextInputWindowSeconds = 0.1f;

    [Tooltip("Animation frame that this step must reach before a queued combo can enter the next step.")]
    [SerializeField] private int nextAttackStartFrame = 15;

    [Tooltip("Effect root played when this attack step starts. Leave empty to play no effect.")]
    [SerializeField] private Transform attackEffectRoot;

    [Tooltip("Camera shake distance when this attack step hits a target.")]
    [SerializeField] private float cameraShakeAmplitude = 0.08f;

    [Tooltip("Camera shake duration when this attack step hits a target.")]
    [SerializeField] private float cameraShakeDuration = 0.08f;

    [Tooltip("Camera shake frequency when this attack step hits a target.")]
    [SerializeField] private float cameraShakeFrequency = 35f;

    public PlayerWeaponAttackStep(string animatorStateName, string animationClipName, string triggerName, int damage, float nextInputWindowSeconds, int nextAttackStartFrame = 15, Transform attackEffectRoot = null)
    {
        this.animatorStateName = animatorStateName;
        this.animationClipName = animationClipName;
        this.triggerName = triggerName;
        this.damage = damage;
        this.nextInputWindowSeconds = nextInputWindowSeconds;
        this.nextAttackStartFrame = nextAttackStartFrame;
        this.attackEffectRoot = attackEffectRoot;
    }

    public string AnimatorStateName => animatorStateName;
    public AnimationClip AnimationClip => animationClip;
    public string AnimationClipName => animationClip != null ? animationClip.name : string.Empty;
    public string TriggerName => triggerName;
    public int Damage => Mathf.Max(0, damage);
    public float NextInputWindowSeconds => Mathf.Max(0f, nextInputWindowSeconds);
    public int NextAttackStartFrame => Mathf.Max(0, nextAttackStartFrame);
    public Transform AttackEffectRoot => attackEffectRoot;
    public float CameraShakeAmplitude => Mathf.Max(0f, cameraShakeAmplitude);
    public float CameraShakeDuration => Mathf.Max(0f, cameraShakeDuration);
    public float CameraShakeFrequency => Mathf.Max(0f, cameraShakeFrequency);

    public PlayerWeaponAttackStep CloneWithFallback(PlayerWeaponAttackStep fallback)
    {
        PlayerWeaponAttackStep clone = (PlayerWeaponAttackStep)MemberwiseClone();
        if (clone.attackEffectRoot == null && fallback != null)
        {
            clone.attackEffectRoot = fallback.attackEffectRoot;
        }

        return clone;
    }
}

public class PlayerWeaponAttackProfile : MonoBehaviour
{
    private const string DefaultHitSoundPath = "Assets/Art/Sound/Hit.wav";

    [Header("Definition")]
    [Tooltip("Optional reusable weapon data asset. This profile can load attack tuning from it.")]
    [SerializeField] private WeaponDefinition3D weaponDefinition;

    [Tooltip("Apply Weapon Definition values when the weapon starts playing.")]
    [SerializeField] private bool applyDefinitionOnAwake = true;

    [Tooltip("Apply Weapon Definition values in edit mode during validation. Keep disabled when locally tweaking a prefab override.")]
    [SerializeField] private bool applyDefinitionInEditor;

    [Header("Attack Behavior")]
    [Tooltip("Minimum time between attack starts.")]
    [SerializeField] private float attackCooldown = 0.45f;

    [Tooltip("Seconds that movement is locked while attacking. If animation length is used, the larger value wins.")]
    [SerializeField] private float attackMoveLockSeconds = 0.35f;

    [Tooltip("When enabled, movement lock lasts at least as long as the assigned attack animation clip.")]
    [SerializeField] private bool useAttackAnimationLength = true;

    [Tooltip("Attack animation speed multiplier. 1 is normal speed, 2 is twice as fast, 0.5 is half speed.")]
    [SerializeField] private float attackSpeedMultiplier = 1.5f;

    [Tooltip("Cross fade duration when switching attack animation states.")]
    [SerializeField] private float attackCrossFadeSeconds = 0.03f;

    [Tooltip("When disabled, attacks cannot start while the player is airborne.")]
    [SerializeField] private bool allowAirAttacks;

    [Header("Targets")]
    [Tooltip("Layers that this weapon can hit. Usually Enemy.")]
    [SerializeField] private LayerMask targetMask = 512;

    [Header("Combo Attacks")]
    [Tooltip("Each entry is one attack step. State names must exist in the player Animator.")]
    [SerializeField]
    private PlayerWeaponAttackStep[] attacks =
    {
        new PlayerWeaponAttackStep("Attack_01", "", "", 1, 0.1f),
        new PlayerWeaponAttackStep("Attack_02", "", "Attack2", 1, 0.1f),
        new PlayerWeaponAttackStep("Attack_03", "", "Attack3", 1, 0f),
    };

    [Header("Hit Audio")]
    [Tooltip("Default sound played when this weapon hits an enemy or reflects a projectile.")]
    [SerializeField] private AudioClip attackHitSound;

    [Tooltip("Default hit sound volume.")]
    [SerializeField, Range(0f, 1f)] private float attackHitSoundVolume = 1f;

    [Tooltip("Optional hit sound rules for different target types. The first matching rule is used.")]
    [SerializeField] private PlayerHitSoundRule[] targetHitSounds;

    public float AttackCooldown => Mathf.Max(0f, attackCooldown);
    public float AttackMoveLockSeconds => Mathf.Max(0f, attackMoveLockSeconds);
    public bool UseAttackAnimationLength => useAttackAnimationLength;
    public float AttackSpeedMultiplier => Mathf.Max(0.1f, attackSpeedMultiplier);
    public float AttackCrossFadeSeconds => Mathf.Max(0f, attackCrossFadeSeconds);
    public bool AllowAirAttacks => allowAirAttacks;
    public LayerMask TargetMask => targetMask;
    public int AttackCount => attacks != null ? attacks.Length : 0;
    public AudioClip AttackHitSound => attackHitSound;
    public float AttackHitSoundVolume => Mathf.Clamp01(attackHitSoundVolume);
    public PlayerHitSoundRule[] TargetHitSounds => targetHitSounds;
    public WeaponDefinition3D Definition => weaponDefinition;

    private void Reset()
    {
        AssignDefaultHitSoundIfNeeded();
    }

    private void Awake()
    {
        if (applyDefinitionOnAwake)
        {
            ApplyDefinition();
        }
    }

    private void OnValidate()
    {
        if (applyDefinitionInEditor)
        {
            ApplyDefinition();
        }

        AssignDefaultHitSoundIfNeeded();
    }

    public void ApplyDefinition()
    {
        ApplyDefinition(weaponDefinition);
    }

    public void ApplyDefinition(WeaponDefinition3D definition)
    {
        if (definition == null || !definition.applyAttackProfile)
        {
            return;
        }

        attackCooldown = definition.attackCooldown;
        attackMoveLockSeconds = definition.attackMoveLockSeconds;
        useAttackAnimationLength = definition.useAttackAnimationLength;
        attackSpeedMultiplier = definition.attackSpeedMultiplier;
        attackCrossFadeSeconds = definition.attackCrossFadeSeconds;
        allowAirAttacks = definition.allowAirAttacks;
        targetMask = definition.targetMask;
        attacks = CloneAttackSteps(definition.attacks, attacks);
        attackHitSound = definition.attackHitSound;
        attackHitSoundVolume = definition.attackHitSoundVolume;
        targetHitSounds = CloneHitSoundRules(definition.targetHitSounds);
        AssignDefaultHitSoundIfNeeded();
    }

    public void SaveToDefinition()
    {
        SaveToDefinition(weaponDefinition);
    }

    public void SaveToDefinition(WeaponDefinition3D definition)
    {
        if (definition == null)
        {
            return;
        }

        definition.applyAttackProfile = true;
        definition.attackCooldown = attackCooldown;
        definition.attackMoveLockSeconds = attackMoveLockSeconds;
        definition.useAttackAnimationLength = useAttackAnimationLength;
        definition.attackSpeedMultiplier = attackSpeedMultiplier;
        definition.attackCrossFadeSeconds = attackCrossFadeSeconds;
        definition.allowAirAttacks = allowAirAttacks;
        definition.targetMask = targetMask;
        definition.attacks = CloneAttackSteps(attacks, definition.attacks);
        definition.attackHitSound = attackHitSound;
        definition.attackHitSoundVolume = attackHitSoundVolume;
        definition.targetHitSounds = CloneHitSoundRules(targetHitSounds);
    }

    public bool TryGetAttack(int index, out PlayerWeaponAttackStep attack)
    {
        if (attacks != null && index >= 0 && index < attacks.Length && attacks[index] != null)
        {
            attack = attacks[index];
            return true;
        }

        attack = null;
        return false;
    }

    private void AssignDefaultHitSoundIfNeeded()
    {
#if UNITY_EDITOR
        if (attackHitSound == null)
        {
            attackHitSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(DefaultHitSoundPath);
        }
#endif
    }

    private static PlayerWeaponAttackStep[] CloneAttackSteps(PlayerWeaponAttackStep[] source, PlayerWeaponAttackStep[] fallback)
    {
        if (source == null)
        {
            return null;
        }

        PlayerWeaponAttackStep[] clones = new PlayerWeaponAttackStep[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            PlayerWeaponAttackStep fallbackStep = fallback != null && i < fallback.Length ? fallback[i] : null;
            clones[i] = source[i] != null ? source[i].CloneWithFallback(fallbackStep) : null;
        }

        return clones;
    }

    private static PlayerHitSoundRule[] CloneHitSoundRules(PlayerHitSoundRule[] source)
    {
        if (source == null)
        {
            return null;
        }

        PlayerHitSoundRule[] clone = new PlayerHitSoundRule[source.Length];
        source.CopyTo(clone, 0);
        return clone;
    }
}
