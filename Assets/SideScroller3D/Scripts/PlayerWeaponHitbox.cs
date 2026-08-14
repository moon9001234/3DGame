using System.Collections.Generic;
using UnityEngine;

// \u73a9\u5bb6\u6b66\u5668\u7684\u5be6\u969b\u653b\u64ca\u5224\u5b9a\u3002\u653b\u64ca\u671f\u9593\u6703\u6383\u63cf\u5224\u5b9a\u76d2\uff0c\u50b7\u5bb3\u6575\u4eba\u6216\u53cd\u5f48\u53ef\u53cd\u5f48\u6295\u5c04\u7269\u3002
public class PlayerWeaponHitbox : MonoBehaviour
{
    private const string DefaultProjectileReflectEffectPath = "Assets/Art/Prefab/FX/CFXR4 Sword Hit FIRE (Cross) 1.prefab";

    [Header("\u6b66\u5668\u5224\u5b9a")]
    [Tooltip("\u6c92\u6709\u4f7f\u7528\u6a21\u578b\u908a\u754c\u6642\uff0c\u66ab\u4ee3\u6b66\u5668\u986f\u793a\u7528\u7684\u5927\u5c0f\u3002\u624b\u52d5\u8abf\u6574\u653b\u64ca\u7bc4\u570d\u6642\uff0c\u8acb\u76f4\u63a5\u8abf Box Collider\u3002")]
    [SerializeField] private Vector3 weaponSize = new Vector3(1.35f, 0.16f, 0.16f);

    [Tooltip("\u6c92\u6709\u639b\u6b66\u5668\u6a21\u578b\u6642\uff0c\u66ab\u4ee3\u6b66\u5668\u65b9\u584a\u7684\u984f\u8272\u3002")]
    [SerializeField] private Color weaponColor = new Color(0.85f, 0.82f, 0.72f, 1f);

    [Tooltip("\u7528\u4f86\u8a08\u7b97\u653b\u64ca\u5224\u5b9a\u7bc4\u570d\u7684\u6b66\u5668\u6a21\u578b\u6839\u7269\u4ef6\uff0c\u4f8b\u5982 TV_Weapon_05\u3002")]
    [SerializeField] private Transform weaponModelRoot;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u7de8\u8f2f\u6a21\u5f0f\u6703\u4f9d\u7167 Weapon Model Root \u7684 Renderer \u908a\u754c\u81ea\u52d5\u8a2d\u5b9a Box Collider\u3002\u95dc\u9589\u5f8c\u53ef\u76f4\u63a5\u624b\u52d5\u8abf Collider\u3002")]
    [SerializeField] private bool useModelBoundsForHitbox;

    [Tooltip("\u958b\u555f\u5f8c\uff0cPlay \u6a21\u5f0f\u4e5f\u6703\u4f9d\u7167\u6a21\u578b\u908a\u754c\u66f4\u65b0 Collider\u3002\u901a\u5e38\u95dc\u9589\uff0c\u907f\u514d\u624b\u52d5\u8abf\u6574\u88ab\u8986\u84cb\u3002")]
    [SerializeField] private bool updateColliderDuringPlay;

    [Tooltip("\u4f9d\u7167\u6a21\u578b\u908a\u754c\u5efa\u7acb\u5224\u5b9a\u76d2\u6642\uff0c\u984d\u5916\u589e\u52a0\u7684\u5bec\u5ea6\uff0c\u907f\u514d\u5224\u5b9a\u592a\u8cbc\u6a21\u578b\u3002")]
    [SerializeField] private Vector3 modelBoundsPadding = new Vector3(0.04f, 0.04f, 0.04f);

    [Header("\u706b\u7403\u53cd\u64ca")]
    [Tooltip("\u53ea\u7528\u65bc\u53cd\u64ca\u706b\u7403\u7684\u984d\u5916\u5224\u5b9a\u7bc4\u570d\uff0c\u55ae\u4f4d\u662f\u4e16\u754c\u5ea7\u6a19\u3002\u9019\u4e0d\u6703\u653e\u5927\u6253\u6575\u4eba\u7684\u7bc4\u570d\u3002")]
    [SerializeField] private Vector3 projectileReflectExtraRange = new Vector3(0.45f, 0.3f, 0.3f);

