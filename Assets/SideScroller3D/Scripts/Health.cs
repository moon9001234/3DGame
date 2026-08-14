using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("Maximum health for this object.")]
    [SerializeField] private int maxHealth = 5;

    [Tooltip("Seconds of invulnerability after taking damage.")]
    [SerializeField] private float invulnerableSeconds = 0.35f;

    private int currentHealth;
    private float invulnerableUntil;

    public event Action<int, int> Changed;
    public event Action<int, Vector3> Damaged;
    public event Action Died;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        currentHealth = maxHealth;
        EnsureEnemyHealthBar();
        EnsureEnemyDamageFlash();
        EnsurePlayerDamageFlash();
        EnsurePlayerHud();
        RemovePlayerHitEffect();
    }

    private void Start()
    {
        EnsureEnemyHealthBar();
        EnsureEnemyDamageFlash();
        EnsurePlayerDamageFlash();
        EnsurePlayerHud();
        RemovePlayerHitEffect();
    }

    private void EnsureEnemyHealthBar()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Enemy") && GetComponent<EnemyHealthBar3D>() == null)
        {
            gameObject.AddComponent<EnemyHealthBar3D>();
        }
    }

    private void EnsureEnemyDamageFlash()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Enemy") && GetComponent<EnemyDamageFlash>() == null)
        {
            gameObject.AddComponent<EnemyDamageFlash>();
        }
    }

    private void EnsurePlayerDamageFlash()
    {
        if (gameObject.layer == LayerMask.NameToLayer("Player") && GetComponent<PlayerDamageFlash>() == null)
        {
            gameObject.AddComponent<PlayerDamageFlash>();
        }
    }

    private void EnsurePlayerHud()
    {
        if (IsPlayerHealth())
        {
            SideScrollerHUD.EnsureRuntimeHud(this);
        }
    }

    private bool IsPlayerHealth()
    {
        return gameObject.layer == LayerMask.NameToLayer("Player")
            || ResolvePlayerMotor() != null;
    }

    private void RemovePlayerHitEffect()
    {
        if (gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            return;
        }

        DamageHitEffect3D hitEffect = GetComponent<DamageHitEffect3D>();
        if (hitEffect == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(hitEffect);
        }
        else
        {
            DestroyImmediate(hitEffect);
        }
    }

    public bool TryTakeDamage(int amount)
    {
        return TryTakeDamage(amount, transform.position - Vector3.right);
    }

    public bool TryTakeDamage(int amount, Vector3 damageSourcePosition)
    {
        return TryTakeDamage(amount, damageSourcePosition, false);
    }

    public bool TryTakeDamage(int amount, Vector3 damageSourcePosition, bool ignoreInvulnerability)
    {
        if (IsDead || (!ignoreInvulnerability && Time.time < invulnerableUntil))
        {
            return false;
        }

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        invulnerableUntil = Time.time + invulnerableSeconds;
        Changed?.Invoke(currentHealth, maxHealth);
        Damaged?.Invoke(amount, damageSourcePosition);
        PlayDamageFlashFeedback();
        ApplyPlayerDamageKnockback(damageSourcePosition);

        if (currentHealth == 0)
        {
            Died?.Invoke();
        }

        return true;
    }

    private void ApplyPlayerDamageKnockback(Vector3 damageSourcePosition)
    {
        PlayerMotor3D motor = ResolvePlayerMotor();
        if (motor != null)
        {
            motor.ApplyKnockback(damageSourcePosition);
        }
    }

    private PlayerMotor3D ResolvePlayerMotor()
    {
        PlayerMotor3D motor = GetComponent<PlayerMotor3D>();
        if (motor != null)
        {
            return motor;
        }

        motor = GetComponentInParent<PlayerMotor3D>();
        if (motor != null)
        {
            return motor;
        }

        return GetComponentInChildren<PlayerMotor3D>();
    }

    public void Kill()
    {
        if (IsDead)
        {
            return;
        }

        currentHealth = 0;
        Changed?.Invoke(currentHealth, maxHealth);
        Died?.Invoke();
    }

    private void PlayDamageFlashFeedback()
    {
        EnemyDamageFlash enemyFlash = GetComponent<EnemyDamageFlash>();
        if (enemyFlash != null)
        {
            enemyFlash.PlayFlash();
        }

        PlayerDamageFlash playerFlash = GetComponent<PlayerDamageFlash>();
        if (playerFlash != null)
        {
            playerFlash.PlayFlash();
        }

        DamageHitEffect3D hitEffect = GetComponent<DamageHitEffect3D>();
        if (hitEffect != null && gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            hitEffect.PlayEffect();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Changed?.Invoke(currentHealth, maxHealth);
    }

    public void ReviveFull()
    {
        currentHealth = maxHealth;
        invulnerableUntil = 0f;
        Changed?.Invoke(currentHealth, maxHealth);
    }
}
