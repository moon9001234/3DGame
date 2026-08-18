using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerWeaponAttackProfile))]
[CanEditMultipleObjects]
public class PlayerWeaponAttackProfileEditor : Editor
{
    private void OnEnable()
    {
        SideScrollerInspectorLabels.ReloadIfNeeded();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();
        DrawDefinitionSection();
        DrawSection("attackBehavior",
            "attackCooldown",
            "attackMoveLockSeconds",
            "useAttackAnimationLength",
            "attackSpeedMultiplier",
            "attackCrossFadeSeconds",
            "allowAirAttacks");

        DrawSection("targets", "targetMask");
        DrawSection("comboAttacks", "attacks");

        DrawSection("hitAudio",
            "attackHitSound",
            "attackHitSoundVolume",
            "targetHitSounds");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawDefinitionSection()
    {
        DrawSection("definition",
            "weaponDefinition",
            "applyDefinitionOnAwake",
            "applyDefinitionInEditor");

        SerializedProperty definitionProperty = serializedObject.FindProperty("weaponDefinition");
        if (definitionProperty != null && definitionProperty.objectReferenceValue == null)
        {
            EditorGUILayout.HelpBox(
                SideScrollerInspectorLabels.Text("PlayerWeaponAttackProfile.weaponDefinitionMissing", "No Weapon Definition asset is assigned yet."),
                MessageType.Info);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button(SideScrollerInspectorLabels.Text("PlayerWeaponAttackProfile.applyDefinition", "Apply Weapon Definition")))
            {
                serializedObject.ApplyModifiedProperties();
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] is PlayerWeaponAttackProfile profile)
                    {
                        Undo.RecordObject(profile, "Apply Weapon Definition");
                        profile.ApplyDefinition();
                        EditorUtility.SetDirty(profile);

                        PlayerWeaponHitbox hitbox = profile.GetComponent<PlayerWeaponHitbox>();
                        if (hitbox != null)
                        {
                            Undo.RecordObject(hitbox, "Apply Weapon Definition");
                            hitbox.ApplyDefinition(profile.Definition);
                            EditorUtility.SetDirty(hitbox);
                        }
                    }
                }

                serializedObject.Update();
            }

            if (GUILayout.Button(SideScrollerInspectorLabels.Text("PlayerWeaponAttackProfile.createDefinition", "Create & Assign")))
            {
                serializedObject.ApplyModifiedProperties();
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] is PlayerWeaponAttackProfile profile)
                    {
                        Undo.RecordObject(profile, "Create Weapon Definition");
                        WeaponDefinition3D definition = SideScrollerDefinitionAssetUtility.CreateWeaponDefinition(profile);
                        SerializedObject profileSerializedObject = new SerializedObject(profile);
                        SerializedProperty weaponDefinitionProperty = profileSerializedObject.FindProperty("weaponDefinition");
                        if (weaponDefinitionProperty != null)
                        {
                            weaponDefinitionProperty.objectReferenceValue = definition;
                            profileSerializedObject.ApplyModifiedProperties();
                        }

                        PlayerWeaponHitbox hitbox = profile.GetComponent<PlayerWeaponHitbox>();
                        if (hitbox != null)
                        {
                            Undo.RecordObject(hitbox, "Create Weapon Definition");
                            hitbox.ApplyDefinition(definition);
                            EditorUtility.SetDirty(hitbox);
                        }

                        profile.ApplyDefinition(definition);
                        EditorUtility.SetDirty(profile);
                    }
                }

                serializedObject.Update();
            }
        }
    }

    private void DrawScriptField()
    {
        SerializedProperty script = serializedObject.FindProperty("m_Script");
        if (script == null)
        {
            return;
        }

        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(script);
        }
    }

    private void DrawSection(string sectionKey, params string[] propertyNames)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField(SideScrollerInspectorLabels.Section(sectionKey, ObjectNames.NicifyVariableName(sectionKey)), EditorStyles.boldLabel);

        for (int i = 0; i < propertyNames.Length; i++)
        {
            DrawProperty(propertyNames[i]);
        }
    }

    private void DrawProperty(string propertyName)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        if (propertyName == "attacks")
        {
            DrawAttackArray(property);
            return;
        }

        GUIContent label = SideScrollerInspectorLabels.Content(
            "PlayerWeaponAttackProfile." + propertyName,
            ObjectNames.NicifyVariableName(propertyName));
        EditorGUILayout.PropertyField(property, label, true);
    }

    private void DrawAttackArray(SerializedProperty property)
    {
        GUIContent label = SideScrollerInspectorLabels.Content(
            "PlayerWeaponAttackProfile.attacks",
            ObjectNames.NicifyVariableName(property.name));
        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
        if (!property.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        SerializedProperty size = property.FindPropertyRelative("Array.size");
        if (size != null)
        {
            EditorGUILayout.PropertyField(size, SideScrollerInspectorLabels.Content("PlayerWeaponAttackProfile.attacksSize", "Size"));
        }

        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(i);
            if (element != null)
            {
                EditorGUILayout.PropertyField(element, true);
            }
        }

        EditorGUI.indentLevel--;
    }
}

