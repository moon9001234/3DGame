using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerMotor3D))]
[CanEditMultipleObjects]
public class PlayerMotor3DEditor : Editor
{
    private const string OwnerTypeName = "PlayerMotor3D";
    private const string OtherSectionKey = "other";

    private static readonly Dictionary<string, string> ShortLabels = new Dictionary<string, string>
    {
        { "movementMode", "模式" },
        { "moveSpeed", "速度" },
        { "airControl", "空中控制" },
        { "useCameraRelativeMovement", "依相機方向移動" },
        { "movementCamera", "移動相機" },
        { "freeTurnSpeed", "轉向速度" },
        { "lockedZ", "鎖定 Z" },
        { "movementAxis", "移動軸" },
        { "jumpForce", "跳躍力" },
        { "upwardGravityMultiplier", "上升重力" },
        { "fallGravityMultiplier", "下落重力" },
        { "maxFallSpeed", "最大下落速度" },
        { "jumpBufferSeconds", "跳躍緩衝" },
        { "coyoteTimeSeconds", "離地寬容" },
        { "extraAirJumps", "空中跳躍次數" },
        { "airJumpForceMultiplier", "空中跳躍倍率" },
        { "dashDistance", "距離" },
        { "dashDuration", "時間" },
        { "dashCooldown", "冷卻" },
        { "allowAirDash", "允許空中衝刺" },
        { "flattenVerticalVelocityDuringDash", "衝刺壓平垂直速度" },
        { "dashJumpHorizontalMultiplier", "衝刺跳水平倍率" },
        { "dashJumpDashSpeedCarryMultiplier", "衝刺跳速度保留" },
        { "dashJumpBoostSeconds", "衝刺跳加成時間" },
        { "airDashAnimationMinSeconds", "空中衝刺動畫保留" },
        { "dashEndAnimationMinSeconds", "結束動畫保留" },
        { "enableDashAfterimage", "啟用" },
        { "dashAfterimageVisualRoot", "視覺根物件" },
        { "dashAfterimageSpawnInterval", "產生間隔" },
        { "dashAfterimageLifetime", "淡出時間" },
        { "dashAfterimageColor", "顏色" },
        { "dashAfterimageIncludeMeshRenderers", "包含 Mesh Renderer" },
        { "dashAfterimageIncludeInactiveRenderers", "包含未啟用 Renderer" },
        { "actionAnimationCrossFadeSeconds", "動作淡入" },
        { "jumpAnimationCrossFadeSeconds", "跳躍淡入" },
        { "groundCheck", "定位點" },
        { "groundCheckLocalOffset", "本地偏移" },
        { "groundCheckRadius", "半徑" },
        { "groundMask", "地面圖層" },
        { "groundFallbackDistance", "備用距離" },
        { "useAnySolidGroundFallback", "允許實體地面備用" },
        { "enableOneWayPlatforms", "啟用" },
        { "dropThroughSeconds", "下穿時間" },
        { "dropThroughStartSpeed", "下穿起始速度" },
        { "dropThroughPlatformSearchDistance", "跳板搜尋距離" },
        { "dropInputThreshold", "下方向門檻" },
        { "oneWayPlatformPrecheckHeight", "預先偵測高度" },
        { "oneWayPlatformPrecheckPadding", "預先偵測外擴" },
        { "preventAirWallSticking", "防止空中貼牆" },
        { "wallNormalThreshold", "牆面法線門檻" },
        { "wallContactGraceSeconds", "離牆保留時間" },
        { "useNoFrictionColliderMaterial", "使用無摩擦材質" },
        { "enableDamageKnockback", "啟用" },
        { "knockbackForce", "彈飛力" },
        { "knockbackControlLockSeconds", "操作鎖定時間" },
        { "lockControlUntilKnockbackLands", "鎖定直到落地" }
    };

    private static readonly InspectorSection[] Sections =
    {
        new InspectorSection("movement", "移動設定", true, new[]
        {
            "movementMode",
            "moveSpeed",
            "airControl",
            "useCameraRelativeMovement",
            "movementCamera",
            "freeTurnSpeed"
        }),
        new InspectorSection("sideScroller", "橫向卷軸限制", false, new[]
        {
            "lockedZ",
            "movementAxis"
        }),
        new InspectorSection("jump", "跳躍設定", true, new[]
        {
            "jumpForce",
            "upwardGravityMultiplier",
            "fallGravityMultiplier",
            "maxFallSpeed",
            "jumpBufferSeconds",
            "coyoteTimeSeconds",
            "extraAirJumps",
            "airJumpForceMultiplier"
        }),
        new InspectorSection("dash", "衝刺設定", true, new[]
        {
            "dashDistance",
            "dashDuration",
            "dashCooldown",
            "allowAirDash",
            "flattenVerticalVelocityDuringDash",
            "dashJumpHorizontalMultiplier",
            "dashJumpDashSpeedCarryMultiplier",
            "dashJumpBoostSeconds",
            "airDashAnimationMinSeconds",
            "dashEndAnimationMinSeconds"
        }),
        new InspectorSection("dashAfterimage", "衝刺殘影", false, new[]
        {
            "enableDashAfterimage",
            "dashAfterimageVisualRoot",
            "dashAfterimageSpawnInterval",
            "dashAfterimageLifetime",
            "dashAfterimageColor",
            "dashAfterimageIncludeMeshRenderers",
            "dashAfterimageIncludeInactiveRenderers"
        }),
        new InspectorSection("animation", "動畫設定", true, new[]
        {
            "actionAnimationCrossFadeSeconds",
            "jumpAnimationCrossFadeSeconds"
        }),
        new InspectorSection("ground", "地面偵測", false, new[]
        {
            "groundCheck",
            "groundCheckLocalOffset",
            "groundCheckRadius",
            "groundMask",
            "groundFallbackDistance",
            "useAnySolidGroundFallback"
        }),
        new InspectorSection("oneWayPlatform", "單向跳板", false, new[]
        {
            "enableOneWayPlatforms",
            "dropThroughSeconds",
            "dropThroughStartSpeed",
            "dropThroughPlatformSearchDistance",
            "dropInputThreshold",
            "oneWayPlatformPrecheckHeight",
            "oneWayPlatformPrecheckPadding"
        }),
        new InspectorSection("wall", "牆面滑落", false, new[]
        {
            "preventAirWallSticking",
            "wallNormalThreshold",
            "wallContactGraceSeconds",
            "useNoFrictionColliderMaterial"
        }),
        new InspectorSection("damageKnockback", "受傷彈飛", false, new[]
        {
            "enableDamageKnockback",
            "knockbackForce",
            "knockbackControlLockSeconds",
            "lockControlUntilKnockbackLands"
        })
    };

