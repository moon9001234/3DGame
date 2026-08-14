using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider))]
public class DeathZone3D : MonoBehaviour
{
    [Header("Death Zone")]
    [Tooltip("Layers that can be killed by this zone. Leave empty to use the Player layer.")]
    [SerializeField] private LayerMask targetMask;

    private void Awake()
    {
        EnsureTargetMask();
        EnsureTriggerCollider();
    }

    private void OnEnable()
    {
        EnsureTargetMask();
        EnsureTriggerCollider();
    }

    private void Reset()
    {
        EnsureTargetMask();
        EnsureTriggerCollider();
    }

    private void OnValidate()
    {
        EnsureTriggerCollider();
    }

    private void OnTriggerEnter(Collider other)
    {
        TryKill(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryKill(other);
    }

    private void TryKill(Collider other)
    {
        if (other == null)
        {
            return;
        }

        Health health = ResolveHealth(other);
        if (health == null)
        {
            return;
        }

        EnsureTargetMask();
        if (!IsTarget(other, health))
        {
            return;
        }

        health.Kill();
    }

    private void EnsureTargetMask()
    {
        if (targetMask.value != 0)
        {
            return;
        }

        int playerLayer = LayerMask.NameToLayer("Player");
        targetMask = playerLayer >= 0 ? LayerMask.GetMask("Player") : Physics.DefaultRaycastLayers;
    }

    private void EnsureTriggerCollider()
    {
        BoxCollider zoneCollider = GetComponent<BoxCollider>();
        if (zoneCollider == null)
        {
            zoneCollider = gameObject.AddComponent<BoxCollider>();
        }

        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider != null && collider != zoneCollider)
            {
                collider.enabled = false;
            }
        }

        zoneCollider.isTrigger = true;
    }

    private bool IsTarget(Collider other, Health health)
    {
        int mask = targetMask.value;
        if ((mask & (1 << other.gameObject.layer)) != 0)
        {
            return true;
        }

        if ((mask & (1 << health.gameObject.layer)) != 0)
        {
            return true;
        }

        PlayerMotor3D player = other.GetComponentInParent<PlayerMotor3D>();
        return player != null && (mask & (1 << player.gameObject.layer)) != 0;
    }

    private static Health ResolveHealth(Collider other)
    {
        Health health = other.GetComponentInParent<Health>();
        if (health != null)
        {
            return health;
        }

        return other.GetComponentInChildren<Health>();
    }
}

public static class DeathZoneRuntimeSetup
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        ApplyToNamedDeathObjects();
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToNamedDeathObjects();
    }

    private static void ApplyToNamedDeathObjects()
    {
        Collider[] colliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider == null || !IsNamedDeathObject(collider.gameObject))
            {
                continue;
            }

            if (collider.GetComponent<DeathZone3D>() == null)
            {
                collider.gameObject.AddComponent<DeathZone3D>();
            }
        }
    }

    private static bool IsNamedDeathObject(GameObject gameObject)
    {
        return gameObject != null && gameObject.name == "Death";
    }
}