[CustomPropertyDrawer(typeof(PlayerWeaponAttackStep))]
public class PlayerWeaponAttackStepDrawer : PropertyDrawer
{
    private static readonly string[] FieldNames =
    {
        "animatorStateName",
        "animationClip",
        "triggerName",
        "damage",
        "nextInputWindowSeconds",
        "nextAttackStartFrame",
        "attackEffectRoot",
        "cameraShakeAmplitude",
        "cameraShakeDuration",
        "cameraShakeFrequency"
    };

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;
        if (!property.isExpanded)
        {
            return height;
        }

        height += EditorGUIUtility.standardVerticalSpacing;
        for (int i = 0; i < FieldNames.Length; i++)
        {
            if (!ShouldDrawField(property, FieldNames[i]))
            {
                continue;
            }

            SerializedProperty child = property.FindPropertyRelative(FieldNames[i]);
            if (child != null)
            {
                height += EditorGUI.GetPropertyHeight(child, true) + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        return height;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SideScrollerInspectorLabels.ReloadIfNeeded();

        Rect foldoutRect = new Rect(
            position.x,
            position.y,
            position.width,
            EditorGUIUtility.singleLineHeight);

        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, BuildFoldoutLabel(property, label), true);
        if (!property.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        float y = foldoutRect.yMax + EditorGUIUtility.standardVerticalSpacing;
        for (int i = 0; i < FieldNames.Length; i++)
        {
            if (!ShouldDrawField(property, FieldNames[i]))
            {
                continue;
            }

            SerializedProperty child = property.FindPropertyRelative(FieldNames[i]);
            if (child == null)
            {
                continue;
            }

            GUIContent childLabel = SideScrollerInspectorLabels.Content(
                "PlayerWeaponAttackStep." + FieldNames[i],
                ObjectNames.NicifyVariableName(FieldNames[i]));
            float childHeight = EditorGUI.GetPropertyHeight(child, childLabel, true);
            Rect childRect = new Rect(position.x, y, position.width, childHeight);
            EditorGUI.PropertyField(childRect, child, childLabel, true);
            y += childHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        EditorGUI.indentLevel--;
    }

    private static GUIContent BuildFoldoutLabel(SerializedProperty property, GUIContent fallback)
    {
        string stepLabel = SideScrollerInspectorLabels.Text("PlayerWeaponAttackStep.step", "Attack");
        int displayIndex = TryParseArrayIndex(property.propertyPath, out int index) ? index + 1 : 0;
        string title = displayIndex > 0 ? stepLabel + " " + displayIndex : fallback.text;

        SerializedProperty stateName = property.FindPropertyRelative("animatorStateName");
        if (stateName != null && !string.IsNullOrWhiteSpace(stateName.stringValue))
        {
            title += " - " + stateName.stringValue;
        }

        return new GUIContent(title);
    }

    private static bool ShouldDrawField(SerializedProperty property, string fieldName)
    {
        if (fieldName != "nextInputWindowSeconds" && fieldName != "nextAttackStartFrame")
        {
            return true;
        }

        return HasNextAttackStep(property);
    }

    private static bool HasNextAttackStep(SerializedProperty property)
    {
        if (property == null || !TryParseArrayIndex(property.propertyPath, out int index))
        {
            return true;
        }

        SerializedProperty attacks = FindParentArrayProperty(property);
        return attacks == null || index < attacks.arraySize - 1;
    }

    private static SerializedProperty FindParentArrayProperty(SerializedProperty property)
    {
        const string arrayMarker = ".Array.data[";
        int markerIndex = property.propertyPath.LastIndexOf(arrayMarker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return null;
        }

        string arrayPath = property.propertyPath.Substring(0, markerIndex);
        return property.serializedObject.FindProperty(arrayPath);
    }

    private static bool TryParseArrayIndex(string propertyPath, out int index)
    {
        index = -1;
        int start = propertyPath.LastIndexOf("[", StringComparison.Ordinal);
        int end = propertyPath.LastIndexOf("]", StringComparison.Ordinal);
        if (start < 0 || end <= start)
        {
            return false;
        }

        string rawIndex = propertyPath.Substring(start + 1, end - start - 1);
        return int.TryParse(rawIndex, out index);
    }
}
