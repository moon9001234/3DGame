using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyVisualAnimator : MonoBehaviour
{
    private const string EnemyAttackClipName = "Monster_01_Atk_01";

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int InCombatHash = Animator.StringToHash("InCombat");

    private Rigidbody body;
    private Animator animator;
    private bool pauseHitAtNormalizedTime;
    private bool hitPaused;
    private float hitPauseNormalizedTime = 0.5f;
    private float animatorSpeedBeforeHitPause = 1f;
    private bool attackSpeedOverrideActive;
    private float attackSpeedRestoreTime;
    private float animatorSpeedBeforeAttack = 1f;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnDisable()
    {
        ResumeHit();
        ClearAttackSpeedOverride();
    }

    private void Update()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            return;
        }

        animator.SetFloat(SpeedHash, Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude);
        UpdateAttackSpeedOverride();
        UpdateHitPause();
    }

    public float PlayAttack()
    {
        return PlayAttack(1f);
    }

    public float PlayAttack(float speedMultiplier)
    {
        return PlayAttack(speedMultiplier, 0f);
    }

    public float PlayAttack(float speedMultiplier, float restoreAfterSeconds)
    {
        animator = animator != null ? animator : GetComponentInChildren<Animator>();
        if (animator == null)
        {
            return 0f;
        }

        float attackClipLength = GetAttackClipLength();
        float resolvedSpeedMultiplier = Mathf.Max(0.01f, speedMultiplier);
        float adjustedAttackLength = attackClipLength / resolvedSpeedMultiplier;
        float speedRestoreSeconds = restoreAfterSeconds > 0f ? restoreAfterSeconds : adjustedAttackLength;
        ApplyAttackSpeedOverride(resolvedSpeedMultiplier, speedRestoreSeconds);

        if (HasAnimatorParameter(AttackHash))
        {
            animator.SetTrigger(AttackHash);
            return adjustedAttackLength;
        }

        if (animator.HasState(0, AttackHash))
        {
            animator.CrossFade(AttackHash, 0.04f, 0, 0f);
        }

        return adjustedAttackLength;
    }

    public float GetAttackAnimationLength()
    {
        animator = animator != null ? animator : GetComponentInChildren<Animator>();
        return animator != null ? GetAttackClipLength() : 0f;
    }

    public float PlayHit()
    {
        animator = animator != null ? animator : GetComponentInChildren<Animator>();
        if (animator == null)
        {
            return 0f;
        }

        if (HasAnimatorParameter(HitHash))
        {
            animator.ResetTrigger(HitHash);
            animator.SetTrigger(HitHash);
            return GetHitClipLength();
        }

        if (animator.HasState(0, HitHash))
        {
            animator.CrossFade(HitHash, 0.04f, 0, 0f);
        }

        return GetHitClipLength();
    }

    public void PauseHitAtNormalizedTime(float normalizedTime)
    {
        pauseHitAtNormalizedTime = true;
        hitPaused = false;
        hitPauseNormalizedTime = Mathf.Clamp01(normalizedTime);
    }

    public void ResumeHit()
    {
        pauseHitAtNormalizedTime = false;
        if (!hitPaused || animator == null)
        {
            hitPaused = false;
            return;
        }

        animator.speed = animatorSpeedBeforeHitPause;
        hitPaused = false;
    }

    public void SetCombatMode(bool inCombat)
    {
        animator = animator != null ? animator : GetComponentInChildren<Animator>();
        if (animator == null)
        {
            return;
        }

        if (HasAnimatorParameter(InCombatHash))
        {
            animator.SetBool(InCombatHash, inCombat);
        }
    }

    private void UpdateHitPause()
    {
        if (!pauseHitAtNormalizedTime || hitPaused || animator == null)
        {
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.shortNameHash != HitHash)
        {
            return;
        }

        if (stateInfo.normalizedTime < hitPauseNormalizedTime || stateInfo.normalizedTime >= 1f)
        {
            return;
        }

        animatorSpeedBeforeHitPause = animator.speed;
        animator.speed = 0f;
        hitPaused = true;
    }

    private void ApplyAttackSpeedOverride(float speedMultiplier, float restoreAfterSeconds)
    {
        if (speedMultiplier <= 1.0001f || restoreAfterSeconds <= 0f)
        {
            ClearAttackSpeedOverride();
            return;
        }

        if (!attackSpeedOverrideActive)
        {
            animatorSpeedBeforeAttack = animator.speed;
        }

        animator.speed = animatorSpeedBeforeAttack * speedMultiplier;
        attackSpeedRestoreTime = Time.time + restoreAfterSeconds;
        attackSpeedOverrideActive = true;
    }

    private void UpdateAttackSpeedOverride()
    {
        if (!attackSpeedOverrideActive || hitPaused || Time.time < attackSpeedRestoreTime)
        {
            return;
        }

        ClearAttackSpeedOverride();
    }

    private void ClearAttackSpeedOverride()
    {
        if (!attackSpeedOverrideActive)
        {
            return;
        }

        if (animator != null)
        {
            animator.speed = animatorSpeedBeforeAttack;
        }

        attackSpeedOverrideActive = false;
    }

    private bool HasAnimatorParameter(int parameterHash)
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == parameterHash)
            {
                return true;
            }
        }

        return false;
    }

    private float GetAttackClipLength()
    {
        RuntimeAnimatorController controller = animator != null ? animator.runtimeAnimatorController : null;
        if (controller == null)
        {
            return 0f;
        }

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name.Contains("Atk_01") || clip.name.Contains("Attack"))
            {
                return clip.length;
            }
        }

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == EnemyAttackClipName || (clip.name.Contains("Atk") && !clip.name.Contains("Idle")))
            {
                return clip.length;
            }
        }

        return 0f;
    }

    private float GetHitClipLength()
    {
        RuntimeAnimatorController controller = animator != null ? animator.runtimeAnimatorController : null;
        if (controller == null)
        {
            return 0f;
        }

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name.Contains("Hit") || clip.name.Contains("Hurt") || clip.name.Contains("Damage"))
            {
                return clip.length;
            }
        }

        return 0f;
    }
}
