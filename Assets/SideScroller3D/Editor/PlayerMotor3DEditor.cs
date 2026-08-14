using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerMotor3D))]
[CanEditMultipleObjects]
public class PlayerMotor3DEditor : Editor
{
    private const string OwnerTypeName = "PlayerMotor3D";
    private const string OtherSectionKey = "other";

    private static readonly HashSet<string> ShortLabelKeys = new HashSet<string>
    {
        "movementMode",
        "moveSpeed",
        "airControl",
        "useCameraRelativeMovement",
        "movementCamera",
        "freeTurnSpeed",
        "lockedZ",
        "movementAxis",
        "jumpForce",
        "upwardGravityMultiplier",
        "fallGravityMultiplier",
        "maxFallSpeed",
        "jumpBufferSeconds",
        "coyoteTimeSeconds",
        "extraAirJumps",
        "airJumpForceMultiplier",
        "dashDistance",
        "dashDuration",
        "dashCooldown",
        "allowAirDash",
        "flattenVerticalVelocityDuringDash",
        "dashJumpHorizontalMultiplier",
        "dashJumpDashSpeedCarryMultiplier",
        "dashJumpBoostSeconds",
        "airDashAnimationMinSeconds",
        "dashEndAnimationMinSeconds",
        "enableDashAfterimage",
        "dashAfterimageVisualRoot",
        "dashAfterimageSpawnInterval",
        "dashAfterimageLifetime",
        "dashAfterimageColor",
        "dashAfterimageIncludeMeshRenderers",
        "dashAfterimageIncludeInactiveRenderers",
        "actionAnimationCrossFadeSeconds",
        "jumpAnimationCrossFadeSeconds",
        "groundCheck",
        "groundCheckLocalOffset",
        "groundCheckRadius",
        "groundMask",
        "groundFallbackDistance",
        "useAnySolidGroundFallback",
        "enableOneWayPlatforms",
        "dropThroughSeconds",
        "dropThroughStartSpeed",
        "dropThroughPlatformSearchDistance",
        "dropInputThreshold",
        "oneWayPlatformPrecheckHeight",
        "oneWayPlatformPrecheckPadding",
        "preventAirWallSticking",
        "wallNormalThreshold",
        "wallContactGraceSeconds",
        "useNoFrictionColliderMaterial",
        "enableDamageKnockback",
        "knockbackForce",
        "knockbackControlLockSeconds",
        "lockControlUntilKnockbackLands"
    };

    private static readonly InspectorSection[] Sections =
    {
        new InspectorSection("movement", true, new[]
        {
            "movementMode",
            "moveSpeed",
            "airControl",
            "useCameraRelativeMovement",
            "movementCamera",
            "freeTurnSpeed"
        }),
        new InspectorSection("sideScroller", false, new[]
        {
            "lockedZ",
            "movementAxis"
        }),
        new InspectorSection("jump", true, new[]
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
        new InspectorSection("dash", true, new[]
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
        new InspectorSection("dashAfterimage", false, new[]
        {
            "enableDashAfterimage",
            "dashAfterimageVisualRoot",
            "dashAfterimageSpawnInterval",
            "dashAfterimageLifetime",
            "dashAfterimageColor",
            "dashAfterimageIncludeMeshRenderers",
            "dashAfterimageIncludeInactiveRenderers"
        }),
        new InspectorSection("animation", true, new[]
        {
            "actionAnimationCrossFadeSeconds",
            "jumpAnimationCrossFadeSeconds"
        }),
        new InspectorSection("ground", false, new[]
        {
            "groundCheck",
            "groundCheckLocalOffset",
            "groundCheckRadius",
            "groundMask",
            "groundFallbackDistance",
            "useAnySolidGroundFallback"
        }),
        new InspectorSection("oneWayPlatform", false, new[]
        {
            "enableOneWayPlatforms",
            "dropThroughSeconds",
            "dropThroughStartSpeed",
            "dropThroughPlatformSearchDistance",
            "dropInputThreshold",
            "oneWayPlatformPrecheckHeight",
            "oneWayPlatformPrecheckPadding"
        }),
        new InspectorSection("wall", false, new[]
        {
            "preventAirWallSticking",
            "wallNormalThreshold",
            "wallContactGraceSeconds",
            "useNoFrictionColliderMaterial"
        }),
        new InspectorSection("damageKnockback", false, new[]
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
        expanded = EditorGUILayout.Foldout(expanded, GetSectionTitle(section.key), true, EditorStyles.foldoutHeader);
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
        expanded = EditorGUILayout.Foldout(expanded, GetSectionTitle(OtherSectionKey), true, EditorStyles.foldoutHeader);
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
        if (ShortLabelKeys.Contains(property.name))
        {
            GUIContent shortLabel = SideScrollerInspectorLabels.Content(
                OwnerTypeName + ".short." + property.name,
                ObjectNames.NicifyVariableName(property.name));
            shortLabel.tooltip = GetTooltip(property);
            EditorGUILayout.PropertyField(property, shortLabel, true);
            return;
        }

        GUIContent label = SideScrollerInspectorLabels.Content(
            OwnerTypeName,
            property.name,
            ObjectNames.NicifyVariableName(property.name));
        EditorGUILayout.PropertyField(property, label, true);
    }

    private string GetSectionTitle(string sectionKey)
    {
        return SideScrollerInspectorLabels.Section(
            OwnerTypeName + ".section." + sectionKey,
            ObjectNames.NicifyVariableName(sectionKey));
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
        public readonly bool defaultExpanded;
        public readonly string[] propertyNames;

        public InspectorSection(string key, bool defaultExpanded, string[] propertyNames)
        {
            this.key = key;
            this.defaultExpanded = defaultExpanded;
            this.propertyNames = propertyNames;
        }
    }
}