    [Tooltip("\u706b\u7403\u53cd\u64ca\u6210\u529f\u77ac\u9593\u64ad\u653e\u7684\u7279\u6548 Prefab\u3002\u9810\u8a2d\u4f7f\u7528 CFXR4 Sword Hit FIRE (Cross) 1\u3002")]
    [SerializeField] private GameObject projectileReflectEffectPrefab;

    [Tooltip("\u53cd\u64ca\u7279\u6548\u5927\u5c0f\u500d\u7387\u3002")]
    [SerializeField] private float projectileReflectEffectScale = 1f;

    [Tooltip("\u6c92\u6709\u7c92\u5b50\u751f\u547d\u9031\u671f\u53ef\u5224\u65b7\u6642\uff0c\u53cd\u64ca\u7279\u6548\u4fdd\u7559\u5e7e\u79d2\u5f8c\u81ea\u52d5\u522a\u9664\u3002")]
    [SerializeField] private float projectileReflectEffectFallbackLifetime = 2f;

    private readonly HashSet<Health> damagedThisSwing = new HashSet<Health>();

    private BoxCollider hitbox;
    private Renderer visualRenderer;
    private LayerMask enemyMask;
    private int damage = 1;
    private float activeUntil;
    private Health ownerHealth;
    private AudioClip hitSound;
    private float hitSoundVolume = 1f;
    private PlayerHitSoundRule[] targetHitSounds = new PlayerHitSoundRule[0];
    private float cameraShakeAmplitude;
    private float cameraShakeDuration;
    private float cameraShakeFrequency;
    private readonly Collider[] overlapResults = new Collider[32];
    private readonly Collider[] projectileReflectResults = new Collider[32];

    private bool IsActive => Time.time < activeUntil;

    private void Awake()
    {
        ownerHealth = GetComponentInParent<Health>();
        EnsureParts();
        SetHitboxActive(false);
    }

    private void Update()
    {
        bool active = IsActive;
        SetHitboxActive(active);

        if (active)
        {
            ScanForTargets();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    public void Configure(int attackDamage, LayerMask targetMask)
    {
        damage = attackDamage;
        enemyMask = targetMask;
    }

    public void ConfigureHitSound(AudioClip clip, float volume)
    {
        ConfigureHitSounds(clip, volume, null);
    }

    public void ConfigureHitSounds(AudioClip clip, float volume, PlayerHitSoundRule[] hitSoundRules)
    {
        hitSound = clip;
        hitSoundVolume = Mathf.Clamp01(volume);
        targetHitSounds = hitSoundRules ?? new PlayerHitSoundRule[0];
    }

    public void ConfigureCameraShake(float amplitude, float duration, float frequency)
    {
        cameraShakeAmplitude = Mathf.Max(0f, amplitude);
        cameraShakeDuration = Mathf.Max(0f, duration);
        cameraShakeFrequency = Mathf.Max(0f, frequency);
    }

    public void BeginSwing(float activeSeconds)
    {
        damagedThisSwing.Clear();
        activeUntil = Time.time + Mathf.Max(0.01f, activeSeconds);
        SetHitboxActive(true);
        ScanForTargets();
    }

    public void RefreshParts()
    {
        EnsureParts();
    }

    public void SetSize(Vector3 size)
    {
        weaponSize = size;
        EnsureParts();
        transform.localScale = weaponSize;
    }

    public void SetWorldSize(Vector3 size)
    {
        weaponSize = size;
        EnsureParts();
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            SafeDivide(size.x, parentScale.x),
            SafeDivide(size.y, parentScale.y),
            SafeDivide(size.z, parentScale.z));
    }

    private void TryDamage(Collider other)
    {
        if (!IsActive)
        {
            return;
        }

        ReflectableProjectile3D projectile = other.GetComponentInParent<ReflectableProjectile3D>();
        if (projectile != null && TryReflectProjectile(projectile, other, GetHitboxWorldCenter()))
        {
            return;
        }

        damagedThisSwing.RemoveWhere(damagedHealth => damagedHealth == null);

        if (!TryResolveTargetHealth(other, out Health health) || IsOwnerHealth(health) || damagedThisSwing.Contains(health))
        {
            return;
        }

        Transform targetTransform = health.transform;
        if (health.TryTakeDamage(damage, transform.root.position, true))
        {
            if (health != null)
            {
                damagedThisSwing.Add(health);
            }

            PlayHitSound(other, targetTransform);
            PlayCameraShake();
        }
    }

    private void ScanForTargets()
    {
        if (hitbox == null)
        {
            return;
        }

        Physics.SyncTransforms();
        Vector3 center = transform.TransformPoint(hitbox.center);
        Vector3 halfExtents = Vector3.Scale(hitbox.size, Abs(transform.lossyScale)) * 0.5f;
        int hitCount = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents,
            overlapResults,
            transform.rotation,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            if (overlapResults[i] != null)
            {
                TryDamage(overlapResults[i]);
            }
        }

        ScanForReflectableProjectiles(center, halfExtents);
    }

