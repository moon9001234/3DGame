using System;
using UnityEngine;

[Serializable]
public class PlayerHitSoundRule
{
    [SerializeField] private string targetNameContains;
    [SerializeField] private string targetTag;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private AudioClip hitSound;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    public AudioClip HitSound => hitSound;
    public float Volume => Mathf.Clamp01(volume);

    public bool Matches(Collider targetCollider, Transform targetRoot)
    {
        bool hasNameFilter = !string.IsNullOrWhiteSpace(targetNameContains);
        bool hasTagFilter = !string.IsNullOrWhiteSpace(targetTag);
        bool hasLayerFilter = targetLayers.value != 0;

        if (!hasNameFilter && !hasTagFilter && !hasLayerFilter)
        {
            return false;
        }

        if (hasNameFilter && !MatchesName(targetCollider, targetRoot))
        {
            return false;
        }

        if (hasTagFilter && !MatchesTag(targetCollider, targetRoot))
        {
            return false;
        }

        if (hasLayerFilter && !MatchesLayer(targetCollider, targetRoot))
        {
            return false;
        }

        return true;
    }

    private bool MatchesName(Collider targetCollider, Transform targetRoot)
    {
        if (NameContains(targetRoot))
        {
            return true;
        }

        Transform current = targetCollider != null ? targetCollider.transform : null;
        while (current != null)
        {
            if (NameContains(current))
            {
                return true;
            }

            if (current == targetRoot)
            {
                break;
            }

            current = current.parent;
        }

        return false;
    }

    private bool NameContains(Transform candidate)
    {
        return candidate != null
            && candidate.name.IndexOf(targetNameContains, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private bool MatchesTag(Collider targetCollider, Transform targetRoot)
    {
        return HasTag(targetCollider != null ? targetCollider.gameObject : null)
            || HasTag(targetRoot != null ? targetRoot.gameObject : null);
    }

    private bool HasTag(GameObject candidate)
    {
        return candidate != null && string.Equals(candidate.tag, targetTag, StringComparison.Ordinal);
    }

    private bool MatchesLayer(Collider targetCollider, Transform targetRoot)
    {
        return HasLayer(targetCollider != null ? targetCollider.gameObject : null)
            || HasLayer(targetRoot != null ? targetRoot.gameObject : null);
    }

    private bool HasLayer(GameObject candidate)
    {
        return candidate != null && (targetLayers.value & (1 << candidate.layer)) != 0;
    }
}