    private void OnEnable()
    {
        SideScrollerInspectorLabels.ReloadIfNeeded();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawScriptField();

        HashSet<string> drawnProperties = new HashSet<string>();
        for (int i = 0; i < Sections.Length; i++)
        {
            DrawSection(Sections[i], drawnProperties);
        }

        DrawRemainingProperties(drawnProperties);
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSection(InspectorSection section, HashSet<string> drawnProperties)
    {
        string prefsKey = GetPrefsKey(section.key);
        bool expanded = SessionState.GetBool(prefsKey, section.defaultExpanded);

        EditorGUILayout.Space(4f);
        expanded = EditorGUILayout.Foldout(expanded, section.title, true, EditorStyles.foldoutHeader);
        SessionState.SetBool(prefsKey, expanded);
        MarkSectionPropertiesAsDrawn(section, drawnProperties);

        if (!expanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        for (int i = 0; i < section.propertyNames.Length; i++)
        {
            string propertyName = section.propertyNames[i];
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                continue;
            }

            DrawProperty(property);
        }

        EditorGUI.indentLevel--;
    }

    private void MarkSectionPropertiesAsDrawn(InspectorSection section, HashSet<string> drawnProperties)
    {
        for (int i = 0; i < section.propertyNames.Length; i++)
        {
            SerializedProperty property = serializedObject.FindProperty(section.propertyNames[i]);
            if (property != null)
            {
                drawnProperties.Add(property.propertyPath);
            }
        }
    }

    private void DrawRemainingProperties(HashSet<string> drawnProperties)
    {
        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        List<string> remainingPropertyPaths = new List<string>();

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (property.propertyPath == "m_Script" || drawnProperties.Contains(property.propertyPath))
            {
                continue;
            }

            remainingPropertyPaths.Add(property.propertyPath);
        }

        if (remainingPropertyPaths.Count == 0)
        {
            return;
        }

        string prefsKey = GetPrefsKey(OtherSectionKey);
        bool expanded = SessionState.GetBool(prefsKey, false);

        EditorGUILayout.Space(4f);
        expanded = EditorGUILayout.Foldout(expanded, "其他", true, EditorStyles.foldoutHeader);
        SessionState.SetBool(prefsKey, expanded);

        if (!expanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        for (int i = 0; i < remainingPropertyPaths.Count; i++)
        {
            property = serializedObject.FindProperty(remainingPropertyPaths[i]);
            if (property == null)
            {
                continue;
            }

            DrawProperty(property);
        }

        EditorGUI.indentLevel--;
    }

    private void DrawProperty(SerializedProperty property)
    {
        if (ShortLabels.TryGetValue(property.name, out string shortLabel))
        {
            EditorGUILayout.PropertyField(property, new GUIContent(shortLabel, GetTooltip(property)), true);
            return;
        }

        GUIContent label = SideScrollerInspectorLabels.Content(
            OwnerTypeName,
            property.name,
            ObjectNames.NicifyVariableName(property.name));
        EditorGUILayout.PropertyField(property, label, true);
    }

    private string GetTooltip(SerializedProperty property)
    {
        return SideScrollerInspectorLabels.Content(
            OwnerTypeName,
            property.name,
            ObjectNames.NicifyVariableName(property.name)).tooltip;
    }

    private void DrawScriptField()
    {
        SerializedProperty scriptProperty = serializedObject.FindProperty("m_Script");
        if (scriptProperty == null)
        {
            return;
        }

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(scriptProperty);
        }
    }

    private static string GetPrefsKey(string sectionKey)
    {
        return "SideScroller3D.PlayerMotor3DEditor." + sectionKey;
    }

    private class InspectorSection
    {
        public readonly string key;
        public readonly string title;
        public readonly bool defaultExpanded;
        public readonly string[] propertyNames;

        public InspectorSection(string key, string title, bool defaultExpanded, string[] propertyNames)
        {
            this.key = key;
            this.title = title;
            this.defaultExpanded = defaultExpanded;
            this.propertyNames = propertyNames;
        }
    }
}
