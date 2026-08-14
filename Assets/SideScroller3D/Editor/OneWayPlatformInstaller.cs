using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class OneWayPlatformInstaller
{
    private const string ScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";

    [MenuItem("Tools/3D \u904a\u6232\u5de5\u5177/\u5957\u7528\u55ae\u5411\u5e73\u53f0")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        int groundLayer = LayerMask.NameToLayer("Ground");
        int changedCount = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                if (!child.name.StartsWith("Platform_"))
                {
                    continue;
                }

                Collider platformCollider = child.GetComponent<Collider>();
                if (platformCollider == null)
                {
                    continue;
                }

                if (child.GetComponent<OneWayPlatform3D>() == null)
                {
                    child.gameObject.AddComponent<OneWayPlatform3D>();
                    changedCount++;
                }

                if (groundLayer >= 0 && child.gameObject.layer != groundLayer)
                {
                    child.gameObject.layer = groundLayer;
                    changedCount++;
                }

                EditorUtility.SetDirty(child.gameObject);
            }
        }

        if (changedCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"One way platform settings applied. Changes: {changedCount}");
    }
}
