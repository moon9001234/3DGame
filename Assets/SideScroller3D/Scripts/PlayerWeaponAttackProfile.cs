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
}

public class PlayerWeaponAttackProfile : MonoBehaviour
{
    private const string DefaultHitSoundPath = "Assets/Art/Sound/Hit.wav";

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

    private void Reset()
    {
        AssignDefaultHitSoundIfNeeded();
    }

    private void OnValidate()
    {
        AssignDefaultHitSoundIfNeeded();
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
}