    private void ScanForReflectableProjectiles(Vector3 center, Vector3 halfExtents)
    {
        Vector3 extraRange = Abs(projectileReflectExtraRange);
        if (extraRange.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        int hitCount = Physics.OverlapBoxNonAlloc(
            center,
            halfExtents + extraRange,
            projectileReflectResults,
            transform.rotation,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            Collider candidate = projectileReflectResults[i];
            if (candidate == null)
            {
                continue;
            }

            ReflectableProjectile3D projectile = candidate.GetComponentInParent<ReflectableProjectile3D>();
            if (projectile != null)
            {
                TryReflectProjectile(projectile, candidate, center);
            }
        }
    }

    private bool TryReflectProjectile(ReflectableProjectile3D projectile, Collider projectileCollider, Vector3 hitboxWorldCenter)
    {
        if (projectile == null || !projectile.TryReflectFrom(transform.root.position, damage))
        {
            return false;
        }

        PlayProjectileReflectEffect(projectile.transform, projectileCollider, hitboxWorldCenter);
        PlayProjectileReflectSound(projectile, projectileCollider);
        PlayCameraShake();
        return true;
    }

    private void PlayCameraShake()
    {
        if (cameraShakeAmplitude <= 0f || cameraShakeDuration <= 0f)
        {
            return;
        }

        CameraShake3D.Shake(cameraShakeAmplitude, cameraShakeDuration, cameraShakeFrequency);
    }

    private void PlayProjectileReflectSound(ReflectableProjectile3D projectile, Collider projectileCollider)
    {
        if (projectile != null && projectile.TryGetHitSound(out AudioClip projectileHitSound, out float projectileHitSoundVolume))
        {
            SideScrollerSfxPlayer.PlayOneShot(projectileHitSound, projectileHitSoundVolume);
            return;
        }

        PlayHitSound(projectileCollider, projectile != null ? projectile.transform : null);
    }

    private void PlayHitSound(Collider targetCollider, Transform targetRoot)
    {
        AudioClip clip = ResolveHitSound(targetCollider, targetRoot, out float volume);
        if (clip == null || volume <= 0f)
        {
            return;
        }

        SideScrollerSfxPlayer.PlayOneShot(clip, volume);
    }

    private AudioClip ResolveHitSound(Collider targetCollider, Transform targetRoot, out float volume)
    {
        for (int i = 0; i < targetHitSounds.Length; i++)
        {
            PlayerHitSoundRule rule = targetHitSounds[i];
            if (rule == null || !rule.Matches(targetCollider, targetRoot))
            {
                continue;
            }

            volume = rule.Volume;
            return rule.HitSound != null ? rule.HitSound : hitSound;
        }

        volume = hitSoundVolume;
        return hitSound;
    }

    private void PlayProjectileReflectEffect(Transform projectileTransform, Collider projectileCollider, Vector3 hitboxWorldCenter)
    {
        GameObject effectPrefab = ResolveProjectileReflectEffectPrefab();
        if (effectPrefab == null || projectileTransform == null)
        {
            return;
        }

        Vector3 effectPosition = ResolveReflectEffectPosition(projectileTransform, projectileCollider, hitboxWorldCenter);
        Quaternion effectRotation = ResolveReflectEffectRotation(projectileTransform.position, hitboxWorldCenter);
        GameObject effectInstance = Instantiate(effectPrefab, effectPosition, effectRotation);
        effectInstance.transform.localScale *= Mathf.Max(0.01f, projectileReflectEffectScale);

        PlayParticleSystems(effectInstance.transform);
        Destroy(effectInstance, ResolveEffectLifetime(effectInstance));
    }

    private GameObject ResolveProjectileReflectEffectPrefab()
    {
        if (projectileReflectEffectPrefab != null)
        {
            return projectileReflectEffectPrefab;
        }

#if UNITY_EDITOR
        projectileReflectEffectPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(DefaultProjectileReflectEffectPath);
#endif
        return projectileReflectEffectPrefab;
    }

    private Vector3 ResolveReflectEffectPosition(Transform projectileTransform, Collider projectileCollider, Vector3 hitboxWorldCenter)
    {
        if (projectileCollider == null)
        {
            return projectileTransform.position;
        }

        Vector3 closestPoint = projectileCollider.ClosestPoint(hitboxWorldCenter);
        if ((closestPoint - hitboxWorldCenter).sqrMagnitude <= 0.0001f)
        {
            return projectileTransform.position;
        }

        return closestPoint;
    }

    private Quaternion ResolveReflectEffectRotation(Vector3 projectilePosition, Vector3 hitboxWorldCenter)
    {
        Vector3 directionToProjectile = projectilePosition - hitboxWorldCenter;
        if (directionToProjectile.sqrMagnitude <= 0.0001f)
        {
            return transform.rotation;
        }

        return Quaternion.LookRotation(directionToProjectile.normalized, Vector3.up);
    }

    private float ResolveEffectLifetime(GameObject effectInstance)
    {
        float lifetime = Mathf.Max(0.1f, projectileReflectEffectFallbackLifetime);
        ParticleSystem[] particleSystems = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem particleSystem = particleSystems[i];
            if (particleSystem == null)
            {
                continue;
            }

            ParticleSystem.MainModule main = particleSystem.main;
            float particleLifetime = main.duration + main.startLifetime.constantMax;
            lifetime = Mathf.Max(lifetime, particleLifetime);
        }

        return lifetime;
    }

