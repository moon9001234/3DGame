using System.Collections.Generic;
using UnityEngine;

public class DamageOnTouch : MonoBehaviour
{
    [Header("Contact Damage")]
    [Tooltip("Enable damage when this object touches a target layer.")]
    [SerializeField] private bool contactDamageEnabled;

    [Tooltip("Damage applied each time the cooldown allows a hit.")]
    [SerializeField] private int damage = 1;

    [Tooltip("Layers that can be damaged by this object, usually Player.")]
    [SerializeField] private LayerMask targetMask;

    [Tooltip("Seconds before the same Health can be damaged by this object again.")]
    [SerializeField] private float contactDamageCooldown = 0.8f;

    [Tooltip("Ignore the player's weapon hitbox so attacking an enemy does not count as the player touching it.")]
    [SerializeField] private bool ignorePlayerWeaponHitboxes = true;

    private readonly Dictionary<Health, float> nextDamageTimes = new Dictionary<Health, float>();

    public void Configure(bool enabled, int contactDamage, LayerMask targets, float cooldown)
    {
        contactDamageEnabled = enabled;
        damage = Mathf.Max(0, contactDamage);
        targetMask = targets;
        contactDamageCooldown = Mathf.Max(0.01f, cooldown);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryDamage(collision.collider);
    }

    private void OnCollisionStay(Collision collision)
    {
        TryDamage(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        if (!contactDamageEnabled || other == null)
        {
            return;
        }

        if (ignorePlayerWeaponHitboxes && other.GetComponentInParent<PlayerWeaponHitbox>() != null)
        {
            return;
        }

        Health health = other.GetComponentInParent<Health>();
        if (health == null)
        {
            return;
        }

        if (!IsInTargetMask(other.gameObject.layer) && !IsInTargetMask(health.gameObject.layer))
        {
            return;
        }

        if (nextDamageTimes.TryGetValue(health, out float nextTime) && Time.time < nextTime)
        {
            return;
        }

        if (health.TryTakeDamage(damage, transform.position))
        {
            nextDamageTimes[health] = Time.time + contactDamageCooldown;
        }
    }

    private bool IsInTargetMask(int layer)
    {
        return (targetMask.value & (1 << layer)) != 0;
    }
}
