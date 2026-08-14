using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DigitalRainSkyboxInstaller
{
    private const string MaterialPath = "Assets/SideScroller3D/Materials/DigitalRainSkybox.mat";

    [MenuItem("SideScroller3D/Skybox/Apply Digital Rain Skybox To Current Scene")]
    public static void ApplyToCurrentScene()
    {
        Material skyboxMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (skyboxMaterial == null)
        {
            Debug.LogError($"Digital rain skybox material not found at {MaterialPath}.");
            return;
        }

        RenderSettings.skybox = skyboxMaterial;
        DynamicGI.UpdateEnvironment();

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }

        Debug.Log($"Applied {skyboxMaterial.name} to the current scene skybox.");
    }

    [MenuItem("SideScroller3D/Skybox/Select Digital Rain Skybox Material")]
    public static void SelectMaterial()
    {
        Material skyboxMaterial = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (skyboxMaterial == null)
        {
            Debug.LogError($"Digital rain skybox material not found at {MaterialPath}.");
            return;
        }

        Selection.activeObject = skyboxMaterial;
        EditorGUIUtility.PingObject(skyboxMaterial);
    }
}
