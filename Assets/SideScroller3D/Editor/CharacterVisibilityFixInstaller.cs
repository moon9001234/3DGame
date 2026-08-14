using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class CharacterVisibilityFixInstaller
{
    private static readonly string[] PrefabPaths =
    {
        "Assets/SideScroller3D/Prefabs/Player_Model.prefab",
        "Assets/SideScroller3D/Prefabs/Enemy_Model.prefab",
        "Assets/SideScroller3D/Prefabs/Ranged_Enemy_Model.prefab"
    };

    private const string ScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";

    public static void Install()
    {
        foreach (string path in PrefabPaths)
        {
            InstallOnPrefab(path);
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath);
        foreach (SkinnedMeshRenderer renderer in Object.FindObjectsByType<SkinnedMeshRenderer>(FindObjectsSortMode.None))
        {
            ApplyRendererFix(renderer);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void InstallOnPrefab(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Character visibility fix skipped missing prefab: {path}");
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(path);
        if (root.GetComponent<CharacterSkinnedMeshVisibilityFix>() == null)
        {
            root.AddComponent<CharacterSkinnedMeshVisibilityFix>();
        }

        foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            ApplyRendererFix(renderer);
        }

        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
        Debug.Log($"Character visibility fix installed: {path}");
    }

    private static void ApplyRendererFix(SkinnedMeshRenderer renderer)
    {
        renderer.updateWhenOffscreen = true;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;

        Bounds bounds = renderer.localBounds;
        Vector3 size = bounds.size;
        size.x = Mathf.Max(size.x, 4f);
        size.y = Mathf.Max(size.y, 4f);
        size.z = Mathf.Max(size.z, 4f);
        renderer.localBounds = new Bounds(bounds.center, size);
        EditorUtility.SetDirty(renderer);
    }
}
