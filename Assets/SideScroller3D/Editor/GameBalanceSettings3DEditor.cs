using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(GameBalanceSettings3D))]
public class GameBalanceSettings3DEditor : Editor
{
    private const string FoldoutPrefsPrefix = "SideScroller3D.GameBalanceSettings3DEditor.";

    private static readonly string[] PlayerMotorFields =
    {
        "movementMode",
        "moveSpeed",
        "airControl",
        "useCameraRelativeMovement",
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
        "dashAfterimageSpawnInterval",
        "dashAfterimageLifetime",
        "dashAfterimageColor",
        "dashAfterimageIncludeMeshRenderers",
        "dashAfterimageIncludeInactiveRenderers",
        "actionAnimationCrossFadeSeconds",
        "jumpAnimationCrossFadeSeconds",
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

    private static readonly string[] PlayerCombatFields =
    {
        "combatDetectionRange",
        "combatVerticalRange",
        "combatMemorySeconds"
    };

    private static readonly string[] CameraShakeFields =
    {
        "defaultAmplitude",
        "defaultDuration",
        "defaultFrequency",
        "useUnscaledTime"
    };

    private static readonly string[] EnemyFields =
    {
        "attackMode",
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
        "patrolObstacleRayHeights",
        "searchRange",
        "giveUpRange",
        "detectionBoxOffset",
        "detectionBoxHeight",
        "detectionBoxDepth",
        "giveUpBoxPadding",
        "showDetectionBoxGizmo",
        "onlyShowDetectionBoxWhenSelected",
        "attackRange",
        "meleeAttackHeight",
        "attackDamage",
        "meleeHitSoundVolume",
        "projectileDamage",
        "projectileSpeed",
        "projectileLifetime",
        "projectileHitSoundVolume",
        "projectileLocalOffset",
        "returnSpeed",
        "bossRangedDistance",
        "bossRangedDistanceTolerance",
        "bossContactDamageEnabled",
        "bossContactDamage",
        "bossContactDamageCooldown",
        "bossContactDamageBoxSize",
        "bossContactDamageBoxCenter",
        "bossContactDamageTargetMask",
        "attackCooldown",
        "useRangedAttackRhythm",
        "rangedAttackRhythm",
        "rangedAttackGroupCooldown",
        "attackWindup",
        "attackLockSeconds",
        "launchAwayOnDeath",
        "deathLaunchSpeed",
        "deathLaunchUpSpeed",
        "deathSpinDegreesPerSecond",
        "deathDestroyDelay",
        "knockbackOnDamage",
        "damageKnockbackForce",
        "damageKnockbackLockSeconds",
        "airborneHitPauseNormalizedTime",
        "damageLandingRecoverySeconds",
        "damageGroundCheckDistance",
        "damageGroundMask",
        "respawnAfterCameraLeaves",
        "respawnCameraAwaySeconds",
        "respawnViewportPadding"
    };

    private static readonly string[] EnemyHitEffectFields =
    {
        "effectPrefab",
        "effectAnchorName",
        "stopEffectOnAwake"
    };

    private static readonly string[] WeaponProfileFields =
    {
        "attackCooldown",
        "attackMoveLockSeconds",
        "useAttackAnimationLength",
        "attackSpeedMultiplier",
        "attackCrossFadeSeconds",
        "allowAirAttacks",
        "targetMask",
        "attacks",
        "attackHitSound",
        "attackHitSoundVolume",
        "targetHitSounds"
    };

    private static readonly string[] WeaponHitboxFields =
    {
        "weaponSize",
        "weaponColor",
        "weaponModelRoot",
        "useModelBoundsForHitbox",
        "updateColliderDuringPlay",
        "modelBoundsPadding",
        "projectileReflectExtraRange",
        "projectileReflectEffectPrefab",
        "projectileReflectEffectScale",
        "projectileReflectEffectFallbackLifetime"
    };

    [MenuItem("Tools/3D \u904a\u6232\u5de5\u5177/\u5efa\u7acb\u96c6\u4e2d\u6578\u503c\u8a2d\u5b9a")]
    private static void CreateOrSelectSettingsObject()
    {
        GameBalanceSettings3D existing = FindFirstSceneObject<GameBalanceSettings3D>(true);
        if (existing != null)
        {
            Selection.activeObject = existing.gameObject;
            EditorGUIUtility.PingObject(existing.gameObject);
            return;
        }

        GameObject settingsObject = new GameObject("Game Balance Settings");
        Undo.RegisterCreatedObjectUndo(settingsObject, "Create Game Balance Settings");
        settingsObject.AddComponent<GameBalanceSettings3D>();
        Selection.activeObject = settingsObject;
        EditorGUIUtility.PingObject(settingsObject);

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
    }

    public override void OnInspectorGUI()
    {
        SideScrollerInspectorLabels.ReloadIfNeeded();
        serializedObject.Update();
        DrawScriptField();
        serializedObject.ApplyModifiedProperties();

        DrawSyncToolsSection();

        serializedObject.Update();
        DrawGroupedLocalizedProperties();
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(6f);
        EditorGUILayout.HelpBox(
            "\u6575\u4eba\u6703\u4f9d Enemy_A / Enemy_B \u9019\u985e\u540d\u7a31\u5206\u7d44\uff1b\u6b66\u5668\u6703\u6383\u63cf Player_Weapon \u5e95\u4e0b\u6240\u6709\u5b50\u5c64\u4e2d\u5e36\u6709\u6b66\u5668\u8173\u672c\u7684\u7269\u4ef6\u3002\u540c\u6b65\u524d\u5efa\u8b70\u5148\u8b80\u53d6\u4e00\u6b21\u76ee\u524d\u5834\u666f\u6578\u503c\u3002",
            MessageType.Info);
    }

    private void DrawSyncToolsSection()
    {
        if (!DrawFoldoutHeader("syncTools", "\u540c\u6b65\u5de5\u5177", true))
        {
            return;
        }

        EditorGUI.indentLevel++;
        DrawSyncTools();
        EditorGUI.indentLevel--;
    }

    private void DrawSyncTools()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("\u5f9e\u5834\u666f\u8b80\u53d6\u76ee\u524d\u6578\u503c", GUILayout.Height(30f)))
            {
                ReadFromScene((GameBalanceSettings3D)target);
            }

