using System.Collections.Generic;
using UnityEngine;

// 玩家武器的實際攻擊判定。攻擊期間會掃描判定盒，傷害敵人或反彈可反彈投射物。
public class PlayerWeaponHitbox : MonoBehaviour
{
    private const string DefaultProjectileReflectEffectPath = "Assets/Art/Prefab/FX/CFXR4 Sword Hit FIRE (Cross) 1.prefab";

    [Header("武器判定")]
    [Tooltip("沒有使用模型邊界時，暫代武器顯示用的大小。手動調整攻擊範圍時，請直接調 Box Collider。")]
    [SerializeField] private Vector3 weaponSize = new Vector3(1.35f, 0.16f, 0.16f);

    [Tooltip("沒有掛武器模型時，暫代武器方塊的顏色。")]
    [SerializeField] private Color weaponColor = new Color(0.85f, 0.82f, 0.72f, 1f);

    [Tooltip("用來計算攻擊判定範圍的武器模型根物件，例如 TV_Weapon_05。")]
    [SerializeField] private Transform weaponModelRoot;

    [Tooltip("開啟後，編輯模式會依照 Weapon Model Root 的 Renderer 邊界自動設定 Box Collider。關閉後可直接手動調 Collider。")]
    [SerializeField] private bool useModelBoundsForHitbox;

    [Tooltip("開啟後，Play 模式也會依照模型邊界更新 Collider。通常關閉，避免手動調整被覆蓋。")]
    [SerializeField] private bool updateColliderDuringPlay;

    [Tooltip("依照模型邊界建立判定盒時，額外增加的寬度，避免判定太貼模型。")]
    [SerializeField] private Vector3 modelBoundsPadding = new Vector3(0.04f, 0.04f, 0.04f);

    [Header("火球反擊")]
    [Tooltip("只用於反擊火球的額外判定範圍，單位是世界座標。這不會放大打敵人的範圍。")]
    [SerializeField] private Vector3 projectileReflectExtraRange = new Vector3(0.45f, 0.3f, 0.3f);

    [Tooltip("火球反擊成功瞬間播放的特效 Prefab。預設使用 CFXR4 Sword Hit FIRE (Cross) 1。")]
    [SerializeField] private GameObject projectileReflectEffectPrefab;

    [Tooltip("反擊特效大小倍率。")]
    [SerializeField] private float projectileReflectEffectScale = 1f;

    [Tooltip("沒有粒子生命週期可判斷時，反擊特效保留幾秒後自動刪除。")]
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
