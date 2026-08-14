using UnityEngine;

[RequireComponent(typeof(Health))]
[RequireComponent(typeof(Rigidbody))]
// 讀取敵人的 Rigidbody 速度與戰鬥狀態，並把 Speed、Attack、InCombat 參數送進子物件上的 Animator。
// 這支腳本沒有 Inspector 可調參數；動畫片段與狀態切換主要由 Animator Controller 決定。
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

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnDisable()
    {
        ResumeHit();
    }

    private void Update()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
            return;
        }

        animator.SetFloat(SpeedHash, Vector3.ProjectOnPlane(body.linearVelocity, Vector3.up).magnitude);
        UpdateHitPause();
    }

    public float PlayAttack()
    {
        animator = animator != null ? animator : GetComponentInChildren<Animator>();
        if (animator == null)
        {
            return 0f;
        }

        if (HasAnimatorParameter(AttackHash))
        {
            animator.SetTrigger(AttackHash);
            return GetAttackClipLength();
        }

        if (animator.HasState(0, AttackHash))
        {
            animator.CrossFade(AttackHash, 0.04f, 0, 0f);
        }

        return GetAttackClipLength();
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
