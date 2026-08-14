using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyPatrol3D))]
[CanEditMultipleObjects]
public class EnemyPatrol3DEditor : Editor
{
    private SerializedProperty attackMode;

    private void OnEnable()
    {
        SideScrollerInspectorLabels.ReloadIfNeeded();
        attackMode = serializedObject.FindProperty("attackMode");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();
        DrawSection("Enemy Type", "attackMode");

        if (attackMode == null || attackMode.hasMultipleDifferentValues)
        {
            EditorGUILayout.HelpBox(
                SideScrollerInspectorLabels.Text("EnemyPatrol3D.selectOneAttackMode", "Select one Attack Mode to show its specific attack settings."),
                MessageType.Info);
            DrawCommonSections();
            serializedObject.ApplyModifiedProperties();
            return;
        }

        DrawCommonSections();
        DrawAttackSection((EnemyPatrol3D.AttackMode)attackMode.enumValueIndex);
        DrawSection("Attack Timing", "attackCooldown", "attackWindup", "attackLockSeconds");
        DrawSection("Death", "launchAwayOnDeath", "deathLaunchSpeed", "deathLaunchUpSpeed", "deathSpinDegreesPerSecond", "deathDestroyDelay");
        DrawSection("Damage Knockback", "knockbackOnDamage", "damageKnockbackForce", "damageKnockbackLockSeconds", "airborneHitPauseNormalizedTime", "damageLandingRecoverySeconds", "damageGroundCheckDistance", "damageGroundMask");
        DrawSection("Respawn", "respawnAfterCameraLeaves", "respawnCameraAwaySeconds", "respawnViewportPadding");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawCommonSections()
    {
        DrawSection("Patrol",
            "movementMode",
            "useTransformRightAsMovementAxis",
            "movementAxis",
            "lockDepthToMovementPlane",
            "moveSpeed",
            "patrolMoveSpeed",
            "homeStopDistance",
            "fallbackPatrolHalfWidth",
            "patrolRadius",
            "patrolDestinationReachDistance",
            "patrolDestinationMinDistance",
            "patrolObstacleMask",
            "usePatrolObstacleMask",
            "patrolObstacleCheckDistance",
            "patrolObstacleRayHeights");

        DrawSection("Detection",
            "searchRange",
            "giveUpRange",
            "detectionBoxOffset",
            "detectionBoxHeight",
            "detectionBoxDepth",
            "giveUpBoxPadding",
            "showDetectionBoxGizmo",
            "onlyShowDetectionBoxWhenSelected");
    }

    private void DrawAttackSection(EnemyPatrol3D.AttackMode mode)
    {
        switch (mode)
        {
            case EnemyPatrol3D.AttackMode.Melee:
                DrawSection("Melee Attack",
                    "attackRange",
                    "meleeAttackHeight",
                    "attackDamage",
                    "meleeHitSound",
                    "meleeHitSoundVolume");
                break;

            case EnemyPatrol3D.AttackMode.Ranged:
                DrawSection("Ranged Attack",
                    "projectileDamage",
                    "projectileSpeed",
                    "projectileLifetime",
                    "projectileHitSound",
                    "projectileHitSoundVolume",
                    "projectileLocalOffset",
                    "returnSpeed");
                DrawSection("Ranged Rhythm",
                    "useRangedAttackRhythm",
                    "rangedAttackRhythm");
                break;

            case EnemyPatrol3D.AttackMode.Boss:
                DrawSection("Boss Melee Attack",
                    "attackRange",
                    "meleeAttackHeight",
                    "attackDamage",
                    "meleeHitSound",
                    "meleeHitSoundVolume");
                DrawSection("Boss Contact Damage",
                    "bossContactDamageEnabled",
                    "bossContactDamage",
                    "bossContactDamageCooldown",
                    "bossContactDamageBoxSize",
                    "bossContactDamageBoxCenter",
                    "bossContactDamageTargetMask");
                DrawSection("Boss Ranged Attack",
                    "projectileDamage",
                    "projectileSpeed",
                    "projectileLifetime",
                    "projectileLocalOffset",
                    "returnSpeed",
                    "projectileHitSound",
                    "projectileHitSoundVolume",
                    "bossRangedDistance",
                    "bossRangedDistanceTolerance");
                DrawSection("Ranged Rhythm",
                    "useRangedAttackRhythm",
                    "rangedAttackRhythm");
                DrawBossProjectileTypes();
                break;
        }
    }

    private void DrawBossProjectileTypes()
    {
        SerializedProperty projectiles = serializedObject.FindProperty("bossProjectileTypes");
        if (projectiles == null)
        {
            return;
        }

        if (!DrawFoldoutHeader("Boss Remote Projectiles", false))
        {
            return;
        }

        EditorGUI.indentLevel++;
        EditorGUILayout.Space(6f);
        EditorGUILayout.PropertyField(projectiles.FindPropertyRelative("Array.size"), SideScrollerInspectorLabels.Content("EnemyPatrol3D.bossProjectileTypesSize", "Size"));

        for (int i = 0; i < projectiles.arraySize; i++)
        {
            SerializedProperty element = projectiles.GetArrayElementAtIndex(i);
            if (element == null)
            {
                continue;
            }

            SerializedProperty name = element.FindPropertyRelative("name");
            string label = name != null && !string.IsNullOrEmpty(name.stringValue)
                ? $"{i}: {name.stringValue}"
                : $"Projectile {i}";

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            element.isExpanded = EditorGUILayout.Foldout(element.isExpanded, label, true);

            if (element.isExpanded)
            {
                EditorGUI.indentLevel++;
                DrawRelativeProperty(element, "name");
                DrawRelativeProperty(element, "visualTemplate");
                DrawRelativeProperty(element, "canBeReflected");
                DrawRelativeProperty(element, "projectileDamage");
                DrawRelativeProperty(element, "projectileSpeed");
                DrawRelativeProperty(element, "projectileLifetime");
                DrawRelativeProperty(element, "rangedDistance");
                DrawRelativeProperty(element, "hitSound");
                DrawRelativeProperty(element, "hitSoundVolume");
                EditorGUI.indentLevel--;

                if (GUILayout.Button(SideScrollerInspectorLabels.Text("EnemyPatrol3D.removeProjectileType", "Remove Projectile Type")))
                {
                    projectiles.DeleteArrayElementAtIndex(i);
                    EditorGUILayout.EndVertical();
                    break;
                }
            }

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button(SideScrollerInspectorLabels.Text("EnemyPatrol3D.addProjectileType", "Add Projectile Type")))
        {
            int index = projectiles.arraySize;
            projectiles.InsertArrayElementAtIndex(index);
            InitializeBossProjectileType(projectiles.GetArrayElementAtIndex(index), index);
        }

        EditorGUI.indentLevel--;
    }

    private static void DrawRelativeProperty(SerializedProperty parent, string propertyName)
    {
        SerializedProperty property = parent.FindPropertyRelative(propertyName);
        if (property != null)
        {
            GUIContent label = SideScrollerInspectorLabels.Content(
                "EnemyPatrol3D.BossProjectileType",
                propertyName,
                ObjectNames.NicifyVariableName(propertyName));
            EditorGUILayout.PropertyField(property, label, true);
        }
    }

    private static void InitializeBossProjectileType(SerializedProperty element, int index)
    {
        if (element == null)
        {
            return;
        }

        SetString(element, "name", $"Projectile {index + 1}");
        SetObjectReference(element, "visualTemplate", null);
        SetBool(element, "canBeReflected", true);
        SetInt(element, "projectileDamage", 1);
        SetFloat(element, "projectileSpeed", 5.5f);
        SetFloat(element, "projectileLifetime", 5f);
        SetFloat(element, "rangedDistance", 4.5f);
        SetObjectReference(element, "hitSound", null);
        SetFloat(element, "hitSoundVolume", 1f);
        element.isExpanded = true;
    }

    private static void SetString(SerializedProperty parent, string propertyName, string value)
    {
        SerializedProperty property = parent.FindPropertyRelative(propertyName);
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void SetObjectReference(SerializedProperty parent, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = parent.FindPropertyRelative(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetBool(SerializedProperty parent, string propertyName, bool value)
    {
        SerializedProperty property = parent.FindPropertyRelative(propertyName);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetInt(SerializedProperty parent, string propertyName, int value)
    {
        SerializedProperty property = parent.FindPropertyRelative(propertyName);
        if (property != null)
        {
            property.intValue = value;
        }
    }

    private static void SetFloat(SerializedProperty parent, string propertyName, float value)
    {
        SerializedProperty property = parent.FindPropertyRelative(propertyName);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private void DrawSection(string title, params string[] propertyNames)
    {
        if (!DrawFoldoutHeader(title, IsSectionExpandedByDefault(title)))
        {
            return;
        }

        EditorGUI.indentLevel++;
        for (int i = 0; i < propertyNames.Length; i++)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyNames[i]);
            if (property == null)
            {
                continue;
            }

            GUIContent label = SideScrollerInspectorLabels.Content(
                "EnemyPatrol3D",
                propertyNames[i],
                ObjectNames.NicifyVariableName(propertyNames[i]));
            EditorGUILayout.PropertyField(property, label, true);
        }

        EditorGUI.indentLevel--;
    }

    private bool DrawFoldoutHeader(string title, bool defaultExpanded)
    {
        string sectionTitle = SideScrollerInspectorLabels.Section("EnemyPatrol3D." + title, title);
        string prefsKey = "SideScroller3D.EnemyPatrol3DEditor." + title;
        bool expanded = SessionState.GetBool(prefsKey, defaultExpanded);

        EditorGUILayout.Space(4f);
        expanded = EditorGUILayout.Foldout(expanded, sectionTitle, true, EditorStyles.foldoutHeader);
        SessionState.SetBool(prefsKey, expanded);
        return expanded;
    }

    private static bool IsSectionExpandedByDefault(string title)
    {
        switch (title)
        {
            case "Enemy Type":
            case "Patrol":
            case "Detection":
            case "Melee Attack":
            case "Ranged Attack":
            case "Boss Melee Attack":
            case "Boss Ranged Attack":
            case "Ranged Rhythm":
                return true;
            default:
                return false;
        }
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
}
