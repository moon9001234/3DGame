using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class CameraAnchorPrototypeInstaller
{
    private const string PrototypeScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";

    [MenuItem("Tools/3D 遊戲工具/套用平台跟隨鏡頭")]
    public static void Apply()
    {
        Scene scene = EditorSceneManager.OpenScene(PrototypeScenePath, OpenSceneMode.Single);

        Camera mainCamera = Camera.main != null
            ? Camera.main
            : Object.FindFirstObjectByType<Camera>();
        Transform player = GameObject.Find("Player")?.transform;

        if (mainCamera == null || player == null)
        {
            Debug.LogWarning("CameraAnchorPrototypeInstaller: 找不到 Main Camera 或 Player。");
            return;
        }

        DestroyIfExists("Camera_Anchor");
        DestroyIfExists("CM_PlayerCamera");
        DestroyIfExists("CM_PlayerCamera_Target");
        RemoveCinemachineComponents(mainCamera.gameObject);
        SetupSideScrollerCamera(mainCamera, player);
        SetupCameraLevelLine(mainCamera, player);
        SetupCornerTurnTrigger(mainCamera);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("CameraAnchorPrototypeInstaller: Prototype 已套用跳板垂直跟隨攝影機。");
    }

    private static void SetupSideScrollerCamera(Camera mainCamera, Transform player)
    {
        SideScrollerCamera cameraFollow = mainCamera.GetComponent<SideScrollerCamera>();
        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<SideScrollerCamera>();
        }

        cameraFollow.enabled = true;

        SerializedObject serializedCamera = new SerializedObject(cameraFollow);
        SetObject(serializedCamera, "target", player);
        SetBool(serializedCamera, "useColliderCenterForVerticalFraming", true);
        SetVector3(serializedCamera, "offset", mainCamera.transform.position - player.position);
        SetFloat(serializedCamera, "followSpeed", 8f);
        SetBool(serializedCamera, "useHorizontalDeadZone", true);
        SetVector2(serializedCamera, "horizontalDeadZoneViewport", new Vector2(0.35f, 0.65f));
        SetBool(serializedCamera, "useCameraZones", false);
        SetBool(serializedCamera, "returnToDefaultYWithoutZone", true);
        SetBool(serializedCamera, "holdPreviousZoneWhenEmpty", true);
        SetBool(serializedCamera, "useSceneCameraPoseOnPlay", true);
        SetBool(serializedCamera, "keepSceneRotation", true);
        SetBool(serializedCamera, "enableOneWayPlatformVerticalFollow", false);
        SetFloat(serializedCamera, "oneWayPlatformGraceSeconds", 0.25f);
        SetBool(serializedCamera, "disableXBoundsAfterCameraTurn", true);
        SetBool(serializedCamera, "mirrorViewportXOnTurn", true);
        SetFloat(serializedCamera, "turnViewportMinX", 0.18f);
        SetFloat(serializedCamera, "turnViewportMaxX", 0.82f);
        SetVector2(serializedCamera, "xBounds", new Vector2(-3f, 55f));
        SetVector2(serializedCamera, "yBounds", new Vector2(-20f, 30f));
        SetVector2(serializedCamera, "zBounds", new Vector2(-3f, 55f));
        serializedCamera.ApplyModifiedProperties();

        EditorUtility.SetDirty(cameraFollow);
    }

    private static void SetupCameraLevelLine(Camera mainCamera, Transform player)
    {
        DestroyIfExists("Camera_Zones");

        GameObject lineObject = GameObject.Find("CameraLevelLine");
        if (lineObject == null)
        {
            lineObject = new GameObject("CameraLevelLine");
            lineObject.transform.position = new Vector3(player.position.x, 6f, player.position.z);
        }

        CameraLevelLine3D levelLine = lineObject.GetComponent<CameraLevelLine3D>();
        if (levelLine == null)
        {
            levelLine = lineObject.AddComponent<CameraLevelLine3D>();
        }

        SerializedObject serializedLine = new SerializedObject(levelLine);
        SetObject(serializedLine, "sideScrollerCamera", mainCamera.GetComponent<SideScrollerCamera>());
        SetObject(serializedLine, "player", player.GetComponent<PlayerMotor3D>());
        SetFloat(serializedLine, "switchHeight", 6f);
        SetInt(serializedLine, "lineCount", 1);
        SetFloat(serializedLine, "lineSpacing", 6f);
        SetFloat(serializedLine, "lowerCameraY", mainCamera.transform.position.y);
        SetFloat(serializedLine, "upperCameraY", mainCamera.transform.position.y + 6f);
        SetBool(serializedLine, "useSceneCameraYAsLowerLevel", true);
        SetFloat(serializedLine, "switchPadding", 0.15f);
        SetInt(serializedLine, "priority", 0);
        SetFloat(serializedLine, "gizmoWidth", 60f);
        SetBool(serializedLine, "killPlayerWhenFallingTooManyLines", true);
        SetInt(serializedLine, "lethalFallLineCount", 3);
        serializedLine.ApplyModifiedProperties();

        EditorUtility.SetDirty(lineObject);
        EditorUtility.SetDirty(levelLine);
    }

    private static void SetupCornerTurnTrigger(Camera mainCamera)
    {
        GameObject turnObject = FindSceneObjectByName("Turn");
        if (turnObject == null)
        {
            return;
        }

        Collider turnCollider = turnObject.GetComponent<Collider>();
        if (turnCollider == null)
        {
            BoxCollider boxCollider = turnObject.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(2f, 3f, 2f);
            turnCollider = boxCollider;
        }

        turnCollider.isTrigger = true;

        CornerTurnTrigger3D turnTrigger = turnObject.GetComponent<CornerTurnTrigger3D>();
        if (turnTrigger == null)
        {
            turnTrigger = turnObject.AddComponent<CornerTurnTrigger3D>();
        }

        SerializedObject serializedTrigger = new SerializedObject(turnTrigger);
        SetEnum(serializedTrigger, "firstTurnDirection", 1);
        SetFloat(serializedTrigger, "turnDegrees", 90f);
        SetFloat(serializedTrigger, "turnDuration", 1.2f);
        SetBool(serializedTrigger, "alternateDirection", false);
        SetBool(serializedTrigger, "turnDirectionFollowsEntrySide", true);
        SetFloat(serializedTrigger, "centerTriggerDistance", 0.15f);
        SetObject(serializedTrigger, "sideScrollerCamera", mainCamera.GetComponent<SideScrollerCamera>());
        SetBool(serializedTrigger, "showTurnDirectionGuide", true);
        SetBool(serializedTrigger, "onlyShowGuideWhenSelected", true);
        SetVector3(serializedTrigger, "guideLocalMovementAxis", Vector3.right);
        SetBool(serializedTrigger, "flipGuideEntryDirection", false);
        SetFloat(serializedTrigger, "guideLineLength", 2.4f);
        serializedTrigger.ApplyModifiedProperties();

        EditorUtility.SetDirty(turnObject);
        EditorUtility.SetDirty(turnTrigger);
    }

    private static GameObject FindSceneObjectByName(string objectName)
    {
        foreach (GameObject candidate in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (candidate == null || candidate.name != objectName)
            {
                continue;
            }

            if (candidate.scene.IsValid() && candidate.scene.path == PrototypeScenePath)
            {
                return candidate;
            }
        }

        return null;
    }

    private static void DestroyIfExists(string objectName)
    {
        GameObject targetObject = GameObject.Find(objectName);
        if (targetObject != null)
        {
            Object.DestroyImmediate(targetObject);
        }
    }

    private static void RemoveCinemachineComponents(GameObject cameraObject)
    {
        Component[] components = cameraObject.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component == null)
            {
                continue;
            }

            string typeName = component.GetType().FullName;
            if (!string.IsNullOrEmpty(typeName) && typeName.StartsWith("Unity.Cinemachine"))
            {
                Object.DestroyImmediate(component);
            }
        }
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

    private static void SetEnum(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.enumValueIndex = value;
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

    private static void SetInt(SerializedObject serializedObject, string propertyName, int value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.intValue = value;
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

    private static void SetVector3(SerializedObject serializedObject, string propertyName, Vector3 value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property != null)
        {
            property.vector3Value = value;
        }
    }
}
