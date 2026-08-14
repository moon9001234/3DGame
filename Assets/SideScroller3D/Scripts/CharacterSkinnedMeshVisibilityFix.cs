using UnityEngine;
using UnityEngine.Rendering;

public class CharacterSkinnedMeshVisibilityFix : MonoBehaviour
{
    [Header("模型顯示修正")]
    [Tooltip("開啟後，即使角色模型暫時離開攝影機視野，Skinned Mesh 仍會持續更新，避免重新進入畫面時消失。")]
    [SerializeField] private bool updateWhenOffscreen = true;

    [Tooltip("角色模型的最小本地 Bounds 大小。數值越大，越不容易因動畫或裁切造成模型消失。")]
    [SerializeField] private Vector3 minimumLocalBoundsSize = new Vector3(4f, 4f, 4f);

    [Tooltip("開啟後，角色模型會投射陰影。")]
    [SerializeField] private bool castShadows = true;

    [Tooltip("開啟後，角色模型會接收場景陰影。")]
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
