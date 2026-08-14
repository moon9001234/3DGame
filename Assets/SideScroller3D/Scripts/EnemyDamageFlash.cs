using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
// \u6575\u4eba\u53d7\u5230\u50b7\u5bb3\u6642\uff0c\u77ed\u66ab\u628a\u6a21\u578b\u6750\u8cea\u8207\u9ede\u5149\u6e90\u5207\u6210\u53d7\u64ca\u984f\u8272\u3002
public class EnemyDamageFlash : MonoBehaviour
{
    [Header("\u53d7\u50b7\u9583\u5149\u8a2d\u5b9a")]
    [Tooltip("\u6575\u4eba\u53d7\u5230\u50b7\u5bb3\u6642\uff0c\u6a21\u578b\u6750\u8cea\u6703\u77ed\u66ab\u5207\u63db\u6210\u9019\u500b\u984f\u8272\u3002")]
    [SerializeField] private Color flashColor = new Color(1f, 0.04f, 0.02f, 1f);

    [Tooltip("\u6575\u4eba\u53d7\u50b7\u9583\u5149\u6642\u7684\u81ea\u767c\u5149\u984f\u8272\u3002\u6578\u503c\u53ef\u4ee5\u5927\u65bc 1\uff0c\u8b93\u9583\u5149\u66f4\u4eae\u3002")]
    [SerializeField] private Color emissionColor = new Color(3f, 0.05f, 0.02f, 1f);

    [Tooltip("\u53d7\u50b7\u9583\u5149\u7dad\u6301\u7684\u79d2\u6578\u3002\u6578\u503c\u8d8a\u5927\uff0c\u7d05\u5149\u505c\u7559\u8d8a\u4e45\u3002")]
    [SerializeField] private float flashDuration = 0.12f;

    private readonly Dictionary<Material, MaterialSnapshot> materialSnapshots = new Dictionary<Material, MaterialSnapshot>();

    private Health health;
    private Coroutine flashRoutine;
    private int lastHealth;
    private int lastFlashFrame = -1;

    private struct MaterialSnapshot
    {
        public bool HasBaseColor;
        public bool HasColor;
        public bool HasEmission;
        public Color BaseColor;
        public Color Color;
        public Color EmissionColor;
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
        health.Changed += OnHealthChanged;
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
        materialSnapshots.Clear();
        flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        CacheMaterialSnapshots();

        SetFlash(true);
        yield return new WaitForSeconds(flashDuration);

        ClearFlash();
        flashRoutine = null;
    }

    private void CacheMaterialSnapshots()
    {
        materialSnapshots.Clear();
        Renderer[] renderers = GetComponentsInChildren<Renderer>(true);

        foreach (Renderer targetRenderer in renderers)
        {
            foreach (Material material in targetRenderer.materials)
            {
                if (material == null || materialSnapshots.ContainsKey(material))
                {
                    continue;
                }

                MaterialSnapshot snapshot = new MaterialSnapshot
                {
                    HasBaseColor = material.HasProperty("_BaseColor"),
                    HasColor = material.HasProperty("_Color"),
                    HasEmission = material.HasProperty("_EmissionColor")
                };

                if (snapshot.HasBaseColor)
                {
                    snapshot.BaseColor = material.GetColor("_BaseColor");
                }

                if (snapshot.HasColor)
                {
                    snapshot.Color = material.GetColor("_Color");
                }

                if (snapshot.HasEmission)
                {
                    snapshot.EmissionColor = material.GetColor("_EmissionColor");
                }

                materialSnapshots.Add(material, snapshot);
            }
        }
    }

    private void SetFlash(bool enabled)
    {
        foreach (KeyValuePair<Material, MaterialSnapshot> entry in materialSnapshots)
        {
            Material material = entry.Key;
            MaterialSnapshot snapshot = entry.Value;

            if (enabled)
            {
                if (snapshot.HasBaseColor)
                {
                    material.SetColor("_BaseColor", flashColor);
                }

                if (snapshot.HasColor)
                {
                    material.SetColor("_Color", flashColor);
                }

                if (snapshot.HasEmission)
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", emissionColor);
                }
            }
            else
            {
                RestoreMaterial(material, snapshot);
            }
        }

    }

    private void ClearFlash()
    {
        foreach (KeyValuePair<Material, MaterialSnapshot> entry in materialSnapshots)
        {
            RestoreMaterial(entry.Key, entry.Value);
        }

        materialSnapshots.Clear();
    }

    private void RestoreMaterial(Material material, MaterialSnapshot snapshot)
    {
        if (material == null)
        {
            return;
        }

        if (snapshot.HasBaseColor)
        {
            material.SetColor("_BaseColor", snapshot.BaseColor);
        }

        if (snapshot.HasColor)
        {
            material.SetColor("_Color", snapshot.Color);
        }

        if (snapshot.HasEmission)
        {
            material.SetColor("_EmissionColor", snapshot.EmissionColor);
        }
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