    private Vector3 GetHitboxWorldCenter()
    {
        return hitbox != null ? transform.TransformPoint(hitbox.center) : transform.position;
    }

    private void EnsureParts()
    {
        if (hitbox == null)
        {
            hitbox = GetComponent<BoxCollider>();
            if (hitbox == null)
            {
                hitbox = gameObject.AddComponent<BoxCollider>();
            }
        }

        hitbox.isTrigger = true;
        if (ShouldSyncHitboxToWeaponModel())
        {
            hitbox.size = Vector3.one;
            hitbox.center = Vector3.zero;
        }

        SyncHitboxToWeaponModel();

        bool hasModelRenderer = HasWeaponModelRenderer();
        if (!hasModelRenderer && visualRenderer == null)
        {
            Transform visual = transform.Find("Weapon_Visual");
            if (visual == null)
            {
                GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                visualObject.name = "Weapon_Visual";
                visualObject.transform.SetParent(transform, false);

                visual = visualObject.transform;
            }

            visual.localPosition = Vector3.zero;
            visual.localRotation = Quaternion.identity;
            visual.localScale = Vector3.one;
            visualRenderer = visual.GetComponent<Renderer>();
        }

        if (hasModelRenderer)
        {
            Transform fallbackVisual = transform.Find("Weapon_Visual");
            if (fallbackVisual != null)
            {
                fallbackVisual.gameObject.SetActive(false);
            }
        }
        else if (visualRenderer != null && visualRenderer.sharedMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            Material material = new Material(shader);
            ApplyWeaponColor(material);
            visualRenderer.sharedMaterial = material;
        }
        else if (visualRenderer != null && visualRenderer.sharedMaterial != null)
        {
            ApplyWeaponColor(visualRenderer.sharedMaterial);
        }

        RemoveNonHitboxColliders();
    }

