using UnityEngine;

[DisallowMultipleComponent]
public class DamageHitEffect3D : MonoBehaviour
{
    [Header("受擊特效")]
    [Tooltip("受到傷害時播放的特效 Prefab，例如 FX_Hit。沒有指定時不播放特效。")]
    [SerializeField] private GameObject effectPrefab;

    [Tooltip("特效生成與播放的位置，例如 Enemy_Model 底下的 EF_Hit。")]
    [SerializeField] private Transform effectAnchor;

    [Tooltip("沒有手動指定 Effect Anchor 時，會用這個名稱在子物件中尋找定位點。")]
    [SerializeField] private string effectAnchorName = "EF_Hit";

    [Tooltip("Stop and hide the effect at startup so Play On Awake particles do not appear when the scene starts.")]
    [SerializeField] private bool stopEffectOnAwake = true;

    private GameObject prefabInstance;

    private void Awake()
    {
        EnsureEffect();
        StopEffectOnAwakeIfNeeded();
    }

    public void PlayEffect()
    {
        EnsureEffect();

        if (prefabInstance == null)
        {
            return;
        }

        MoveEffectToAnchor(prefabInstance.transform);
        prefabInstance.SetActive(true);

        ParticleSystem[] particleSystems = prefabInstance.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            particleSystem.gameObject.SetActive(true);
            particleSystem.Clear(true);
            particleSystem.Play(true);
        }
    }

    public void SetEffectAnchor(Transform anchor)
    {
        effectAnchor = anchor;

        if (prefabInstance != null)
        {
            MoveEffectToAnchor(prefabInstance.transform);
        }
    }

    public void SetEffectPrefab(GameObject prefab)
    {
        if (effectPrefab != prefab)
        {
            effectPrefab = prefab;
            prefabInstance = null;
        }

        ClearGeneratedEffectObjects();
        EnsureEffect();
    }

    private void EnsureEffect()
    {
        if (effectPrefab == null)
        {
            ClearGeneratedEffectObjects();
            return;
        }

        EnsurePrefabEffect();
    }

    private void EnsurePrefabEffect()
    {
        ClearGeneratedEffectObjects();
        Transform parent = ResolveEffectParent();

        if (prefabInstance == null)
        {
            Transform existing = parent.Find(effectPrefab.name);
            if (existing == null && parent != transform)
            {
                existing = transform.Find(effectPrefab.name);
            }

            if (existing == null)
            {
                existing = FindChildByName(parent, effectPrefab.name);
            }

            if (existing != null)
            {
                prefabInstance = existing.gameObject;
            }
        }

        if (prefabInstance == null)
        {
            prefabInstance = Instantiate(effectPrefab, parent);
            prefabInstance.name = effectPrefab.name;
        }

        MoveEffectToAnchor(prefabInstance.transform);
        PrepareEffectInstance(prefabInstance);
    }

    private void PrepareEffectInstance(GameObject effect)
    {
        if (effect == null)
        {
            return;
        }

        ParticleSystem[] particleSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem == null)
            {
                continue;
            }

            ParticleSystem.MainModule main = particleSystem.main;
            main.playOnAwake = false;
            if (main.stopAction == ParticleSystemStopAction.Destroy)
            {
                main.stopAction = ParticleSystemStopAction.None;
            }
        }
    }

    private void StopEffectOnAwakeIfNeeded()
    {
        if (!stopEffectOnAwake || prefabInstance == null)
        {
            return;
        }

        ParticleSystem[] particleSystems = prefabInstance.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem != null)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        prefabInstance.SetActive(false);
    }

    private void ClearGeneratedEffectObjects()
    {
        Transform generated = transform.Find("Damage_Hit_Effect");
        if (generated != null)
        {
            DestroyEffectObject(generated.gameObject);
        }
    }

    private void DestroyEffectObject(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (prefabInstance == target)
        {
            prefabInstance = null;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    private void MoveEffectToAnchor(Transform effect)
    {
        Transform parent = ResolveEffectParent();
        if (effect.parent != parent)
        {
            effect.SetParent(parent, false);
        }

        effect.localPosition = Vector3.zero;
        effect.localRotation = Quaternion.identity;
    }

    private Transform ResolveEffectParent()
    {
        if (effectAnchor == null && !string.IsNullOrEmpty(effectAnchorName))
        {
            effectAnchor = FindChildByName(transform, effectAnchorName);
        }

        return effectAnchor != null ? effectAnchor : transform;
    }

    private static Transform FindChildByName(Transform root, string childName)
    {
        foreach (Transform child in root)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform match = FindChildByName(child, childName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