            if (GUILayout.Button("\u540c\u6b65\u5230\u5834\u666f\u7269\u4ef6", GUILayout.Height(30f)))
            {
                SyncToScene((GameBalanceSettings3D)target);
            }
        }
    }

    private static void ReadFromScene(GameBalanceSettings3D settings)
    {
        Undo.RecordObject(settings, "Read Game Balance Settings From Scene");

        PlayerMotor3D playerMotor = FindFirstSceneObject<PlayerMotor3D>(settings.includeInactiveObjects);
        if (playerMotor != null)
        {
            settings.playerObjectName = playerMotor.name;
            CopyFields(playerMotor, settings.playerMotor, PlayerMotorFields);
        }

        PlayerCombat3D playerCombat = playerMotor != null
            ? playerMotor.GetComponent<PlayerCombat3D>()
            : FindFirstSceneObject<PlayerCombat3D>(settings.includeInactiveObjects);
        if (playerCombat != null)
        {
            CopyFields(playerCombat, settings.playerCombat, PlayerCombatFields);
        }

        CameraShake3D cameraShake = FindCameraShake(settings.includeInactiveObjects, false);
        if (cameraShake != null)
        {
            settings.cameraObjectName = cameraShake.name;
            CopyFields(cameraShake, settings.cameraShake, CameraShakeFields);
        }
        else
        {
            Camera camera = FindMainSceneCamera(settings.includeInactiveObjects);
            settings.cameraObjectName = camera != null ? camera.name : string.Empty;
        }

        settings.enemies = ReadEnemies(settings);
        settings.weapons = ReadWeapons(settings);

        EditorUtility.SetDirty(settings);
        Debug.Log("Game Balance Settings: finished reading scene values.", settings);
    }

    private static void SyncToScene(GameBalanceSettings3D settings)
    {
        int changedCount = 0;

        PlayerMotor3D playerMotor = FindFirstSceneObject<PlayerMotor3D>(settings.includeInactiveObjects);
        if (playerMotor != null && settings.playerMotor.sync)
        {
            CopyFields(settings.playerMotor, playerMotor, PlayerMotorFields);
            MarkChanged(playerMotor);
            changedCount++;
        }

        PlayerCombat3D playerCombat = playerMotor != null
            ? playerMotor.GetComponent<PlayerCombat3D>()
            : FindFirstSceneObject<PlayerCombat3D>(settings.includeInactiveObjects);
        if (playerCombat != null && settings.playerCombat.sync)
        {
            CopyFields(settings.playerCombat, playerCombat, PlayerCombatFields);
            MarkChanged(playerCombat);
            changedCount++;
        }

        CameraShake3D cameraShake = FindCameraShake(settings.includeInactiveObjects, settings.cameraShake.sync);
        if (cameraShake != null && settings.cameraShake.sync)
        {
            CopyFields(settings.cameraShake, cameraShake, CameraShakeFields);
            MarkChanged(cameraShake);
            changedCount++;
        }

        changedCount += SyncEnemies(settings);
        changedCount += SyncWeapons(settings);

        Debug.Log("Game Balance Settings: synchronized " + changedCount + " scene components.", settings);
    }

    private static GameBalanceSettings3D.EnemyTuningEntry[] ReadEnemies(GameBalanceSettings3D settings)
    {
        Dictionary<string, List<EnemyPatrol3D>> groups = new Dictionary<string, List<EnemyPatrol3D>>(StringComparer.OrdinalIgnoreCase);
        EnemyPatrol3D[] enemies = FindSceneObjects<EnemyPatrol3D>(settings.includeInactiveObjects);
        for (int i = 0; i < enemies.Length; i++)
        {
            string key = ResolveNameKey(enemies[i].name, settings.enemyNameKeys);
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            if (!groups.TryGetValue(key, out List<EnemyPatrol3D> group))
            {
                group = new List<EnemyPatrol3D>();
                groups.Add(key, group);
            }

            group.Add(enemies[i]);
        }

        List<GameBalanceSettings3D.EnemyTuningEntry> entries = new List<GameBalanceSettings3D.EnemyTuningEntry>();
        foreach (KeyValuePair<string, List<EnemyPatrol3D>> pair in groups)
        {
            if (pair.Value.Count <= 0 || pair.Value[0] == null)
            {
                continue;
            }

            GameBalanceSettings3D.EnemyTuningEntry entry = new GameBalanceSettings3D.EnemyTuningEntry
            {
                enemyNameKey = pair.Key,
                sceneObjectCount = pair.Value.Count,
                valuesDifferInScene = HasDifferentValues(pair.Value, EnemyFields)
                    || HasDifferentEnemyHitEffectValues(pair.Value)
            };
            CopyFields(pair.Value[0], entry, EnemyFields);
            DamageHitEffect3D hitEffect = pair.Value[0].GetComponent<DamageHitEffect3D>();
            if (hitEffect != null)
            {
                CopyFields(hitEffect, entry.hitEffect, EnemyHitEffectFields);
            }

            entries.Add(entry);
        }

        entries.Sort((left, right) => string.Compare(left.enemyNameKey, right.enemyNameKey, StringComparison.OrdinalIgnoreCase));
        return entries.ToArray();
    }

    private static int SyncEnemies(GameBalanceSettings3D settings)
    {
        int changedCount = 0;
        EnemyPatrol3D[] sceneEnemies = FindSceneObjects<EnemyPatrol3D>(settings.includeInactiveObjects);
        for (int i = 0; i < settings.enemies.Length; i++)
        {
            GameBalanceSettings3D.EnemyTuningEntry entry = settings.enemies[i];
            if (entry == null || !entry.sync || string.IsNullOrEmpty(entry.enemyNameKey))
            {
                continue;
            }

            for (int j = 0; j < sceneEnemies.Length; j++)
            {
                EnemyPatrol3D enemy = sceneEnemies[j];
                if (enemy == null || !NameMatchesKey(enemy.name, entry.enemyNameKey))
                {
                    continue;
                }

                CopyFields(entry, enemy, EnemyFields);
                MarkChanged(enemy);
                changedCount++;

                if (entry.hitEffect != null && entry.hitEffect.sync)
                {
                    DamageHitEffect3D hitEffect = enemy.GetComponent<DamageHitEffect3D>();
                    if (hitEffect == null)
                    {
                        hitEffect = Undo.AddComponent<DamageHitEffect3D>(enemy.gameObject);
                    }

                    CopyFields(entry.hitEffect, hitEffect, EnemyHitEffectFields);
                    hitEffect.SetEffectPrefab(entry.hitEffect.effectPrefab);
                    MarkChanged(hitEffect);
                    changedCount++;
                }
            }
        }

        return changedCount;
    }

    private static GameBalanceSettings3D.WeaponTuningEntry[] ReadWeapons(GameBalanceSettings3D settings)
    {
        Dictionary<string, List<Transform>> groups = FindWeaponRootGroups(settings);
        List<GameBalanceSettings3D.WeaponTuningEntry> entries = new List<GameBalanceSettings3D.WeaponTuningEntry>();

        foreach (KeyValuePair<string, List<Transform>> pair in groups)
        {
            Transform sampleRoot = pair.Value.Count > 0 ? pair.Value[0] : null;
            if (sampleRoot == null)
            {
                continue;
            }

            GameBalanceSettings3D.WeaponTuningEntry entry = new GameBalanceSettings3D.WeaponTuningEntry
            {
                weaponNameKey = pair.Key,
                sceneObjectCount = pair.Value.Count,
                valuesDifferInScene = HasDifferentWeaponValues(pair.Value)
            };

            PlayerWeaponAttackProfile profile = sampleRoot.GetComponentInChildren<PlayerWeaponAttackProfile>(true);
            if (profile != null)
            {
                CopyFields(profile, entry.attackProfile, WeaponProfileFields);
            }

            PlayerWeaponHitbox hitbox = sampleRoot.GetComponentInChildren<PlayerWeaponHitbox>(true);
            if (hitbox != null)
            {
                CopyFields(hitbox, entry.hitbox, WeaponHitboxFields);
            }

            entries.Add(entry);
        }

        entries.Sort((left, right) => string.Compare(left.weaponNameKey, right.weaponNameKey, StringComparison.OrdinalIgnoreCase));
        return entries.ToArray();
    }

    private static int SyncWeapons(GameBalanceSettings3D settings)
    {
        int changedCount = 0;
        Dictionary<string, List<Transform>> groups = FindWeaponRootGroups(settings);
        for (int i = 0; i < settings.weapons.Length; i++)
        {
            GameBalanceSettings3D.WeaponTuningEntry entry = settings.weapons[i];
            if (entry == null || !entry.sync || string.IsNullOrEmpty(entry.weaponNameKey))
            {
                continue;
            }

            if (!groups.TryGetValue(entry.weaponNameKey, out List<Transform> roots))
            {
                continue;
            }

            for (int j = 0; j < roots.Count; j++)
            {
                Transform root = roots[j];
                if (root == null)
                {
                    continue;
                }

                PlayerWeaponAttackProfile[] profiles = root.GetComponentsInChildren<PlayerWeaponAttackProfile>(true);
                for (int p = 0; p < profiles.Length; p++)
                {
                    if (entry.attackProfile.sync)
                    {
                        CopyFields(entry.attackProfile, profiles[p], WeaponProfileFields);
                        MarkChanged(profiles[p]);
                        changedCount++;
                    }
                }

                PlayerWeaponHitbox[] hitboxes = root.GetComponentsInChildren<PlayerWeaponHitbox>(true);
                for (int h = 0; h < hitboxes.Length; h++)
                {
                    if (entry.hitbox.sync)
                    {
                        CopyFields(entry.hitbox, hitboxes[h], WeaponHitboxFields);
                        MarkChanged(hitboxes[h]);
                        changedCount++;
                    }
                }
            }
        }

        return changedCount;
    }

    private static Dictionary<string, List<Transform>> FindWeaponRootGroups(GameBalanceSettings3D settings)
    {
        Dictionary<string, List<Transform>> groups = new Dictionary<string, List<Transform>>(StringComparer.OrdinalIgnoreCase);
        Transform[] weaponParents = FindNamedSceneTransforms(settings.playerWeaponRootName, settings.includeInactiveObjects);
        for (int i = 0; i < weaponParents.Length; i++)
        {
            Transform parent = weaponParents[i];
            HashSet<Transform> weaponRoots = new HashSet<Transform>();

            if (HasWeaponScriptsOnSelf(parent))
            {
                weaponRoots.Add(parent);
            }

            PlayerWeaponAttackProfile[] profiles = parent.GetComponentsInChildren<PlayerWeaponAttackProfile>(true);
            for (int profileIndex = 0; profileIndex < profiles.Length; profileIndex++)
            {
                if (profiles[profileIndex] != null)
                {
                    weaponRoots.Add(ResolveWeaponRoot(parent, profiles[profileIndex].transform));
                }
            }

            PlayerWeaponHitbox[] hitboxes = parent.GetComponentsInChildren<PlayerWeaponHitbox>(true);
            for (int hitboxIndex = 0; hitboxIndex < hitboxes.Length; hitboxIndex++)
            {
                if (hitboxes[hitboxIndex] != null)
                {
                    weaponRoots.Add(ResolveWeaponRoot(parent, hitboxes[hitboxIndex].transform));
                }
            }

            foreach (Transform weaponRoot in weaponRoots)
            {
                if (weaponRoot == null || !HasWeaponScripts(weaponRoot))
                {
                    continue;
                }

                if (!settings.includeInactiveObjects && !weaponRoot.gameObject.activeInHierarchy)
                {
                    continue;
                }

                string key = ResolvePrefabOrObjectName(weaponRoot.gameObject);
                if (!groups.TryGetValue(key, out List<Transform> group))
                {
                    group = new List<Transform>();
                    groups.Add(key, group);
                }

                group.Add(weaponRoot);
            }
        }

        return groups;
    }

    private static bool HasWeaponScripts(Transform root)
    {
        return root != null
            && (root.GetComponentInChildren<PlayerWeaponAttackProfile>(true) != null
                || root.GetComponentInChildren<PlayerWeaponHitbox>(true) != null);
    }

    private static bool HasWeaponScriptsOnSelf(Transform root)
    {
        return root != null
            && (root.GetComponent<PlayerWeaponAttackProfile>() != null
                || root.GetComponent<PlayerWeaponHitbox>() != null);
    }

    private static Transform ResolveWeaponRoot(Transform weaponParent, Transform scriptTransform)
    {
        if (weaponParent == null || scriptTransform == null)
        {
            return scriptTransform;
        }

        Transform current = scriptTransform;
        Transform result = scriptTransform;
        while (current != null && current != weaponParent)
        {
            result = current;
            current = current.parent;
        }

        return result != null ? result : scriptTransform;
    }

    private static CameraShake3D FindCameraShake(bool includeInactive, bool addIfMissing)
    {
        Camera camera = FindMainSceneCamera(includeInactive);
        if (camera == null)
        {
            return FindFirstSceneObject<CameraShake3D>(includeInactive);
        }

        CameraShake3D cameraShake = camera.GetComponent<CameraShake3D>();
        if (cameraShake == null && addIfMissing)
        {
            Undo.AddComponent<CameraShake3D>(camera.gameObject);
            cameraShake = camera.GetComponent<CameraShake3D>();
        }

        return cameraShake;
    }

    private static Camera FindMainSceneCamera(bool includeInactive)
    {
        Camera[] cameras = FindSceneObjects<Camera>(includeInactive);
        for (int i = 0; i < cameras.Length; i++)
        {
            if (cameras[i] != null && cameras[i].CompareTag("MainCamera"))
            {
                return cameras[i];
            }
        }

        return cameras.Length > 0 ? cameras[0] : null;
    }

    private static T FindFirstSceneObject<T>(bool includeInactive) where T : Component
    {
        T[] objects = FindSceneObjects<T>(includeInactive);
        return objects.Length > 0 ? objects[0] : null;
    }

    private static T[] FindSceneObjects<T>(bool includeInactive) where T : Component
    {
        T[] objects = Resources.FindObjectsOfTypeAll<T>();
        List<T> sceneObjects = new List<T>();
        for (int i = 0; i < objects.Length; i++)
        {
            T component = objects[i];
            if (component == null || !IsSceneObject(component.gameObject))
            {
                continue;
            }

            if (!includeInactive && !component.gameObject.activeInHierarchy)
            {
                continue;
            }

            sceneObjects.Add(component);
        }

        return sceneObjects.ToArray();
    }

    private static Transform[] FindNamedSceneTransforms(string objectName, bool includeInactive)
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return new Transform[0];
        }

        Transform[] transforms = FindSceneObjects<Transform>(includeInactive);
        List<Transform> matches = new List<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i] != null && NameMatchesKey(transforms[i].name, objectName))
            {
                matches.Add(transforms[i]);
            }
        }

        return matches.ToArray();
    }

    private static bool IsSceneObject(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        Scene scene = gameObject.scene;
        return scene.IsValid() && scene.isLoaded
            && (gameObject.hideFlags & HideFlags.HideInHierarchy) == 0
            && !EditorUtility.IsPersistent(gameObject);
    }

    private static string ResolveNameKey(string objectName, string[] nameKeys)
    {
        if (nameKeys == null)
        {
            return string.Empty;
        }

        for (int i = 0; i < nameKeys.Length; i++)
        {
            string key = nameKeys[i];
            if (NameMatchesKey(objectName, key))
            {
                return key;
            }
        }

        return string.Empty;
    }

    private static bool NameMatchesKey(string objectName, string key)
    {
        if (string.IsNullOrWhiteSpace(objectName) || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return string.Equals(objectName, key, StringComparison.OrdinalIgnoreCase)
            || objectName.StartsWith(key + " ", StringComparison.OrdinalIgnoreCase)
            || objectName.StartsWith(key + "(", StringComparison.OrdinalIgnoreCase)
            || objectName.StartsWith(key + "_", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolvePrefabOrObjectName(GameObject gameObject)
    {
        GameObject prefab = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);
        return prefab != null ? prefab.name : gameObject.name;
    }

    private static void CopyFields(object source, object destination, string[] fieldNames)
    {
        if (source == null || destination == null || fieldNames == null)
        {
            return;
        }

        if (destination is UnityEngine.Object unityObject)
        {
            Undo.RecordObject(unityObject, "Sync Game Balance Settings To Scene");
        }

        Type sourceType = source.GetType();
        Type destinationType = destination.GetType();
        for (int i = 0; i < fieldNames.Length; i++)
        {
            FieldInfo sourceField = FindField(sourceType, fieldNames[i]);
            FieldInfo destinationField = FindField(destinationType, fieldNames[i]);
            if (sourceField == null || destinationField == null)
            {
                continue;
            }

            object value = sourceField.GetValue(source);
            destinationField.SetValue(destination, ConvertValue(value, destinationField.FieldType));
        }
    }

    private static FieldInfo FindField(Type type, string fieldName)
    {
        while (type != null)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                return field;
            }

            type = type.BaseType;
        }

        return null;
    }

    private static object ConvertValue(object value, Type destinationType)
    {
        if (value == null)
        {
            return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
        }

        Type sourceType = value.GetType();
        if (destinationType == typeof(PlayerWeaponAttackStep[]) && value is PlayerWeaponAttackStep[] attackSteps)
        {
            return CloneAttackSteps(attackSteps);
        }

        if (destinationType == typeof(PlayerHitSoundRule[]) && value is PlayerHitSoundRule[] hitSoundRules)
        {
            return CloneHitSoundRules(hitSoundRules);
        }

        if (destinationType == typeof(float[]) && value is float[] floats)
        {
            return CloneFloats(floats);
        }

        if (destinationType.IsAssignableFrom(sourceType))
        {
            return value;
        }

        if (sourceType.IsEnum && destinationType.IsEnum)
        {
            return Enum.Parse(destinationType, value.ToString());
        }

        if (destinationType.IsEnum)
        {
            return Enum.Parse(destinationType, value.ToString());
        }

        return value;
    }

    private static PlayerWeaponAttackStep[] CloneAttackSteps(PlayerWeaponAttackStep[] source)
    {
        if (source == null)
        {
            return new PlayerWeaponAttackStep[0];
        }

        PlayerWeaponAttackStep[] clone = new PlayerWeaponAttackStep[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            clone[i] = CloneAttackStep(source[i]);
        }

        return clone;
    }

    private static PlayerHitSoundRule[] CloneHitSoundRules(PlayerHitSoundRule[] source)
    {
        if (source == null)
        {
            return new PlayerHitSoundRule[0];
        }

        PlayerHitSoundRule[] clone = new PlayerHitSoundRule[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            clone[i] = CloneHitSoundRule(source[i]);
        }

        return clone;
    }

    private static float[] CloneFloats(float[] source)
    {
        if (source == null)
        {
            return new float[0];
        }

        float[] clone = new float[source.Length];
        Array.Copy(source, clone, source.Length);
        return clone;
    }

    private static PlayerHitSoundRule CloneHitSoundRule(PlayerHitSoundRule source)
    {
        if (source == null)
        {
            return null;
        }

        PlayerHitSoundRule clone = new PlayerHitSoundRule();
        FieldInfo[] fields = typeof(PlayerHitSoundRule).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < fields.Length; i++)
        {
            fields[i].SetValue(clone, fields[i].GetValue(source));
        }

        return clone;
    }

    private static PlayerWeaponAttackStep CloneAttackStep(PlayerWeaponAttackStep source)
    {
        if (source == null)
        {
            return null;
        }

        PlayerWeaponAttackStep clone = new PlayerWeaponAttackStep(string.Empty, string.Empty, string.Empty, 0, 0f);
        FieldInfo[] fields = typeof(PlayerWeaponAttackStep).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (int i = 0; i < fields.Length; i++)
        {
            fields[i].SetValue(clone, fields[i].GetValue(source));
        }

        return clone;
    }

    private static bool HasDifferentValues<T>(List<T> objects, string[] fieldNames)
    {
        if (objects == null || objects.Count <= 1)
        {
            return false;
        }

        for (int i = 1; i < objects.Count; i++)
        {
            if (HasDifferentValues(objects[0], objects[i], fieldNames))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasDifferentValues(object left, object right, string[] fieldNames)
    {
        if (left == null || right == null)
        {
            return left != right;
        }

        Type leftType = left.GetType();
        Type rightType = right.GetType();
        for (int i = 0; i < fieldNames.Length; i++)
        {
            FieldInfo leftField = FindField(leftType, fieldNames[i]);
            FieldInfo rightField = FindField(rightType, fieldNames[i]);
            if (leftField == null || rightField == null)
            {
                continue;
            }

            object leftValue = leftField.GetValue(left);
            object rightValue = rightField.GetValue(right);
            if (!ValuesEqual(leftValue, rightValue))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasDifferentWeaponValues(List<Transform> roots)
    {
        if (roots == null || roots.Count <= 1)
        {
            return false;
        }

        PlayerWeaponAttackProfile firstProfile = roots[0] != null ? roots[0].GetComponentInChildren<PlayerWeaponAttackProfile>(true) : null;
        PlayerWeaponHitbox firstHitbox = roots[0] != null ? roots[0].GetComponentInChildren<PlayerWeaponHitbox>(true) : null;
        for (int i = 1; i < roots.Count; i++)
        {
            Transform root = roots[i];
            if (root == null)
            {
                continue;
            }

            PlayerWeaponAttackProfile profile = root.GetComponentInChildren<PlayerWeaponAttackProfile>(true);
            PlayerWeaponHitbox hitbox = root.GetComponentInChildren<PlayerWeaponHitbox>(true);
            if (HasDifferentValues(firstProfile, profile, WeaponProfileFields)
                || HasDifferentValues(firstHitbox, hitbox, WeaponHitboxFields))
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasDifferentEnemyHitEffectValues(List<EnemyPatrol3D> enemies)
    {
        if (enemies == null || enemies.Count <= 1)
        {
            return false;
        }

        DamageHitEffect3D first = enemies[0] != null ? enemies[0].GetComponent<DamageHitEffect3D>() : null;
        for (int i = 1; i < enemies.Count; i++)
        {
            DamageHitEffect3D current = enemies[i] != null ? enemies[i].GetComponent<DamageHitEffect3D>() : null;
            if (HasDifferentValues(first, current, EnemyHitEffectFields))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ValuesEqual(object left, object right)
    {
        if (left == null || right == null)
        {
            return left == right;
        }

        if (left is PlayerWeaponAttackStep[] leftSteps && right is PlayerWeaponAttackStep[] rightSteps)
        {
            return AttackStepsEqual(leftSteps, rightSteps);
        }

        if (left is PlayerHitSoundRule[] leftRules && right is PlayerHitSoundRule[] rightRules)
        {
            return HitSoundRulesEqual(leftRules, rightRules);
        }

        if (left is float[] leftFloats && right is float[] rightFloats)
        {
            return FloatArraysEqual(leftFloats, rightFloats);
        }

        return left.Equals(right);
    }

    private static bool FloatArraysEqual(float[] left, float[] right)
    {
        if (left == null || right == null)
        {
            return left == right;
        }

        if (left.Length != right.Length)
        {
            return false;
        }

        for (int i = 0; i < left.Length; i++)
        {
            if (!Mathf.Approximately(left[i], right[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static bool AttackStepsEqual(PlayerWeaponAttackStep[] left, PlayerWeaponAttackStep[] right)
    {
        if (left == null || right == null)
        {
            return left == right;
        }

        if (left.Length != right.Length)
        {
            return false;
        }

        for (int i = 0; i < left.Length; i++)
        {
            if (HasDifferentValues(left[i], right[i], GetAttackStepFieldNames()))
            {
                return false;
            }
        }

        return true;
    }

    private static bool HitSoundRulesEqual(PlayerHitSoundRule[] left, PlayerHitSoundRule[] right)
    {
        if (left == null || right == null)
        {
            return left == right;
        }

        if (left.Length != right.Length)
        {
            return false;
        }

        for (int i = 0; i < left.Length; i++)
        {
            if (HasDifferentValues(left[i], right[i], GetHitSoundRuleFieldNames()))
            {
                return false;
            }
        }

        return true;
    }

    private static string[] GetAttackStepFieldNames()
    {
        FieldInfo[] fields = typeof(PlayerWeaponAttackStep).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        string[] names = new string[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            names[i] = fields[i].Name;
        }

        return names;
    }

    private static string[] GetHitSoundRuleFieldNames()
    {
        FieldInfo[] fields = typeof(PlayerHitSoundRule).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        string[] names = new string[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            names[i] = fields[i].Name;
        }

        return names;
    }

    private static void MarkChanged(Component component)
    {
        if (component == null)
        {
            return;
        }

        EditorUtility.SetDirty(component);
        if (component.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(component.gameObject.scene);
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

    private void DrawGroupedLocalizedProperties()
    {
        DrawSettingsSection("scan", "\u6383\u63cf\u8a2d\u5b9a", true,
            "includeInactiveObjects",
            "enemyNameKeys",
            "playerWeaponRootName");

        DrawSettingsSection("player", "\u73a9\u5bb6\u8a2d\u5b9a", true,
            "playerObjectName",
            "playerMotor",
            "playerCombat");

        DrawSettingsSection("camera", "\u76f8\u6a5f\u8a2d\u5b9a", false,
            "cameraObjectName",
            "cameraShake");

        DrawSettingsSection("enemies", "\u6575\u4eba\u8a2d\u5b9a", true,
            "enemies");

        DrawSettingsSection("weapons", "\u6b66\u5668\u8a2d\u5b9a", true,
            "weapons");
    }

    private void DrawSettingsSection(string key, string title, bool defaultExpanded, params string[] propertyNames)
    {
        if (!DrawFoldoutHeader(key, title, defaultExpanded))
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

            if ((property.name == "enemies" || property.name == "weapons")
                && property.isArray
                && property.propertyType != SerializedPropertyType.String)
            {
                DrawLocalizedArrayContents(property);
                continue;
            }

            DrawLocalizedProperty(property.Copy(), BuildLabel(property));
        }

        EditorGUI.indentLevel--;
    }

    private bool DrawFoldoutHeader(string key, string title, bool defaultExpanded)
    {
        title = ResolveSectionTitle(key, title);
        string prefsKey = FoldoutPrefsPrefix + key;
        bool expanded = SessionState.GetBool(prefsKey, defaultExpanded);

        EditorGUILayout.Space(4f);
        expanded = EditorGUILayout.Foldout(expanded, title, true, EditorStyles.foldoutHeader);
        SessionState.SetBool(prefsKey, expanded);
        return expanded;
    }

    private static string ResolveSectionTitle(string key, string fallback)
    {
        key = NormalizeSectionKey(key);
        switch (key)
        {
            case "syncTools": return "\u540c\u6b65\u5de5\u5177";
            case "scan": return "\u6383\u63cf\u8a2d\u5b9a";
            case "player": return "\u73a9\u5bb6\u8a2d\u5b9a";
            case "camera": return "\u651d\u5f71\u6a5f\u8a2d\u5b9a";
            case "enemies": return "\u6575\u4eba\u8a2d\u5b9a";
            case "weapons": return "\u6b66\u5668\u8a2d\u5b9a";
            case "playerMotor.movement": return "\u79fb\u52d5\u8a2d\u5b9a";
            case "playerMotor.sideScroller": return "\u6a6b\u5411\u5377\u8ef8\u9650\u5236";
            case "playerMotor.jump": return "\u8df3\u8e8d\u8a2d\u5b9a";
            case "playerMotor.dash": return "\u885d\u523a\u8a2d\u5b9a";
            case "playerMotor.dashAfterimage": return "\u885d\u523a\u6b98\u5f71";
            case "playerMotor.animation": return "\u52d5\u756b\u8a2d\u5b9a";
            case "playerMotor.ground": return "\u5730\u9762\u5075\u6e2c";
            case "playerMotor.oneWayPlatform": return "\u55ae\u5411\u8df3\u677f";
            case "playerMotor.wall": return "\u7246\u9762\u6ed1\u843d";
            case "playerMotor.damageKnockback": return "\u53d7\u50b7\u5f48\u98db";
            case "playerCombat.combatIdle": return "\u6230\u9b25\u5f85\u6a5f";
            case "cameraShake.basic": return "\u651d\u5f71\u6a5f\u9707\u52d5";
            case "enemy.type": return "\u6575\u4eba\u985e\u578b";
            case "enemy.patrol": return "\u5de1\u908f";
            case "enemy.detection": return "\u5075\u6e2c";
            case "enemy.melee": return "\u8fd1\u6230\u653b\u64ca";
            case "enemy.ranged": return "\u9060\u7a0b\u653b\u64ca";
            case "enemy.bossContact": return "Boss \u63a5\u89f8\u50b7\u5bb3";
            case "enemy.bossRanged": return "Boss \u9060\u7a0b\u653b\u64ca";
            case "enemy.attackTiming": return "\u653b\u64ca\u6642\u9593";
            case "enemy.hitEffect": return "\u53d7\u64ca\u7279\u6548";
            case "enemy.death": return "\u6b7b\u4ea1";
            case "enemy.damageKnockback": return "\u53d7\u50b7\u64ca\u9000";
            case "enemy.respawn": return "\u91cd\u751f";
            case "weapon.basic": return "\u6b66\u5668\u8cc7\u8a0a";
            case "weapon.profile": return "\u653b\u64ca\u8a2d\u5b9a";
            case "weapon.hitbox": return "\u6b66\u5668\u5224\u5b9a";
            case "weaponProfile.behavior": return "\u653b\u64ca\u884c\u70ba";
            case "weaponProfile.targets": return "\u76ee\u6a19";
            case "weaponProfile.combo": return "\u9023\u6bb5\u653b\u64ca";
            case "weaponProfile.audio": return "\u547d\u4e2d\u97f3\u6548";
            case "weaponHitbox.basic": return "\u6b66\u5668\u5224\u5b9a";
            case "weaponHitbox.reflect": return "\u706b\u7403\u53cd\u64ca";
        }

        if (!string.IsNullOrEmpty(key) && key.EndsWith(".other", StringComparison.Ordinal))
        {
            return "\u5176\u4ed6";
        }

        return fallback;
    }

    private static string NormalizeSectionKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return key;
        }

        string[] knownKeys =
        {
            "playerMotor.movement",
            "playerMotor.sideScroller",
            "playerMotor.jump",
            "playerMotor.dash",
            "playerMotor.dashAfterimage",
            "playerMotor.animation",
            "playerMotor.ground",
            "playerMotor.oneWayPlatform",
            "playerMotor.wall",
            "playerMotor.damageKnockback",
            "playerCombat.combatIdle",
            "cameraShake.basic",
            "enemy.type",
            "enemy.patrol",
            "enemy.detection",
            "enemy.melee",
            "enemy.ranged",
            "enemy.bossContact",
            "enemy.bossRanged",
            "enemy.attackTiming",
            "enemy.hitEffect",
            "enemy.death",
            "enemy.damageKnockback",
            "enemy.respawn",
            "weapon.basic",
            "weapon.profile",
            "weapon.hitbox",
            "weaponProfile.behavior",
            "weaponProfile.targets",
            "weaponProfile.combo",
            "weaponProfile.audio",
            "weaponHitbox.basic",
            "weaponHitbox.reflect"
        };

        for (int i = 0; i < knownKeys.Length; i++)
        {
            if (key.EndsWith(knownKeys[i], StringComparison.Ordinal))
            {
                return knownKeys[i];
            }
        }

        return key;
    }

    private void DrawLocalizedProperties()
    {
        SerializedProperty property = serializedObject.GetIterator();
        bool enterChildren = true;
        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (property.propertyPath == "m_Script")
            {
                continue;
            }

            DrawLocalizedProperty(property.Copy(), BuildLabel(property));
        }
    }

    private void DrawLocalizedProperty(SerializedProperty property, GUIContent label)
    {
        if (property.isArray && property.propertyType != SerializedPropertyType.String)
        {
            DrawLocalizedArray(property, label);
            return;
        }

        if (property.propertyType == SerializedPropertyType.Generic)
        {
            DrawLocalizedGeneric(property, label);
            return;
        }

        EditorGUILayout.PropertyField(property, label, false);
    }

    private void DrawLocalizedGeneric(SerializedProperty property, GUIContent label)
    {
        if (TryDrawKnownGroupedGeneric(property, label))
        {
            return;
        }

        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
        if (!property.isExpanded)
        {
            return;
        }

        EditorGUI.indentLevel++;
        SerializedProperty endProperty = property.GetEndProperty();
        SerializedProperty child = property.Copy();
        bool enterChildren = true;
        while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, endProperty))
        {
            enterChildren = false;
            DrawLocalizedProperty(child.Copy(), BuildLabel(child));
        }

        EditorGUI.indentLevel--;
    }

    private bool TryDrawKnownGroupedGeneric(SerializedProperty property, GUIContent label)
    {
        if (property.name == "playerMotor")
        {
            DrawPlayerMotorTuning(property, label);
            return true;
        }

        if (property.name == "playerCombat")
        {
            DrawPlayerCombatTuning(property, label);
            return true;
        }

        if (property.name == "cameraShake")
        {
            DrawCameraShakeTuning(property, label);
            return true;
        }

        if (property.name == "attackProfile")
        {
            DrawWeaponProfileTuning(property, label);
            return true;
        }

        if (property.name == "hitbox")
        {
            DrawWeaponHitboxTuning(property, label);
            return true;
        }

        if (property.name == "hitEffect")
        {
            DrawEnemyHitEffectTuning(property, label);
            return true;
        }

        if (IsAttackStepArrayElement(property))
        {
            EditorGUILayout.PropertyField(property, label, true);
            return true;
        }

        if (IsDirectArrayElement(property, "enemies"))
        {
            DrawEnemyTuningEntry(property, label);
            return true;
        }

        if (IsDirectArrayElement(property, "weapons"))
        {
            DrawWeaponTuningEntry(property, label);
            return true;
        }

        return false;
    }

    private static bool IsDirectArrayElement(SerializedProperty property, string arrayName)
    {
        if (property == null || string.IsNullOrEmpty(arrayName))
        {
            return false;
        }

        string prefix = arrayName + ".Array.data[";
        if (!property.propertyPath.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        int closingBracket = property.propertyPath.IndexOf(']', prefix.Length);
        return closingBracket >= 0 && closingBracket == property.propertyPath.Length - 1;
    }

    private static bool IsAttackStepArrayElement(SerializedProperty property)
    {
        if (property == null)
        {
            return false;
        }

        const string marker = ".attacks.Array.data[";
        int markerIndex = property.propertyPath.LastIndexOf(marker, StringComparison.Ordinal);
        if (markerIndex < 0)
        {
            return false;
        }

        int closingBracket = property.propertyPath.IndexOf(']', markerIndex + marker.Length);
        return closingBracket >= 0 && closingBracket == property.propertyPath.Length - 1;
    }

    private void DrawPlayerMotorTuning(SerializedProperty property, GUIContent label)
    {
        DrawGroupedGeneric(property, label, new[]
        {
            new NestedSection("playerMotor.movement", "\u79fb\u52d5\u8a2d\u5b9a", true, new[] { "sync", "movementMode", "moveSpeed", "airControl", "useCameraRelativeMovement", "freeTurnSpeed" }),
            new NestedSection("playerMotor.sideScroller", "\u6a6b\u5411\u5377\u8ef8\u9650\u5236", false, new[] { "lockedZ", "movementAxis" }),
            new NestedSection("playerMotor.jump", "\u8df3\u8e8d\u8a2d\u5b9a", true, new[] { "jumpForce", "upwardGravityMultiplier", "fallGravityMultiplier", "maxFallSpeed", "jumpBufferSeconds", "coyoteTimeSeconds", "extraAirJumps", "airJumpForceMultiplier" }),
            new NestedSection("playerMotor.dash", "\u885d\u523a\u8a2d\u5b9a", true, new[] { "dashDistance", "dashDuration", "dashCooldown", "allowAirDash", "flattenVerticalVelocityDuringDash", "dashJumpHorizontalMultiplier", "dashJumpDashSpeedCarryMultiplier", "dashJumpBoostSeconds", "airDashAnimationMinSeconds", "dashEndAnimationMinSeconds" }),
            new NestedSection("playerMotor.dashAfterimage", "\u885d\u523a\u6b98\u5f71", false, new[] { "enableDashAfterimage", "dashAfterimageSpawnInterval", "dashAfterimageLifetime", "dashAfterimageColor", "dashAfterimageIncludeMeshRenderers", "dashAfterimageIncludeInactiveRenderers" }),
            new NestedSection("playerMotor.animation", "\u52d5\u756b\u8a2d\u5b9a", true, new[] { "actionAnimationCrossFadeSeconds", "jumpAnimationCrossFadeSeconds" }),
            new NestedSection("playerMotor.ground", "\u5730\u9762\u5075\u6e2c", false, new[] { "groundCheckLocalOffset", "groundCheckRadius", "groundMask", "groundFallbackDistance", "useAnySolidGroundFallback" }),
            new NestedSection("playerMotor.oneWayPlatform", "\u55ae\u5411\u8df3\u677f", false, new[] { "enableOneWayPlatforms", "dropThroughSeconds", "dropThroughStartSpeed", "dropThroughPlatformSearchDistance", "dropInputThreshold", "oneWayPlatformPrecheckHeight", "oneWayPlatformPrecheckPadding" }),
            new NestedSection("playerMotor.wall", "\u7246\u9762\u6ed1\u843d", false, new[] { "preventAirWallSticking", "wallNormalThreshold", "wallContactGraceSeconds", "useNoFrictionColliderMaterial" }),
            new NestedSection("playerMotor.damageKnockback", "\u53d7\u50b7\u5f48\u98db", false, new[] { "enableDamageKnockback", "knockbackForce", "knockbackControlLockSeconds", "lockControlUntilKnockbackLands" })
        });
    }

    private void DrawPlayerCombatTuning(SerializedProperty property, GUIContent label)
    {
        DrawGroupedGeneric(property, label, new[]
        {
            new NestedSection("playerCombat.combatIdle", "Combat Idle", true, new[] { "sync", "combatDetectionRange", "combatVerticalRange", "combatMemorySeconds" })
        });
    }

    private void DrawCameraShakeTuning(SerializedProperty property, GUIContent label)
    {
        DrawGroupedGeneric(property, label, new[]
        {
            new NestedSection("cameraShake.basic", "Camera Shake", true, new[] { "sync", "defaultAmplitude", "defaultDuration", "defaultFrequency", "useUnscaledTime" })
        });
    }

    private void DrawEnemyTuningEntry(SerializedProperty property, GUIContent label)
    {
        DrawGroupedGeneric(property, label, new[]
        {
            new NestedSection("enemy.type", "\u6575\u4eba\u985e\u578b", true, new[] { "sync", "enemyNameKey", "sceneObjectCount", "valuesDifferInScene", "attackMode" }),
            new NestedSection("enemy.patrol", "\u5de1\u908f", true, new[] { "movementMode", "useTransformRightAsMovementAxis", "movementAxis", "lockDepthToMovementPlane", "moveSpeed", "patrolMoveSpeed", "homeStopDistance", "fallbackPatrolHalfWidth", "patrolRadius", "patrolDestinationReachDistance", "patrolDestinationMinDistance", "patrolObstacleMask", "usePatrolObstacleMask", "patrolObstacleCheckDistance", "patrolObstacleRayHeights" }),
            new NestedSection("enemy.detection", "\u5075\u6e2c", true, new[] { "searchRange", "giveUpRange", "detectionBoxOffset", "detectionBoxHeight", "detectionBoxDepth", "giveUpBoxPadding", "showDetectionBoxGizmo", "onlyShowDetectionBoxWhenSelected" }),
            new NestedSection("enemy.melee", "\u8fd1\u6230\u653b\u64ca", true, new[] { "attackRange", "meleeAttackHeight", "attackDamage", "meleeHitSoundVolume" }),
            new NestedSection("enemy.ranged", "\u9060\u7a0b\u653b\u64ca", false, new[] { "projectileDamage", "projectileSpeed", "projectileLifetime", "projectileHitSoundVolume", "projectileLocalOffset", "returnSpeed" }),
            new NestedSection("enemy.bossContact", "Boss \u63a5\u89f8\u50b7\u5bb3", false, new[] { "bossContactDamageEnabled", "bossContactDamage", "bossContactDamageCooldown", "bossContactDamageBoxSize", "bossContactDamageBoxCenter", "bossContactDamageTargetMask" }),
            new NestedSection("enemy.bossRanged", "Boss \u9060\u7a0b\u653b\u64ca", false, new[] { "bossRangedDistance", "bossRangedDistanceTolerance" }),
            new NestedSection("enemy.attackTiming", "\u653b\u64ca\u6642\u6a5f", false, new[] { "attackCooldown", "useRangedAttackRhythm", "rangedAttackRhythm", "rangedAttackGroupCooldown", "attackWindup", "attackLockSeconds" }),
            new NestedSection("enemy.hitEffect", "\u53d7\u64ca\u7279\u6548", false, new[] { "hitEffect" }),
            new NestedSection("enemy.death", "\u6b7b\u4ea1", false, new[] { "launchAwayOnDeath", "deathLaunchSpeed", "deathLaunchUpSpeed", "deathSpinDegreesPerSecond", "deathDestroyDelay" }),
            new NestedSection("enemy.damageKnockback", "\u53d7\u50b7\u64ca\u9000", false, new[] { "knockbackOnDamage", "damageKnockbackForce", "damageKnockbackLockSeconds", "airborneHitPauseNormalizedTime", "damageLandingRecoverySeconds", "damageGroundCheckDistance", "damageGroundMask" }),
            new NestedSection("enemy.respawn", "\u91cd\u751f", false, new[] { "respawnAfterCameraLeaves", "respawnCameraAwaySeconds", "respawnViewportPadding" })
        });
    }

    private void DrawEnemyHitEffectTuning(SerializedProperty property, GUIContent label)
    {
        DrawGroupedGeneric(property, label, new[]
        {
            new NestedSection("enemy.hitEffect", "\u53d7\u64ca\u7279\u6548", true, new[] { "sync", "effectPrefab", "effectAnchorName", "stopEffectOnAwake" })
        });
    }

    private void DrawWeaponTuningEntry(SerializedProperty property, GUIContent label)
    {
        DrawGroupedGeneric(property, label, new[]
        {
            new NestedSection("weapon.basic", "\u6b66\u5668", true, new[] { "sync", "weaponNameKey", "sceneObjectCount", "valuesDifferInScene" }),
            new NestedSection("weapon.profile", "\u653b\u64ca\u8a2d\u5b9a", true, new[] { "attackProfile" }),
            new NestedSection("weapon.hitbox", "\u6b66\u5668\u5224\u5b9a", false, new[] { "hitbox" })
        });
    }

    private void DrawWeaponProfileTuning(SerializedProperty property, GUIContent label)
    {
        DrawGroupedGeneric(property, label, new[]
        {
            new NestedSection("weaponProfile.behavior", "\u653b\u64ca\u884c\u70ba", true, new[] { "sync", "attackCooldown", "attackMoveLockSeconds", "useAttackAnimationLength", "attackSpeedMultiplier", "attackCrossFadeSeconds", "allowAirAttacks" }),
            new NestedSection("weaponProfile.targets", "\u76ee\u6a19", false, new[] { "targetMask" }),
            new NestedSection("weaponProfile.combo", "\u9023\u6bb5\u653b\u64ca", true, new[] { "attacks" }),
            new NestedSection("weaponProfile.audio", "\u547d\u4e2d\u97f3\u6548", false, new[] { "attackHitSound", "attackHitSoundVolume", "targetHitSounds" })
        });
    }

    private void DrawWeaponHitboxTuning(SerializedProperty property, GUIContent label)
    {
        DrawGroupedGeneric(property, label, new[]
        {
            new NestedSection("weaponHitbox.basic", "\u6b66\u5668\u5224\u5b9a", true, new[] { "sync", "weaponSize", "weaponColor", "weaponModelRoot", "useModelBoundsForHitbox", "updateColliderDuringPlay", "modelBoundsPadding" }),
            new NestedSection("weaponHitbox.reflect", "\u6295\u5c04\u7269\u53cd\u5f48", false, new[] { "projectileReflectExtraRange", "projectileReflectEffectPrefab", "projectileReflectEffectScale", "projectileReflectEffectFallbackLifetime" })
        });
    }

    private void DrawGroupedGeneric(SerializedProperty property, GUIContent label, NestedSection[] sections)
    {
        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
        if (!property.isExpanded)
        {
            return;
        }

        HashSet<string> drawnNames = new HashSet<string>();
        EditorGUI.indentLevel++;
        for (int i = 0; i < sections.Length; i++)
        {
            DrawNestedSection(property, sections[i], drawnNames);
        }

        DrawRemainingGenericChildren(property, drawnNames);
        EditorGUI.indentLevel--;
    }

    private void DrawNestedSection(SerializedProperty parent, NestedSection section, HashSet<string> drawnNames)
    {
        if (!DrawFoldoutHeader(parent.propertyPath + "." + section.key, section.title, section.defaultExpanded))
        {
            MarkNestedSectionFields(parent, section, drawnNames);
            return;
        }

        EditorGUI.indentLevel++;
        for (int i = 0; i < section.propertyNames.Length; i++)
        {
            SerializedProperty child = parent.FindPropertyRelative(section.propertyNames[i]);
            if (child == null)
            {
                continue;
            }

            drawnNames.Add(child.name);
            DrawLocalizedProperty(child.Copy(), BuildLabel(child));
        }

        EditorGUI.indentLevel--;
    }

    private void MarkNestedSectionFields(SerializedProperty parent, NestedSection section, HashSet<string> drawnNames)
    {
        for (int i = 0; i < section.propertyNames.Length; i++)
        {
            SerializedProperty child = parent.FindPropertyRelative(section.propertyNames[i]);
            if (child != null)
            {
                drawnNames.Add(child.name);
            }
        }
    }

    private void DrawRemainingGenericChildren(SerializedProperty parent, HashSet<string> drawnNames)
    {
        SerializedProperty endProperty = parent.GetEndProperty();
        SerializedProperty child = parent.Copy();
        bool enterChildren = true;
        bool drewHeader = false;

        while (child.NextVisible(enterChildren) && !SerializedProperty.EqualContents(child, endProperty))
        {
            enterChildren = false;
            if (drawnNames.Contains(child.name))
            {
                continue;
            }

            if (!drewHeader)
            {
                if (!DrawFoldoutHeader(parent.propertyPath + ".other", "\u5176\u4ed6", false))
                {
                    return;
                }

                EditorGUI.indentLevel++;
                drewHeader = true;
            }

            DrawLocalizedProperty(child.Copy(), BuildLabel(child));
        }

        if (drewHeader)
        {
            EditorGUI.indentLevel--;
        }
    }

    private void DrawLocalizedArray(SerializedProperty property, GUIContent label)
    {
        property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
        if (!property.isExpanded)
        {
            return;
        }

        DrawLocalizedArrayContents(property);
    }

    private void DrawLocalizedArrayContents(SerializedProperty property)
    {
        EditorGUI.indentLevel++;
        SerializedProperty size = property.FindPropertyRelative("Array.size");
        if (size != null)
        {
            EditorGUILayout.PropertyField(size, new GUIContent("\u6578\u91cf"));
        }

        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(i);
            if (element == null)
            {
                continue;
            }

            DrawLocalizedProperty(element.Copy(), BuildArrayElementLabel(property, element, i));
        }

        EditorGUI.indentLevel--;
    }

    private GUIContent BuildArrayElementLabel(SerializedProperty arrayProperty, SerializedProperty element, int index)
    {
        string text = "\u5143\u7d20 " + index;
        if (arrayProperty.name == "enemies")
        {
            SerializedProperty name = element.FindPropertyRelative("enemyNameKey");
            text = name != null && !string.IsNullOrWhiteSpace(name.stringValue)
                ? name.stringValue
                : "\u6575\u4eba " + (index + 1);
        }
        else if (arrayProperty.name == "weapons")
        {
            SerializedProperty name = element.FindPropertyRelative("weaponNameKey");
            text = name != null && !string.IsNullOrWhiteSpace(name.stringValue)
                ? name.stringValue
                : "\u6b66\u5668 " + (index + 1);
        }
        else if (arrayProperty.name == "attacks")
        {
            text = "\u653b\u64ca " + (index + 1);
        }
        else if (arrayProperty.name == "targetHitSounds")
        {
            text = "\u547d\u4e2d\u97f3\u6548\u898f\u5247 " + (index + 1);
        }
        else if (arrayProperty.name == "enemyNameKeys")
        {
            text = "\u6575\u4eba\u540d\u7a31 " + (index + 1);
        }

        return new GUIContent(text);
    }

    private GUIContent BuildLabel(SerializedProperty property)
    {
        string text = ResolveChineseLabel(property.name);
        string tooltip = ResolveChineseTooltip(property.name);
        return new GUIContent(text, tooltip);
    }

    private static string ResolveChineseLabel(string propertyName)
    {
        string label = SideScrollerInspectorLabels.Text(propertyName, string.Empty);
        if (!string.IsNullOrEmpty(label))
        {
            return label;
        }

        switch (propertyName)
        {
            case "includeInactiveObjects": return "\u8b80\u53d6\u505c\u7528\u4e2d\u7684\u7269\u4ef6";
            case "enemyNameKeys": return "\u6575\u4eba\u540d\u7a31\u6e05\u55ae";
            case "playerWeaponRootName": return "\u6b66\u5668\u6839\u7269\u4ef6\u540d\u7a31";
            case "playerObjectName": return "\u73a9\u5bb6\u7269\u4ef6\u540d\u7a31";
            case "playerMotor": return "\u73a9\u5bb6\u79fb\u52d5\u6578\u503c";
            case "playerCombat": return "\u73a9\u5bb6\u6230\u9b25\u6578\u503c";
            case "cameraObjectName": return "\u651d\u5f71\u6a5f\u7269\u4ef6\u540d\u7a31";
            case "cameraShake": return "\u651d\u5f71\u6a5f\u9707\u52d5\u6578\u503c";
            case "enemies": return "\u6575\u4eba\u6578\u503c";
            case "weapons": return "\u6b66\u5668\u6578\u503c";
            case "sync": return "\u540c\u6b65\u9019\u7d44\u6578\u503c";
            case "enemyNameKey": return "\u6575\u4eba\u540d\u7a31";
            case "weaponNameKey": return "\u6b66\u5668\u540d\u7a31";
            case "sceneObjectCount": return "\u8b80\u53d6\u5230\u7684\u6578\u91cf";
            case "valuesDifferInScene": return "\u5834\u666f\u4e2d\u6578\u503c\u4e0d\u4e00\u81f4";
            case "attackProfile": return "\u653b\u64ca\u8a2d\u5b9a";
            case "hitbox": return "\u6b66\u5668\u5224\u5b9a";
            case "hitEffect": return "\u53d7\u64ca\u7279\u6548";
            case "attacks": return "\u653b\u64ca\u6bb5\u6578\u8a2d\u5b9a";
            case "animatorStateName": return "Animator \u72c0\u614b\u540d\u7a31";
            case "animationClip": return "\u653b\u64ca\u52d5\u756b";
            case "animationClipName": return "\u653b\u64ca\u52d5\u756b\u540d\u7a31";
            case "triggerName": return "\u9023\u6bb5\u89f8\u767c Trigger";
            case "nextInputWindowSeconds": return "\u4e0b\u4e00\u6bb5\u8f38\u5165\u6642\u9593\u7a97";
            case "nextAttackStartFrame": return "\u4e0b\u4e00\u6bb5\u5207\u63db\u5e40";
            case "attackEffectRoot": return "\u653b\u64ca\u7279\u6548\u6839\u7269\u4ef6";
            case "useRangedAttackRhythm": return "\u4f7f\u7528\u9060\u7a0b\u653b\u64ca\u7bc0\u594f";
            case "rangedAttackRhythm": return "\u653b\u64ca\u7bc0\u594f";
            case "rangedAttackGroupCooldown": return "\u6bcf\u7d44\u653b\u64caCD\u6642\u9593";
            case "cameraShakeAmplitude": return "\u547d\u4e2d\u9707\u52d5\u5f37\u5ea6";
            case "cameraShakeDuration": return "\u547d\u4e2d\u9707\u52d5\u6642\u9593";
            case "cameraShakeFrequency": return "\u547d\u4e2d\u9707\u52d5\u983b\u7387";
            case "jumpAnimationCrossFadeSeconds": return "\u8df3\u8e8d\u52d5\u756b\u6de1\u5165\u79d2\u6578";
            case "attackMoveLockSeconds": return "\u653b\u64ca\u79fb\u52d5\u9396\u5b9a\u79d2\u6578";
            case "useAttackAnimationLength": return "\u4f7f\u7528\u653b\u64ca\u52d5\u756b\u9577\u5ea6";
            case "attackSpeedMultiplier": return "\u653b\u64ca\u901f\u5ea6\u500d\u7387";
            case "attackCrossFadeSeconds": return "\u653b\u64ca\u52d5\u756b\u6de1\u5165\u79d2\u6578";
            case "allowAirAttacks": return "\u5141\u8a31\u7a7a\u4e2d\u653b\u64ca";
            case "attackHitSound": return "\u9810\u8a2d\u547d\u4e2d\u97f3\u6548";
            case "attackHitSoundVolume": return "\u547d\u4e2d\u97f3\u6548\u97f3\u91cf";
            case "targetHitSounds": return "\u76ee\u6a19\u547d\u4e2d\u97f3\u6548\u898f\u5247";
            case "targetNameContains": return "\u76ee\u6a19\u540d\u7a31\u5305\u542b";
            case "targetTag": return "\u76ee\u6a19 Tag";
            case "targetLayers": return "\u76ee\u6a19\u5716\u5c64";
            case "hitSound": return "\u547d\u4e2d\u97f3\u6548";
            case "volume": return "\u97f3\u91cf";
            default: return ObjectNames.NicifyVariableName(propertyName);
        }
    }

    private static string ResolveChineseTooltip(string propertyName)
    {
        switch (propertyName)
        {
            case "valuesDifferInScene":
                return "\u540c\u540d\u6575\u4eba\u6216\u540c\u540d\u6b66\u5668\u5728\u5834\u666f\u4e2d\u7684\u6578\u503c\u4e0d\u5b8c\u5168\u76f8\u540c\u3002\u8b80\u53d6\u6642\u6703\u5148\u7528\u7b2c\u4e00\u500b\u7269\u4ef6\u7684\u6578\u503c\u3002";
            case "sceneObjectCount":
                return "\u76ee\u524d\u5834\u666f\u4e2d\u7b26\u5408\u9019\u500b\u540d\u7a31\u5206\u7d44\u7684\u7269\u4ef6\u6578\u91cf\u3002";
            case "includeInactiveObjects":
                return "\u958b\u555f\u5f8c\uff0c\u505c\u7528\u4e2d\u7684\u6575\u4eba\u3001\u6b66\u5668\u3001\u76f8\u6a5f\u4e5f\u6703\u88ab\u8b80\u53d6\u8207\u540c\u6b65\u3002";
            case "playerWeaponRootName":
                return "\u5de5\u5177\u6703\u5c0b\u627e\u9019\u500b\u540d\u7a31\u7684\u7269\u4ef6\uff0c\u4e26\u6383\u63cf\u5b83\u5e95\u4e0b\u6240\u6709\u5b50\u5c64\u4e2d\u7684\u6b66\u5668\u8173\u672c\u3002";
            default:
                return string.Empty;
        }
    }

    private class NestedSection
    {
        public readonly string key;
        public readonly string title;
        public readonly bool defaultExpanded;
        public readonly string[] propertyNames;

        public NestedSection(string key, string title, bool defaultExpanded, string[] propertyNames)
        {
            this.key = key;
            this.title = title;
            this.defaultExpanded = defaultExpanded;
            this.propertyNames = propertyNames;
        }
    }
}