    private void ApplyWeaponColor(Material material)
    {
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", weaponColor);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", weaponColor);
        }
    }

    private static void PlayParticleSystems(Transform root)
    {
        ParticleSystem[] particleSystems = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem particleSystem = particleSystems[i];
            if (particleSystem == null)
            {
                continue;
            }

            particleSystem.Clear(true);
            particleSystem.Play(true);
        }
    }

    private void SetHitboxActive(bool active)
    {
        if (hitbox != null)
        {
            hitbox.isTrigger = true;
            hitbox.enabled = active;
        }
    }

    private bool TryResolveTargetHealth(Collider other, out Health health)
    {
        EnemyHurtbox3D hurtbox = other.GetComponent<EnemyHurtbox3D>();
        if (hurtbox == null)
        {
            hurtbox = other.GetComponentInParent<EnemyHurtbox3D>();
        }

        if (hurtbox != null)
        {
            health = hurtbox.TargetHealth;
            return health != null && IsTargetLayer(other, health);
        }

        health = other.GetComponentInParent<Health>();
        if (health == null)
        {
            return false;
        }

        if (health.GetComponentInChildren<EnemyHurtbox3D>(true) != null)
        {
            return false;
        }

        return IsTargetLayer(other, health);
    }

    private bool IsTargetLayer(Collider other, Health health)
    {
        int colliderLayer = 1 << other.gameObject.layer;
        int healthLayer = 1 << health.gameObject.layer;
        return (enemyMask.value & (colliderLayer | healthLayer)) != 0;
    }

    private bool IsOwnerHealth(Health health)
    {
        if (health == null)
        {
            return false;
        }

        if (ownerHealth == null)
        {
            ownerHealth = GetComponentInParent<Health>();
        }

        return health == ownerHealth;
    }

    private void RemoveNonHitboxColliders()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (collider == hitbox)
            {
                continue;
            }

            collider.enabled = false;

            if (Application.isPlaying)
            {
                Destroy(collider);
            }
            else
            {
                DestroyImmediate(collider);
            }
        }
    }

    private void SyncHitboxToWeaponModel()
    {
        if (!ShouldSyncHitboxToWeaponModel())
        {
            return;
        }

        if (weaponModelRoot == null)
        {
            weaponModelRoot = FindWeaponModelRoot();
        }

        if (weaponModelRoot == null)
        {
            return;
        }

        Renderer[] renderers = weaponModelRoot.GetComponentsInChildren<Renderer>(true);
        bool hasBounds = false;
        Bounds localBounds = new Bounds(Vector3.zero, Vector3.zero);

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null || renderer.transform.name == "Weapon_Visual")
            {
                continue;
            }

            Bounds rendererBounds = renderer.bounds;
            Vector3 min = rendererBounds.min;
            Vector3 max = rendererBounds.max;
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(min.x, min.y, min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(min.x, min.y, max.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(min.x, max.y, min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(min.x, max.y, max.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(max.x, min.y, min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(max.x, min.y, max.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(max.x, max.y, min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(max.x, max.y, max.z));
        }

        if (!hasBounds)
        {
            return;
        }

        hitbox.center = localBounds.center;
        hitbox.size = localBounds.size + modelBoundsPadding;
    }

    private bool ShouldSyncHitboxToWeaponModel()
    {
        return useModelBoundsForHitbox && (!Application.isPlaying || updateColliderDuringPlay);
    }

    private Transform FindWeaponModelRoot()
    {
        Transform namedWeapon = transform.Find("TV_Weapon_05");
        if (namedWeapon != null)
        {
            return namedWeapon;
        }

        foreach (Transform child in transform)
        {
            if (child.name == "Weapon_Visual")
            {
                continue;
            }

            if (child.GetComponentInChildren<Renderer>(true) != null)
            {
                return child;
            }
        }

        return null;
    }

    private bool HasWeaponModelRenderer()
    {
        if (weaponModelRoot == null)
        {
            weaponModelRoot = FindWeaponModelRoot();
        }

        return weaponModelRoot != null && weaponModelRoot.GetComponentInChildren<Renderer>(true) != null;
    }

    private void EncapsulateLocalPoint(ref Bounds bounds, ref bool hasBounds, Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        if (!hasBounds)
        {
            bounds = new Bounds(localPoint, Vector3.zero);
            hasBounds = true;
        }
        else
        {
            bounds.Encapsulate(localPoint);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.1f, 0.85f, 1f, 0.95f);
        Gizmos.matrix = transform.localToWorldMatrix;
        BoxCollider box = hitbox != null ? hitbox : GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.DrawWireCube(box.center, box.size);
            Vector3 extraRange = Abs(projectileReflectExtraRange);
            if (extraRange.sqrMagnitude > 0.0001f)
            {
                Gizmos.color = new Color(0.25f, 0.85f, 1f, 0.35f);
                Vector3 scaledExtraRange = new Vector3(
                    SafeDivide(extraRange.x, transform.lossyScale.x),
                    SafeDivide(extraRange.y, transform.lossyScale.y),
                    SafeDivide(extraRange.z, transform.lossyScale.z));
                Gizmos.DrawWireCube(box.center, box.size + scaledExtraRange * 2f);
            }
        }
        else
        {
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        Gizmos.matrix = Matrix4x4.identity;
    }

    private static float SafeDivide(float value, float divisor)
    {
        if (Mathf.Abs(divisor) <= 0.0001f)
        {
            return value;
        }

        return value / Mathf.Abs(divisor);
    }

    private static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
