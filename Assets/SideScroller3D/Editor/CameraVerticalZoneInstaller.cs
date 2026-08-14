using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CameraVerticalZoneInstaller
{
    private const string PrototypeScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";

    [MenuItem("Tools/3D \u904a\u6232\u5de5\u5177/\u5957\u7528\u93e1\u982d\u5782\u76f4\u5340\u57df")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(PrototypeScenePath, OpenSceneMode.Single);
        SideScrollerCamera cameraFollow = Object.FindFirstObjectByType<SideScrollerCamera>();

        if (cameraFollow == null)
        {
            Debug.LogWarning("CameraVerticalZoneInstaller: Prototype \u5834\u666f\u4e2d\u627e\u4e0d\u5230 SideScrollerCamera\u3002");
            return;
        }

        SerializedObject serializedCamera = new SerializedObject(cameraFollow);
        SetBool(serializedCamera, "useVerticalScreenZone", true);
        SetBool(serializedCamera, "useColliderCenterForVerticalFraming", true);
        SetFloat(serializedCamera, "verticalLowerTriggerViewportY", 0f);
        SetFloat(serializedCamera, "verticalTriggerViewportY", 0.5f);
        SetFloat(serializedCamera, "verticalTargetViewportY", 0.25f);
        SetBool(serializedCamera, "keepInitialYAsMinimum", false);
        SetBool(serializedCamera, "followVerticalMovement", false);
        SetVector2(serializedCamera, "yBounds", new Vector2(-20f, 30f));
        serializedCamera.ApplyModifiedProperties();

        EditorUtility.SetDirty(cameraFollow);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CameraVerticalZoneInstaller: Prototype \u651d\u5f71\u6a5f\u5782\u76f4\u6b7b\u5340\u8a2d\u5b9a\u5df2\u5957\u7528\u3002");
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetFloat(SerializedObject serializedObject, string propertyName, float value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetVector2(SerializedObject serializedObject, string propertyName, Vector2 value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.vector2Value = value;
        }
    }
}
