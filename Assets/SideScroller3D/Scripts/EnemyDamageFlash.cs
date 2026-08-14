using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Health))]
// 敵人受到傷害時，短暫把模型材質與點光源切成受擊顏色。
public class EnemyDamageFlash : MonoBehaviour
{
    [Header("受傷閃光設定")]
    [Tooltip("敵人受到傷害時，模型材質會短暫切換成這個顏色。")]
    [SerializeField] private Color flashColor = new Color(1f, 0.04f, 0.02f, 1f);

    [Tooltip("敵人受傷閃光時的自發光顏色。數值可以大於 1，讓閃光更亮。")]
    [SerializeField] private Color emissionColor = new Color(3f, 0.05f, 0.02f, 1f);

    [Tooltip("受傷閃光維持的秒數。數值越大，紅光停留越久。")]
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
