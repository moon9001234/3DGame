using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CameraVerticalZoneInstaller
{
    private const string PrototypeScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";

    [MenuItem("Tools/3D 遊戲工具/套用鏡頭垂直區域")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(PrototypeScenePath, OpenSceneMode.Single);
        SideScrollerCamera cameraFollow = Object.FindFirstObjectByType<SideScrollerCamera>();

        if (cameraFollow == null)
        {
            Debug.LogWarning("CameraVerticalZoneInstaller: Prototype 場景中找不到 SideScrollerCamera。");
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

        Debug.Log("CameraVerticalZoneInstaller: Prototype 攝影機垂直死區設定已套用。");
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
