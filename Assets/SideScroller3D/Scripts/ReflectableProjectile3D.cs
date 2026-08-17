using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ReflectableProjectile3D : MonoBehaviour
{
    [Header("\u6295\u5c04\u7269")]
    [Tooltip("\u706b\u7403\u98db\u884c\u901f\u5ea6\u3002\u6575\u4eba\u767c\u5c04\u6642\u6703\u7531 EnemyPatrol3D \u7684 Projectile Speed \u8986\u84cb\u3002")]
    [SerializeField] private float speed = 5.5f;

    [Tooltip("\u706b\u7403\u5b58\u5728\u5e7e\u79d2\u5f8c\u81ea\u52d5\u6d88\u5931\u3002\u6575\u4eba\u767c\u5c04\u6642\u6703\u7531 EnemyPatrol3D \u7684 Projectile Lifetime \u8986\u84cb\u3002")]
    [SerializeField] private float lifetime = 5f;

    [Tooltip("\u706b\u7403\u547d\u4e2d\u73a9\u5bb6\u6642\u9020\u6210\u7684\u50b7\u5bb3\u3002\u6575\u4eba\u767c\u5c04\u6642\u6703\u7531 EnemyPatrol3D \u7684 Projectile Damage \u8986\u84cb\u3002")]
    [SerializeField] private int damage = 1;

    [Tooltip("\u706b\u7403\u88ab\u73a9\u5bb6\u53cd\u64ca\u5f8c\u547d\u4e2d\u6575\u4eba\u6642\u9020\u6210\u7684\u50b7\u5bb3\u3002\u9810\u8a2d\u6703\u540c\u6b65\u4f7f\u7528\u6575\u4eba\u7684 Projectile Damage\u3002")]
    [SerializeField] private int reflectedDamage = 1;

    [Tooltip("\u706b\u7403\u6b63\u5e38\u98db\u884c\u6642\u5957\u7528\u5230 Renderer \u7684\u984f\u8272\u3002")]
    [SerializeField] private Color fireColor = new Color(1f, 0.35f, 0.05f, 1f);

    [Tooltip("\u706b\u7403\u88ab\u73a9\u5bb6\u53cd\u64ca\u5f8c\u5957\u7528\u5230 Renderer \u7684\u984f\u8272\u3002")]
    [SerializeField] private Color reflectedColor = new Color(0.25f, 0.85f, 1f, 1f);

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u73a9\u5bb6\u653b\u64ca\u706b\u7403\u6642\u53ef\u4ee5\u628a\u706b\u7403\u53cd\u5f48\u56de\u6575\u4eba\u3002Boss \u706b\u7403\u901a\u5e38\u95dc\u9589\u3002")]
    [SerializeField] private bool canBeReflected = true;

    private Vector3 direction = Vector3.right;
    private GameObject owner;
    private BoxCollider hitbox;
    private Renderer[] visualRenderers;
    private float destroyAt;
    private bool reflected;
    private bool usesCustomVisualTemplate;
    private AudioClip hitSound;
    private float hitSoundVolume = 1f;

    public bool IsReflected => reflected;
    public bool CanBeReflected => canBeReflected;

    private void Awake()
    {
        EnsureParts();
        destroyAt = Time.time + lifetime;
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (Time.time >= destroyAt)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    public void Launch(Vector3 launchDirection, GameObject projectileOwner, int projectileDamage, float projectileSpeed, float projectileLifetime)
    {
        Launch(launchDirection, projectileOwner, projectileDamage, projectileSpeed, projectileLifetime, true);
    }

    public void Launch(Vector3 launchDirection, GameObject projectileOwner, int projectileDamage, float projectileSpeed, float projectileLifetime, bool projectileCanBeReflected)
    {
        direction = FlattenDirection(launchDirection);
        owner = projectileOwner;
        damage = Mathf.Max(0, projectileDamage);
        reflectedDamage = damage;
        canBeReflected = projectileCanBeReflected;
        speed = Mathf.Max(0.1f, projectileSpeed);
        lifetime = Mathf.Max(0.1f, projectileLifetime);
        destroyAt = Time.time + lifetime;
        reflected = false;
        ApplyColor(fireColor);
    }

    public void ConfigureHitSound(AudioClip clip, float volume)
    {
        hitSound = clip;
        hitSoundVolume = Mathf.Clamp01(volume);
    }

    public bool TryGetHitSound(out AudioClip clip, out float volume)
    {
        clip = hitSound;
        volume = hitSoundVolume;
        return clip != null && volume > 0f;
    }

    public void UseVisualTemplate(Transform visualTemplate)
    {
        if (visualTemplate == null)
        {
            return;
        }

        Transform fallbackVisual = transform.Find("Projectile_Visual");
        if (fallbackVisual != null)
        {
            fallbackVisual.gameObject.SetActive(false);
            Destroy(fallbackVisual.gameObject);
        }

        usesCustomVisualTemplate = true;
        GameObject visualObject = Instantiate(visualTemplate.gameObject, transform);
        visualObject.name = visualTemplate.name;
        visualObject.transform.localPosition = visualTemplate.localPosition;
        visualObject.transform.localRotation = visualTemplate.localRotation;
        visualObject.transform.localScale = visualTemplate.localScale;
        visualObject.SetActive(true);

        Collider[] visualColliders = visualObject.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < visualColliders.Length; i++)
        {
            visualColliders[i].enabled = false;
            Destroy(visualColliders[i]);
        }

        visualRenderers = visualObject.GetComponentsInChildren<Renderer>(true);
        FitHitboxToRenderers();
        PlayParticleSystems(visualObject.transform);
        ApplyColor(reflected ? reflectedColor : fireColor);
    }

    public bool TryReflectFrom(Vector3 sourcePosition, int reflectDamage)
    {
        if (reflected || !canBeReflected)
        {
            return false;
        }

        Vector3 axis = FlattenDirection(direction);
        float side = Mathf.Sign(Vector3.Dot(transform.position - sourcePosition, axis));
        if (Mathf.Abs(side) < 0.01f)
        {
            side = -1f;
        }

        direction = axis * side;
        reflectedDamage = Mathf.Max(0, reflectDamage);
        reflected = true;
        destroyAt = Time.time + lifetime;
        ApplyColor(reflectedColor);
        return true;
    }

    public void ConfigureGeneratedVisual(PrimitiveType shape, Color normalColor, Color reflectedProjectileColor, Vector3 visualScale, string visualName)
    {
        EnsureParts();
        fireColor = normalColor;
        reflectedColor = reflectedProjectileColor;
        usesCustomVisualTemplate = false;
        hitbox.size = Abs(visualScale);
        hitbox.center = Vector3.zero;

        Transform existingVisual = transform.Find("Projectile_Visual");
        if (existingVisual != null)
        {
            Destroy(existingVisual.gameObject);
        }

        GameObject visualObject = GameObject.CreatePrimitive(shape);
        visualObject.name = string.IsNullOrEmpty(visualName) ? "Projectile_Visual" : visualName;
        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localPosition = Vector3.zero;
        visualObject.transform.localRotation = Quaternion.identity;
        visualObject.transform.localScale = visualScale;

        Collider visualCollider = visualObject.GetComponent<Collider>();
        if (visualCollider != null)
        {
            Destroy(visualCollider);
        }

        visualObject.name = "Projectile_Visual";
        visualRenderers = visualObject.GetComponentsInChildren<Renderer>(true);
        ApplyColor(reflected ? reflectedColor : fireColor);
    }

    private void TryHit(Collider other)
    {
        if (other == null || other.transform.IsChildOf(transform))
        {
            return;
        }

        Health targetHealth = ResolveTargetHealth(other);
        if (targetHealth == null || (!reflected && targetHealth.gameObject == owner))
        {
            return;
        }

        bool targetIsPlayer = targetHealth.GetComponent<PlayerMotor3D>() != null;
        if (!reflected && !targetIsPlayer)
        {
            return;
        }

        if (reflected && targetIsPlayer)
        {
            return;
        }

        bool ignoreInvulnerability = !reflected && targetIsPlayer;
        if (targetHealth.TryTakeDamage(reflected ? reflectedDamage : damage, transform.position, ignoreInvulnerability))
        {
            PlayHitSound();
        }

        Destroy(gameObject);
    }

    private void PlayHitSound()
    {
        if (hitSound == null || hitSoundVolume <= 0f)
        {
            return;
        }

        SideScrollerSfxPlayer.PlayOneShot(hitSound, hitSoundVolume);
    }

    private Health ResolveTargetHealth(Collider other)
    {
        EnemyHurtbox3D hurtbox = other.GetComponentInParent<EnemyHurtbox3D>();
        if (hurtbox != null)
        {
            return hurtbox.TargetHealth;
        }

        return other.GetComponentInParent<Health>();
    }

    private void EnsureParts()
    {
        hitbox = GetComponent<BoxCollider>();
        hitbox.isTrigger = true;
        hitbox.size = Vector3.one;
        hitbox.center = Vector3.zero;

        Transform visual = transform.Find("Projectile_Visual");
        if (visual == null)
        {
            GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualObject.name = "Projectile_Visual";
            visualObject.transform.SetParent(transform, false);
            visual = visualObject.transform;

            Collider visualCollider = visualObject.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Destroy(visualCollider);
            }
        }

        visual.localPosition = Vector3.zero;
        visual.localRotation = Quaternion.identity;
        visual.localScale = Vector3.one;
        visualRenderers = visual.GetComponentsInChildren<Renderer>(true);
        usesCustomVisualTemplate = false;
        ApplyColor(reflected ? reflectedColor : fireColor);
    }

    private void ApplyColor(Color color)
    {
        if (usesCustomVisualTemplate && !reflected)
        {
            return;
        }

        if (visualRenderers == null || visualRenderers.Length == 0)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        for (int i = 0; i < visualRenderers.Length; i++)
        {
            Renderer renderer = visualRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (renderer.material == null)
            {
                renderer.material = new Material(shader);
            }

            Material material = renderer.material;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }
    }

    private void FitHitboxToRenderers()
    {
        if (hitbox == null || visualRenderers == null || visualRenderers.Length == 0)
        {
            return;
        }

        Bounds localBounds = new Bounds();
        bool hasBounds = false;
        for (int i = 0; i < visualRenderers.Length; i++)
        {
            Renderer renderer = visualRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (renderer is ParticleSystemRenderer)
            {
                continue;
            }

            Bounds worldBounds = renderer.bounds;
            Vector3 min = worldBounds.min;
            Vector3 max = worldBounds.max;
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(min.x, min.y, min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(min.x, min.y, max.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(min.x, max.y, min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(min.x, max.y, max.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(max.x, min.y, min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(max.x, min.y, max.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(max.x, max.y, min.z));
            EncapsulateLocalPoint(ref localBounds, ref hasBounds, new Vector3(max.x, max.y, max.z));
        }

        if (!hasBounds || localBounds.size.sqrMagnitude < 0.0001f)
        {
            return;
        }

        hitbox.center = localBounds.center;
        hitbox.size = new Vector3(
            Mathf.Max(0.1f, Mathf.Abs(localBounds.size.x)),
            Mathf.Max(0.1f, Mathf.Abs(localBounds.size.y)),
            Mathf.Max(0.1f, Mathf.Abs(localBounds.size.z)));
    }

    private void EncapsulateLocalPoint(ref Bounds localBounds, ref bool hasBounds, Vector3 worldPoint)
    {
        Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
        if (!hasBounds)
        {
            localBounds = new Bounds(localPoint, Vector3.zero);
            hasBounds = true;
            return;
        }

        localBounds.Encapsulate(localPoint);
    }

    private static void PlayParticleSystems(Transform root)
    {
        ParticleSystem[] particles = root.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].Clear(true);
            particles[i].Play(true);
        }
    }

    private static Vector3 FlattenDirection(Vector3 value)
    {
        value.y = 0f;
        if (value.sqrMagnitude < 0.0001f)
        {
            return Vector3.right;
        }

        return value.normalized;
    }

    private static Vector3 Abs(Vector3 value)
    {
        return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
    }
}
