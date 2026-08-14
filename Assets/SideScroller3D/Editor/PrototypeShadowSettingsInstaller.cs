using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrototypeShadowSettingsInstaller
{
    private const string ScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";

    [MenuItem("Tools/3D \u904a\u6232\u5de5\u5177/\u5957\u7528\u539f\u578b\u9670\u5f71\u8a2d\u5b9a")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Light directionalLight = FindDirectionalLight();
        if (directionalLight == null)
        {
            Debug.LogWarning("Prototype shadow settings skipped: Directional Light not found.");
            return;
        }

        directionalLight.shadows = LightShadows.Soft;
        directionalLight.shadowStrength = 1f;
        directionalLight.shadowBias = 0.05f;
        directionalLight.shadowNormalBias = 0.2f;
        directionalLight.shadowNearPlane = 0.1f;
        directionalLight.intensity = Mathf.Max(directionalLight.intensity, 2f);

        EditorUtility.SetDirty(directionalLight);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Prototype shadow settings applied.");
    }

    private static Light FindDirectionalLight()
    {
        Light[] lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                return light;
            }
        }

        return null;
    }
}
