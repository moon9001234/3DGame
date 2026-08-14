using UnityEngine;
using CartoonFX;
using System.Collections.Generic;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class PlayerCombat3D : MonoBehaviour
{
    private const float FallbackAttackCooldown = 0.45f;
    private const float FallbackAttackMoveLockSeconds = 0.35f;
    private const float FallbackAttackSpeedMultiplier = 1f;
    private const float FallbackAttackCrossFadeSeconds = 0.03f;
    private const int FallbackDamage = 1;
    private static readonly int InCombatHash = Animator.StringToHash("InCombat");

    private PlayerWeaponHitbox weaponHitbox;

    [Header("Combat State")]
    [Tooltip("When an enemy enters this radius, the player stays in combat idle.")]
    [SerializeField] private float combatDetectionRange = 5f;

    [Tooltip("Allowed vertical distance for combat detection.")]
    [SerializeField] private float combatVerticalRange = 2.5f;

    [Tooltip("Seconds to keep combat idle after attacking or after enemies leave detection.")]
    [SerializeField] private float combatMemorySeconds = 2f;

    private PlayerMotor3D motor;
    private Rigidbody body;
    private Animator animator;
    private PlayerWeaponAttackProfile runtimeWeaponAttackProfile;
    private float nextAttackTime;
    private float attackLockedUntil;
    private float attackSpeedResetTime;
    private float baseAnimatorSpeed = 1f;
    private float combatModeUntil;
    private float comboInputWindowUntil = -1f;
    private float currentAttackStartedAt = -1f;
    private int currentAttackIndex = -1;
    private int nextComboAttackIndex = -1;
    private int queuedComboAttackIndex = -1;
    private int queuedComboFromAttackIndex = -1;
    private bool capturedAnimatorSpeed;
    private bool warnedMissingWeaponHitbox;
    private readonly Collider[] combatScanResults = new Collider[16];
    private readonly Dictionary<Transform, Transform> attackEffectInstances = new Dictionary<Transform, Transform>();
    private readonly Dictionary<Transform, Transform> attackEffectWrappers = new Dictionary<Transform, Transform>();
    private readonly Dictionary<Transform, Vector3> attackEffectBaseLocalScales = new Dictionary<Transform, Vector3>();
    private Transform cachedAttackEffectRoot;
    private ParticleSystem[] attackEffectParticles = new ParticleSystem[0];

    public bool IsAttackLocked => Time.time < attackLockedUntil;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor3D>();
        body = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        CaptureBaseAnimatorSpeed();
        if (!EnsureWeaponHitbox())
        {
            return;
        }
        CacheAttackEffect();
        StopAttackStepEffectsOnAwake();
    }

    private void OnDisable()
    {
        RestoreAnimatorSpeed();
    }

    private void Update()
    {
        RestoreAttackAnimationSpeedIfNeeded();
        UpdateCombatMode();

        if (WasAttackPressed())
        {
            if (CanQueueComboInput())
            {
                QueueComboInput();
            }
            else if (Time.time >= nextAttackTime)
            {
                Attack(0);
            }
        }

        TryPlayQueuedCombo();
    }

    private void Attack(int attackIndex)
    {
        PlayerWeaponAttackStep attackStep = ResolveAttackStep(attackIndex);
        if (attackStep == null)
        {
            return;
        }

        bool isComboAttack = attackIndex > 0;
        if (!isComboAttack && IsAirAttackBlocked())
        {
            return;
        }

        if (!EnsureWeaponHitbox())
        {
            return;
        }

        float attackSpeed = GetAttackSpeed();
        AnimationClip attackClip = ResolveAttackClip(attackStep);
        if (attackClip == null)
        {
            Debug.LogWarning($"Player attack step {attackIndex + 1} has no Animation Clip assigned.", this);
            return;
        }

        float attackLockSeconds = GetAttackMoveLockSeconds(attackSpeed, attackClip);
        nextAttackTime = Time.time + Mathf.Max(GetWeaponAttackCooldown() / attackSpeed, attackLockSeconds);
        attackLockedUntil = Time.time + attackLockSeconds;
        combatModeUntil = Mathf.Max(combatModeUntil, Time.time + attackLockSeconds + combatMemorySeconds);
        currentAttackIndex = attackIndex;
        currentAttackStartedAt = Time.time;
        ClearQueuedComboInput();
        OpenNextComboWindow(attackIndex, attackStep);

        if (animator != null)
        {
            SetAttackAnimationSpeed(attackSpeed, attackLockSeconds);
            PlayAttackState(attackStep, attackIndex);
        }

        PlayAttackEffect(attackStep);

        weaponHitbox.Configure(attackStep.Damage, GetWeaponTargetMask());
        weaponHitbox.ConfigureHitSounds(GetWeaponAttackHitSound(), GetWeaponAttackHitSoundVolume(), GetWeaponTargetHitSounds());
        weaponHitbox.ConfigureCameraShake(attackStep.CameraShakeAmplitude, attackStep.CameraShakeDuration, attackStep.CameraShakeFrequency);
        weaponHitbox.BeginSwing(attackLockSeconds);
    }

    private void PlayAttackState(PlayerWeaponAttackStep attackStep, int attackIndex)
    {
        string stateName = attackStep != null ? attackStep.AnimatorStateName : null;
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            return;
        }

        int fullPathHash = Animator.StringToHash($"Base Layer.{stateName}");
        if (!animator.HasState(0, fullPathHash))
        {
            Debug.LogWarning($"Player Animator state 'Base Layer.{stateName}' not found.", this);
            return;
        }

        if (attackIndex == 0)
        {
            ResetComboTriggers();
        }

        animator.CrossFadeInFixedTime(fullPathHash, GetWeaponAttackCrossFadeSeconds(), 0, 0f);
        animator.Update(0f);
    }

    private bool CanQueueComboInput()
    {
        if (queuedComboAttackIndex >= 0
            || nextComboAttackIndex <= 0
            || Time.time > comboInputWindowUntil
            || ResolveAttackStep(nextComboAttackIndex) == null)
        {
            return false;
        }

        return ResolveAttackStep(currentAttackIndex) != null;
    }

    private void QueueComboInput()
    {
        queuedComboAttackIndex = nextComboAttackIndex;
        queuedComboFromAttackIndex = currentAttackIndex;
    }

    private void ClearQueuedComboInput()
    {
        queuedComboAttackIndex = -1;
        queuedComboFromAttackIndex = -1;
    }

    private void TryPlayQueuedCombo()
    {
        if (queuedComboAttackIndex < 0)
        {
            return;
        }

        if (currentAttackIndex != queuedComboFromAttackIndex)
        {
            ClearQueuedComboInput();
            return;
        }

        PlayerWeaponAttackStep currentAttack = ResolveAttackStep(currentAttackIndex);
        if (currentAttack == null || !HasReachedComboStartTime(currentAttack))
        {
            return;
        }

        int attackIndex = queuedComboAttackIndex;
        ClearQueuedComboInput();
        Attack(attackIndex);
    }

    private bool HasReachedComboStartTime(PlayerWeaponAttackStep attackStep)
    {
        if (attackStep == null || currentAttackStartedAt < 0f)
        {
            return true;
        }

        float attackSpeed = Mathf.Max(0.1f, GetAttackSpeed());
        float requiredSeconds = GetComboStartDelaySeconds(attackStep, attackSpeed);
        return Time.time - currentAttackStartedAt >= requiredSeconds;
    }

    private static float GetComboStartDelaySeconds(PlayerWeaponAttackStep attackStep, float attackSpeed)
    {
        if (attackStep == null)
        {
            return 0f;
        }

        float frameRate = attackStep.AnimationClip != null
            ? Mathf.Max(1f, attackStep.AnimationClip.frameRate)
            : 60f;
        float clipSeconds = attackStep.AnimationClip != null
            ? Mathf.Max(0f, attackStep.AnimationClip.length)
            : 0f;
        float requestedSeconds = attackStep.NextAttackStartFrame / frameRate;
        return (clipSeconds > 0f
            ? Mathf.Min(requestedSeconds, clipSeconds)
            : requestedSeconds) / attackSpeed;
    }

    private void OpenNextComboWindow(int attackIndex, PlayerWeaponAttackStep attackStep)
    {
        int nextAttackIndex = attackIndex + 1;
        if (nextAttackIndex < GetAttackCount() && attackStep.NextInputWindowSeconds > 0f)
        {
            nextComboAttackIndex = nextAttackIndex;
            comboInputWindowUntil = Time.time + attackStep.NextInputWindowSeconds;
            return;
        }

        nextComboAttackIndex = -1;
        comboInputWindowUntil = -1f;
    }

    private int GetAttackCount()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        return profile != null ? profile.AttackCount : 0;
    }

    private PlayerWeaponAttackStep ResolveAttackStep(int attackIndex)
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        if (profile != null && profile.TryGetAttack(attackIndex, out PlayerWeaponAttackStep profileAttack))
        {
            return profileAttack;
        }

        return null;
    }

    private PlayerWeaponAttackProfile ResolveWeaponAttackProfile()
    {
        runtimeWeaponAttackProfile = null;

        if (weaponHitbox == null)
        {
            weaponHitbox = GetComponentInChildren<PlayerWeaponHitbox>(true);
        }

        if (weaponHitbox != null)
        {
            runtimeWeaponAttackProfile = weaponHitbox.GetComponent<PlayerWeaponAttackProfile>();
            if (runtimeWeaponAttackProfile == null)
            {
                runtimeWeaponAttackProfile = weaponHitbox.GetComponentInParent<PlayerWeaponAttackProfile>();
            }

            if (runtimeWeaponAttackProfile == null)
            {
                runtimeWeaponAttackProfile = weaponHitbox.GetComponentInChildren<PlayerWeaponAttackProfile>(true);
            }
        }

        return runtimeWeaponAttackProfile;
    }

    private void ResetComboTriggers()
    {
        int attackCount = GetAttackCount();
        for (int i = 1; i < attackCount; i++)
        {
            PlayerWeaponAttackStep attackStep = ResolveAttackStep(i);
            if (attackStep == null || string.IsNullOrWhiteSpace(attackStep.TriggerName))
            {
                continue;
            }

            ResetAnimatorTrigger(Animator.StringToHash(attackStep.TriggerName));
        }
    }

    private void ResetAnimatorTrigger(int triggerHash)
    {
        if (animator != null && HasAnimatorParameter(triggerHash))
        {
            animator.ResetTrigger(triggerHash);
        }
    }

    private bool IsAirAttackBlocked()
    {
        if (GetWeaponAllowAirAttacks())
        {
            return false;
        }

        motor = motor != null ? motor : GetComponent<PlayerMotor3D>();
        if (motor != null && !motor.IsGrounded)
        {
            return true;
        }

        body = body != null ? body : GetComponent<Rigidbody>();
        return body != null && body.linearVelocity.y > 0.1f;
    }

    private void UpdateCombatMode()
    {
        animator = animator != null ? animator : GetComponentInChildren<Animator>();

        if (HasEnemyNearby())
        {
            combatModeUntil = Time.time + combatMemorySeconds;
        }

        if (animator != null && HasAnimatorParameter(InCombatHash))
        {
            animator.SetBool(InCombatHash, Time.time < combatModeUntil || IsAttackLocked);
        }
    }

    private bool HasEnemyNearby()
    {
        LayerMask targetMask = GetWeaponTargetMask();
        if (targetMask.value == 0)
        {
            return false;
        }

        int hitCount = Physics.OverlapSphereNonAlloc(
            transform.position,
            combatDetectionRange,
            combatScanResults,
            targetMask,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            Collider enemy = combatScanResults[i];
            if (enemy == null)
            {
                continue;
            }

            Health health = enemy.GetComponentInParent<Health>();
            if (health == null || health.IsDead)
            {
                continue;
            }

            float verticalDistance = Mathf.Abs(health.transform.position.y - transform.position.y);
            if (verticalDistance <= combatVerticalRange)
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        PlayerWeaponHitbox hitbox = weaponHitbox != null ? weaponHitbox : GetComponentInChildren<PlayerWeaponHitbox>(true);
        if (hitbox != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix = hitbox.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }

    private bool WasAttackPressed()
    {
#if ENABLE_INPUT_SYSTEM
        bool keyboardPressed = Keyboard.current != null
            && (Keyboard.current.jKey.wasPressedThisFrame
                || Keyboard.current.leftCtrlKey.wasPressedThisFrame
                || Keyboard.current.rightCtrlKey.wasPressedThisFrame);
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool gamepadPressed = Gamepad.current != null
            && (Gamepad.current.buttonWest.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame);

        return keyboardPressed || mousePressed || gamepadPressed;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetButtonDown("Fire1");
#else
        return false;
#endif
    }

    private float GetAttackMoveLockSeconds(float attackSpeed, AnimationClip activeAttackClip)
    {
        float scaledMoveLockSeconds = GetWeaponAttackMoveLockSeconds() / attackSpeed;
        if (!GetWeaponUseAttackAnimationLength())
        {
            return scaledMoveLockSeconds;
        }

        if (activeAttackClip != null)
        {
            return Mathf.Max(scaledMoveLockSeconds, activeAttackClip.length / attackSpeed);
        }

        return scaledMoveLockSeconds;
    }

    private AnimationClip ResolveAttackClip(PlayerWeaponAttackStep attackStep)
    {
        if (attackStep == null)
        {
            return null;
        }

        return attackStep.AnimationClip;
    }

    private float GetAttackSpeed()
    {
        return GetWeaponAttackSpeedMultiplier();
    }

    private float GetWeaponAttackCooldown()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        return profile != null ? profile.AttackCooldown : FallbackAttackCooldown;
    }

    private float GetWeaponAttackMoveLockSeconds()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        return profile != null ? profile.AttackMoveLockSeconds : FallbackAttackMoveLockSeconds;
    }

    private bool GetWeaponUseAttackAnimationLength()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        return profile == null || profile.UseAttackAnimationLength;
    }

    private float GetWeaponAttackSpeedMultiplier()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        return profile != null ? profile.AttackSpeedMultiplier : FallbackAttackSpeedMultiplier;
    }

    private float GetWeaponAttackCrossFadeSeconds()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        return profile != null ? profile.AttackCrossFadeSeconds : FallbackAttackCrossFadeSeconds;
    }

    private bool GetWeaponAllowAirAttacks()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        return profile != null && profile.AllowAirAttacks;
    }

    private LayerMask GetWeaponTargetMask()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        if (profile != null)
        {
            return profile.TargetMask;
        }

        return LayerMask.GetMask("Enemy");
    }

    private AudioClip GetWeaponAttackHitSound()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        if (profile != null)
        {
            return profile.AttackHitSound;
        }

        return null;
    }

    private float GetWeaponAttackHitSoundVolume()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        return profile != null ? profile.AttackHitSoundVolume : 1f;
    }

    private PlayerHitSoundRule[] GetWeaponTargetHitSounds()
    {
        PlayerWeaponAttackProfile profile = ResolveWeaponAttackProfile();
        if (profile != null)
        {
            return profile.TargetHitSounds;
        }

        return null;
    }

    private void SetAttackAnimationSpeed(float attackSpeed, float attackLockSeconds)
    {
        animator = animator != null ? animator : GetComponentInChildren<Animator>();
        if (animator == null)
        {
            return;
        }

        CaptureBaseAnimatorSpeed();
        animator.speed = baseAnimatorSpeed * attackSpeed;
        attackSpeedResetTime = Time.time + attackLockSeconds;
    }

    private void RestoreAttackAnimationSpeedIfNeeded()
    {
        if (Time.time >= attackSpeedResetTime)
        {
            RestoreAnimatorSpeed();
        }
    }

    private void RestoreAnimatorSpeed()
    {
        if (animator != null && capturedAnimatorSpeed)
        {
            animator.speed = baseAnimatorSpeed;
        }
    }

    private void PlayAttackEffect(PlayerWeaponAttackStep attackStep)
    {
        Transform effectRoot = PrepareAttackEffectRoot(ResolveAttackEffectRoot(attackStep));
        if (effectRoot == null)
        {
            return;
        }

        if (effectRoot != cachedAttackEffectRoot || attackEffectParticles == null || attackEffectParticles.Length == 0)
        {
            CacheAttackEffectParticles(effectRoot);
        }

        if (attackEffectParticles == null || attackEffectParticles.Length == 0)
        {
            return;
        }

        if (!effectRoot.gameObject.activeSelf)
        {
            effectRoot.gameObject.SetActive(true);
        }

        for (int i = 0; i < attackEffectParticles.Length; i++)
        {
            ParticleSystem particleSystem = attackEffectParticles[i];
            if (particleSystem == null)
            {
                continue;
            }

            particleSystem.Clear(true);
            particleSystem.Play(true);
        }
    }

    private Transform PrepareAttackEffectRoot(Transform effectSource)
    {
        Transform anchor = ResolveAttackEffectAnchor();
        if (effectSource == null)
        {
            return null;
        }

        if (anchor == null)
        {
            return effectSource;
        }

        Transform effectRoot = effectSource;
        if (!IsSceneObject(effectSource))
        {
            if (!attackEffectInstances.TryGetValue(effectSource, out effectRoot) || effectRoot == null)
            {
                effectRoot = Instantiate(effectSource, anchor);
                effectRoot.name = effectSource.name;
                attackEffectInstances[effectSource] = effectRoot;
            }
        }

        ParentAttackEffectToAnchor(effectRoot, anchor);
        return effectRoot;
    }

    private static bool IsSceneObject(Transform candidate)
    {
        return candidate != null && candidate.gameObject.scene.IsValid();
    }

    private void ParentAttackEffectToAnchor(Transform effectRoot, Transform anchor)
    {
        if (effectRoot == null || anchor == null)
        {
            return;
        }

        if (!attackEffectBaseLocalScales.TryGetValue(effectRoot, out Vector3 baseLocalScale))
        {
            baseLocalScale = effectRoot.localScale;
            attackEffectBaseLocalScales[effectRoot] = baseLocalScale;
        }

        Transform wrapper = ResolveAttackEffectWrapper(effectRoot, anchor);
        if (wrapper == null)
        {
            return;
        }

        if (effectRoot.parent != wrapper)
        {
            effectRoot.SetParent(wrapper, false);
        }

        bool mirrored = IsMirroredTransform(anchor);
        wrapper.localPosition = Vector3.zero;
        wrapper.localRotation = mirrored ? Quaternion.AngleAxis(180f, Vector3.up) : Quaternion.identity;
        wrapper.localScale = mirrored ? new Vector3(-1f, 1f, 1f) : Vector3.one;
        effectRoot.localScale = baseLocalScale;
    }

    private Transform ResolveAttackEffectWrapper(Transform effectRoot, Transform anchor)
    {
        if (effectRoot == null || anchor == null)
        {
            return null;
        }

        if (!attackEffectWrappers.TryGetValue(effectRoot, out Transform wrapper) || wrapper == null)
        {
            GameObject wrapperObject = new GameObject(effectRoot.name + "_AttackEffectAnchor");
            wrapper = wrapperObject.transform;
            attackEffectWrappers[effectRoot] = wrapper;
        }

        if (wrapper.parent != anchor)
        {
            wrapper.SetParent(anchor, false);
        }

        return wrapper;
    }

    private static bool IsMirroredTransform(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        Matrix4x4 matrix = target.localToWorldMatrix;
        Vector3 right = matrix.GetColumn(0);
        Vector3 up = matrix.GetColumn(1);
        Vector3 forward = matrix.GetColumn(2);
        return Vector3.Dot(Vector3.Cross(right, up), forward) < 0f;
    }

    private Transform ResolveAttackEffectAnchor()
    {
        Transform weaponTransform = weaponHitbox != null ? weaponHitbox.transform : null;
        Transform current = weaponTransform;
        while (current != null && current != transform)
        {
            if (NameMatchesWeaponAnchor(current.name))
            {
                return current;
            }

            current = current.parent;
        }

        Transform namedAnchor = FindChildByName(transform, "WeaponAnchor");
        if (namedAnchor != null)
        {
            return namedAnchor;
        }

        return weaponTransform != null ? weaponTransform : transform;
    }

    private static bool NameMatchesWeaponAnchor(string objectName)
    {
        return string.Equals(objectName, "WeaponAnchor", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(objectName, "Weapon Anchor", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(objectName, "weapon_anchor", System.StringComparison.OrdinalIgnoreCase);
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child != null && string.Equals(child.name, childName, System.StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }
        }

        return null;
    }

    private void CacheAttackEffect()
    {
        CacheAttackEffectParticles(null);
    }

    private void CacheAttackEffectParticles(Transform effectRoot)
    {
        cachedAttackEffectRoot = effectRoot;
        DisableAttackEffectAutoDestroy(effectRoot);

        attackEffectParticles = effectRoot != null
            ? effectRoot.GetComponentsInChildren<ParticleSystem>(true)
            : new ParticleSystem[0];

        for (int i = 0; i < attackEffectParticles.Length; i++)
        {
            ParticleSystem particleSystem = attackEffectParticles[i];
            if (particleSystem == null)
            {
                continue;
            }

            ParticleSystem.MainModule main = particleSystem.main;
            if (main.stopAction == ParticleSystemStopAction.Destroy)
            {
                main.stopAction = ParticleSystemStopAction.None;
            }
        }
    }

    private Transform ResolveAttackEffectRoot(PlayerWeaponAttackStep attackStep)
    {
        if (attackStep != null && attackStep.AttackEffectRoot != null)
        {
            return attackStep.AttackEffectRoot;
        }

        return null;
    }

    private void DisableAttackEffectAutoDestroy(Transform effectRoot)
    {
        if (effectRoot == null)
        {
            return;
        }

        CFXR_Effect[] effects = effectRoot.GetComponentsInChildren<CFXR_Effect>(true);
        for (int i = 0; i < effects.Length; i++)
        {
            CFXR_Effect effect = effects[i];
            if (effect != null)
            {
                effect.clearBehavior = CFXR_Effect.ClearBehavior.None;
            }
        }
    }

    private void StopAttackStepEffectsOnAwake()
    {
        int attackCount = GetAttackCount();
        for (int i = 0; i < attackCount; i++)
        {
            PlayerWeaponAttackStep attackStep = ResolveAttackStep(i);
            if (attackStep != null)
            {
                StopAttackEffectRoot(attackStep.AttackEffectRoot);
            }
        }
    }

    private void StopAttackEffectRoot(Transform effectRoot)
    {
        if (effectRoot == null || !IsSceneObject(effectRoot))
        {
            return;
        }

        DisableAttackEffectAutoDestroy(effectRoot);
        ParticleSystem[] particleSystems = effectRoot.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem particleSystem = particleSystems[i];
            if (particleSystem != null)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    private void CaptureBaseAnimatorSpeed()
    {
        animator = animator != null ? animator : GetComponentInChildren<Animator>();
        if (animator == null || capturedAnimatorSpeed)
        {
            return;
        }

        baseAnimatorSpeed = animator.speed;
        capturedAnimatorSpeed = true;
    }

    private bool HasAnimatorParameter(int parameterHash)
    {
        if (animator == null)
        {
            return false;
        }

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == parameterHash)
            {
                return true;
            }
        }

        return false;
    }

    private bool EnsureWeaponHitbox()
    {
        if (weaponHitbox == null)
        {
            weaponHitbox = GetComponentInChildren<PlayerWeaponHitbox>(true);
        }

        if (weaponHitbox == null)
        {
            if (!warnedMissingWeaponHitbox)
            {
                Debug.LogWarning("PlayerCombat3D could not find PlayerWeaponHitbox. Put the current weapon under WeaponAnchor and add PlayerWeaponHitbox plus PlayerWeaponAttackProfile.", this);
                warnedMissingWeaponHitbox = true;
            }

            runtimeWeaponAttackProfile = null;
            return false;
        }

        warnedMissingWeaponHitbox = false;

        runtimeWeaponAttackProfile = ResolveWeaponAttackProfile();

        PlayerWeaponAttackStep defaultAttack = ResolveAttackStep(0);
        weaponHitbox.Configure(defaultAttack != null ? defaultAttack.Damage : FallbackDamage, GetWeaponTargetMask());
        weaponHitbox.ConfigureHitSounds(GetWeaponAttackHitSound(), GetWeaponAttackHitSoundVolume(), GetWeaponTargetHitSounds());
        if (defaultAttack != null)
        {
            weaponHitbox.ConfigureCameraShake(defaultAttack.CameraShakeAmplitude, defaultAttack.CameraShakeDuration, defaultAttack.CameraShakeFrequency);
        }
        return true;
    }
}
