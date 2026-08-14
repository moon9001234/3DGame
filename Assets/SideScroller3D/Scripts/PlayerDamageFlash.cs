using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
public class PlayerDamageFlash : MonoBehaviour
{
    private static readonly string[] DefaultBaseColorProperties =
    {
        "_BaseColor",
        "_Color",
        "_MainColor",
        "_TintColor",
        "_BaseMapColor"
    };

    private static readonly string[] DefaultEmissionColorProperties =
    {
        "_EmissionColor",
        "_EmissiveColor",
        "_Emission",
        "_GlowColor",
        "_FresnelColor"
    };

    [Header("受傷閃光設定")]
    [Tooltip("玩家受到傷害時，模型材質會短暫切換成這個顏色。")]
    [SerializeField] private Color flashColor = new Color(1f, 0.04f, 0.02f, 1f);

    [Tooltip("玩家受傷閃光時的自發光顏色。數值可以大於 1，讓閃光更亮。")]
    [SerializeField] private Color emissionColor = new Color(3f, 0.05f, 0.02f, 1f);

    [Tooltip("受傷閃光維持的秒數。數值越大，紅光停留越久。")]
    [SerializeField] private float flashDuration = 0.12f;

    [Header("Shader 欄位相容")]
    [Tooltip("受傷時會嘗試寫入的基礎顏色欄位。新 shader 若使用自訂名稱，可加在這裡。")]
    [SerializeField] private string[] baseColorProperties =
    {
        "_BaseColor",
        "_Color",
        "_MainColor",
        "_TintColor",
        "_BaseMapColor"
    };

    [Tooltip("受傷時會嘗試寫入的自發光/高亮欄位。新 shader 若使用自訂名稱，可加在這裡。")]
    [SerializeField] private string[] emissionColorProperties =
    {
        "_EmissionColor",
        "_EmissiveColor",
        "_Emission",
        "_GlowColor",
        "_FresnelColor"
    };

    private readonly List<RendererSnapshot> rendererSnapshots = new List<RendererSnapshot>();

    private Health health;
    private Coroutine flashRoutine;
    private int lastHealth;
    private int lastFlashFrame = -1;

    private struct RendererSnapshot
    {
        public Renderer Renderer;
        public int MaterialIndex;
        public MaterialPropertyBlock PropertyBlock;
    }

    private void Awake()
    {
        health = GetComponent<Health>();
        lastHealth = -1;
        RemoveLegacyFlashLight();
    }

    private void Start()
    {
        if (lastHealth < 0 && health != null)
        {
            lastHealth = health.CurrentHealth;
        }
    }

    private void OnEnable()
    {
        if (health == null)
        {
            health = GetComponent<Health>();
        }

        if (health != null)
        {
            health.Changed += OnHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Changed -= OnHealthChanged;
        }

        ClearFlash();
    }

    private void OnHealthChanged(int current, int max)
    {
        if (lastHealth < 0)
        {
            if (current < max)
            {
                PlayFlash();
            }

            lastHealth = current;
            return;
        }

        if (current < lastHealth)
        {
            PlayFlash();
        }

        lastHealth = current;
    }

    public void PlayFlash()
    {
        if (lastFlashFrame == Time.frameCount)
        {
            return;
        }

        lastFlashFrame = Time.frameCount;

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        ClearFlash();
        flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        CacheRendererSnapshots();

        SetFlash(true);
        yield return new WaitForSeconds(flashDuration);

        ClearFlash();
        flashRoutine = null;
    }

    private void CacheRendererSnapshots()
    {
        rendererSnapshots.Clear();
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer targetRenderer in renderers)
        {
            if (targetRenderer == null || ShouldIgnoreRenderer(targetRenderer))
            {
                continue;
            }

            Material[] sharedMaterials = targetRenderer.sharedMaterials;
            for (int i = 0; i < sharedMaterials.Length; i++)
            {
                Material sharedMaterial = sharedMaterials[i];
                if (sharedMaterial == null || !MaterialSupportsFlash(sharedMaterial))
                {
                    continue;
                }

                MaterialPropertyBlock snapshotBlock = new MaterialPropertyBlock();
                targetRenderer.GetPropertyBlock(snapshotBlock, i);
                rendererSnapshots.Add(new RendererSnapshot
                {
                    Renderer = targetRenderer,
                    MaterialIndex = i,
                    PropertyBlock = snapshotBlock
                });
            }
        }
    }

    private void SetFlash(bool enabled)
    {
        foreach (RendererSnapshot snapshot in rendererSnapshots)
        {
            Renderer targetRenderer = snapshot.Renderer;
            if (targetRenderer == null)
            {
                continue;
            }

            if (!enabled)
            {
                targetRenderer.SetPropertyBlock(snapshot.PropertyBlock, snapshot.MaterialIndex);
                continue;
            }

            Material sharedMaterial = GetSharedMaterial(targetRenderer, snapshot.MaterialIndex);
            if (sharedMaterial == null)
            {
                continue;
            }

            MaterialPropertyBlock flashBlock = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(flashBlock, snapshot.MaterialIndex);
            SetColorProperties(flashBlock, sharedMaterial, ResolveBaseColorProperties(), flashColor);
            SetColorProperties(flashBlock, sharedMaterial, ResolveEmissionColorProperties(), emissionColor);
            targetRenderer.SetPropertyBlock(flashBlock, snapshot.MaterialIndex);
        }

    }

    private void ClearFlash()
    {
        SetFlash(false);

        rendererSnapshots.Clear();
    }

    private static Material GetSharedMaterial(Renderer targetRenderer, int materialIndex)
    {
        Material[] sharedMaterials = targetRenderer.sharedMaterials;
        return materialIndex >= 0 && materialIndex < sharedMaterials.Length ? sharedMaterials[materialIndex] : null;
    }

    private bool MaterialSupportsFlash(Material material)
    {
        return HasAnyProperty(material, ResolveBaseColorProperties())
            || HasAnyProperty(material, ResolveEmissionColorProperties());
    }

    private static bool HasAnyProperty(Material material, string[] propertyNames)
    {
        for (int i = 0; i < propertyNames.Length; i++)
        {
            if (!string.IsNullOrEmpty(propertyNames[i]) && material.HasProperty(propertyNames[i]))
            {
                return true;
            }
        }

        return false;
    }

    private static void SetColorProperties(
        MaterialPropertyBlock propertyBlock,
        Material material,
        string[] propertyNames,
        Color color)
    {
        for (int i = 0; i < propertyNames.Length; i++)
        {
            string propertyName = propertyNames[i];
            if (!string.IsNullOrEmpty(propertyName) && material.HasProperty(propertyName))
            {
                propertyBlock.SetColor(propertyName, color);
            }
        }
    }

    private string[] ResolveBaseColorProperties()
    {
        return baseColorProperties != null && baseColorProperties.Length > 0
            ? baseColorProperties
            : DefaultBaseColorProperties;
    }

    private string[] ResolveEmissionColorProperties()
    {
        return emissionColorProperties != null && emissionColorProperties.Length > 0
            ? emissionColorProperties
            : DefaultEmissionColorProperties;
    }

    private static bool ShouldIgnoreRenderer(Renderer targetRenderer)
    {
        string objectName = targetRenderer.name;
        return objectName.Contains("Health")
            || objectName.Contains("Weapon")
            || objectName.Contains("Damage Flash");
    }

    private void RemoveLegacyFlashLight()
    {
        Transform legacyLight = transform.Find("Damage Flash Light");
        if (legacyLight == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(legacyLight.gameObject);
        }
        else
        {
            DestroyImmediate(legacyLight.gameObject);
        }
    }
}
