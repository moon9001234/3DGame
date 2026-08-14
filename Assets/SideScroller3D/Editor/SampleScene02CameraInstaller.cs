using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SampleScene02CameraInstaller
{
    private const string ScenePath = "Assets/Scenes/SampleScene_02.unity";
    private const string OccluderLayerName = "BuildingOccluder";
    private const int PreferredOccluderLayer = 11;
    private static readonly string[] OcclusionExcludedNames =
    {
        "Road",
        "Sewer",
        "Hatch",
        "Ground",
        "Floor",
        "Sidewalk",
        "Pavement",
        "Trash",
        "Sign",
        "Lantern"
    };

    [MenuItem("Tools/3D 遊戲工具/設定 SampleScene 02 鏡頭")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Camera camera = FindSceneCamera();
        Transform player = FindPlayer();

        if (camera == null || player == null)
        {
            Debug.LogWarning("SampleScene02CameraInstaller: Missing Camera or Player in SampleScene_02.");
            return;
        }

        int occluderLayer = EnsureLayer(OccluderLayerName, PreferredOccluderLayer);
        Transform housesRoot = FindRoot("Houses");
        Transform streetPropsRoot = FindRoot("StreetProps");
        ConfigureCamera(camera, player, occluderLayer, housesRoot, streetPropsRoot);
        AssignOccluderLayer("Houses", occluderLayer);
        AssignOccluderLayer("StreetProps", occluderLayer);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("SampleScene02CameraInstaller: SampleScene_02 camera configured.");
    }

    private static Camera FindSceneCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            return mainCamera;
        }

        GameObject namedCamera = GameObject.Find("Camera");
        if (namedCamera != null && namedCamera.TryGetComponent(out Camera camera))
        {
            return camera;
        }

        return Object.FindFirstObjectByType<Camera>();
    }

    private static Transform FindPlayer()
    {
        GameObject namedPlayer = GameObject.Find("Player");
        if (namedPlayer != null)
        {
            return namedPlayer.transform;
        }

        PlayerMotor3D playerMotor = Object.FindFirstObjectByType<PlayerMotor3D>();
        return playerMotor != null ? playerMotor.transform : null;
    }

    private static Transform FindRoot(string rootName)
    {
        GameObject root = GameObject.Find(rootName);
        return root != null ? root.transform : null;
    }

    private static void ConfigureCamera(
        Camera camera,
        Transform player,
        int occluderLayer,
        Transform housesRoot,
        Transform streetPropsRoot)
    {
        camera.gameObject.name = "Main Camera";
        camera.gameObject.tag = "MainCamera";
        camera.nearClipPlane = 0.1f;

        SideScrollerCamera sideScrollerCamera = camera.GetComponent<SideScrollerCamera>();
        if (sideScrollerCamera == null)
        {
            sideScrollerCamera = camera.gameObject.AddComponent<SideScrollerCamera>();
        }

        SerializedObject serializedCamera = new SerializedObject(sideScrollerCamera);
        SetObject(serializedCamera, "target", player);
        SetBool(serializedCamera, "useSceneCameraPoseOnPlay", true);
        SetBool(serializedCamera, "keepSceneRotation", true);
        SetFloat(serializedCamera, "followSpeed", 8f);
        SetBool(serializedCamera, "useHorizontalDeadZone", true);
        SetVector2(serializedCamera, "horizontalDeadZoneViewport", new Vector2(0.4f, 0.55f));
        SetVector2(serializedCamera, "xBounds", new Vector2(-20f, 80f));
        SetVector2(serializedCamera, "yBounds", new Vector2(-10f, 40f));
        SetVector2(serializedCamera, "zBounds", new Vector2(-20f, 40f));
        serializedCamera.ApplyModifiedProperties();

        CameraOcclusionHider occlusionHider = camera.GetComponent<CameraOcclusionHider>();
        if (occlusionHider == null)
        {
            occlusionHider = camera.gameObject.AddComponent<CameraOcclusionHider>();
        }

        SerializedObject serializedOcclusion = new SerializedObject(occlusionHider);
        SetObject(serializedOcclusion, "target", player);
        SetLayerMask(serializedOcclusion, "occluderMask", 1 << occluderLayer);
        SetTransformArray(serializedOcclusion, "occluderRoots", housesRoot, streetPropsRoot);
        SetStringArray(serializedOcclusion, "excludedNameKeywords", OcclusionExcludedNames);
        serializedOcclusion.ApplyModifiedProperties();

        EditorUtility.SetDirty(camera);
        EditorUtility.SetDirty(sideScrollerCamera);
        EditorUtility.SetDirty(occlusionHider);
    }

    private static void AssignOccluderLayer(string rootName, int layer)
    {
        GameObject root = GameObject.Find(rootName);
        if (root == null)
        {
            Debug.LogWarning($"SampleScene02CameraInstaller: Could not find {rootName}; occluder layer not assigned.");
            return;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (IsExcludedByName(children[i]))
            {
                continue;
            }

            children[i].gameObject.layer = layer;
            EditorUtility.SetDirty(children[i].gameObject);
        }
    }

    private static bool IsExcludedByName(Transform candidate)
    {
        Transform current = candidate;
        while (current != null)
        {
            for (int i = 0; i < OcclusionExcludedNames.Length; i++)
            {
                string keyword = OcclusionExcludedNames[i];
                if (!string.IsNullOrEmpty(keyword)
                    && current.name.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            current = current.parent;
        }

        return false;
    }

    private static int EnsureLayer(string layerName, int preferredLayer)
    {
        int existingLayer = LayerMask.NameToLayer(layerName);
        if (existingLayer >= 0)
        {
            return existingLayer;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        int targetLayer = string.IsNullOrEmpty(layers.GetArrayElementAtIndex(preferredLayer).stringValue)
            ? preferredLayer
            : FindEmptyLayer(layers);

        if (targetLayer < 0)
        {
            Debug.LogWarning("SampleScene02CameraInstaller: No empty layer slot found; using Default layer for occlusion.");
            return 0;
        }

        layers.GetArrayElementAtIndex(targetLayer).stringValue = layerName;
        tagManager.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
        return targetLayer;
    }

    private static int FindEmptyLayer(SerializedProperty layers)
    {
        for (int i = 8; i < layers.arraySize; i++)
        {
            if (string.IsNullOrEmpty(layers.GetArrayElementAtIndex(i).stringValue))
            {
                return i;
            }
        }

        return -1;
    }

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
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

    private static void SetLayerMask(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.intValue = value;
        }
    }

    private static void SetTransformArray(SerializedObject serializedObject, string propertyName, params Transform[] values)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            int validValueCount = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    validValueCount++;
                }
            }

            property.arraySize = validValueCount;
            int propertyIndex = 0;
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] != null)
                {
                    property.GetArrayElementAtIndex(propertyIndex).objectReferenceValue = values[i];
                    propertyIndex++;
                }
            }
        }
    }

    private static void SetStringArray(SerializedObject serializedObject, string propertyName, string[] values)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).stringValue = values[i];
            }
        }
    }
}
