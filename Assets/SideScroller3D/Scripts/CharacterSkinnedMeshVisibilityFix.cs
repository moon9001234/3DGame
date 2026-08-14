using UnityEngine;
using UnityEngine.Rendering;

public class CharacterSkinnedMeshVisibilityFix : MonoBehaviour
{
    [Header("\u6a21\u578b\u986f\u793a\u4fee\u6b63")]
    [Tooltip("\u958b\u555f\u5f8c\uff0c\u5373\u4f7f\u89d2\u8272\u6a21\u578b\u66ab\u6642\u96e2\u958b\u651d\u5f71\u6a5f\u8996\u91ce\uff0cSkinned Mesh \u4ecd\u6703\u6301\u7e8c\u66f4\u65b0\uff0c\u907f\u514d\u91cd\u65b0\u9032\u5165\u756b\u9762\u6642\u6d88\u5931\u3002")]
    [SerializeField] private bool updateWhenOffscreen = true;

    [Tooltip("\u89d2\u8272\u6a21\u578b\u7684\u6700\u5c0f\u672c\u5730 Bounds \u5927\u5c0f\u3002\u6578\u503c\u8d8a\u5927\uff0c\u8d8a\u4e0d\u5bb9\u6613\u56e0\u52d5\u756b\u6216\u88c1\u5207\u9020\u6210\u6a21\u578b\u6d88\u5931\u3002")]
    [SerializeField] private Vector3 minimumLocalBoundsSize = new Vector3(4f, 4f, 4f);

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u89d2\u8272\u6a21\u578b\u6703\u6295\u5c04\u9670\u5f71\u3002")]
    [SerializeField] private bool castShadows = true;

    [Tooltip("\u958b\u555f\u5f8c\uff0c\u89d2\u8272\u6a21\u578b\u6703\u63a5\u6536\u5834\u666f\u9670\u5f71\u3002")]
    [SerializeField] private bool receiveShadows = true;

    private void Awake()
    {
        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    private void Apply()
    {
        foreach (SkinnedMeshRenderer renderer in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            renderer.updateWhenOffscreen = updateWhenOffscreen;

            Bounds bounds = renderer.localBounds;
            Vector3 size = bounds.size;
            size.x = Mathf.Max(size.x, minimumLocalBoundsSize.x);
            size.y = Mathf.Max(size.y, minimumLocalBoundsSize.y);
            size.z = Mathf.Max(size.z, minimumLocalBoundsSize.z);
            renderer.localBounds = new Bounds(bounds.center, size);
            renderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = receiveShadows;
        }
    }
}
