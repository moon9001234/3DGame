using UnityEditor;
using UnityEngine;

public static class SideScrollerPresetTools
{
    private const string PlayerGroundCheckName = "GroundCheck";
    private const string PlayerWeaponAnchorName = "WeaponAnchor";
    private const string EnemyHurtboxName = "Enemy_Hurtbox";
    private const string EnemyHitEffectName = "EF_Hit";

    [MenuItem("Tools/3D \u904a\u6232\u5de5\u5177/\u639b\u4e0a\u57fa\u790e\u8173\u672c/\u89d2\u8272")]
    public static void ApplyPlayerPreset()
    {
        ApplyToSelection("\u639b\u4e0a\u89d2\u8272\u57fa\u790e\u8173\u672c", ApplyPlayerPresetToObject);
    }

    [MenuItem("Tools/3D \u904a\u6232\u5de5\u5177/\u639b\u4e0a\u57fa\u790e\u8173\u672c/\u6575\u4eba")]
    public static void ApplyEnemyPreset()
    {
        ApplyToSelection("\u639b\u4e0a\u6575\u4eba\u57fa\u790e\u8173\u672c", ApplyEnemyPresetToObject);
    }

    [MenuItem("Tools/3D \u904a\u6232\u5de5\u5177/\u639b\u4e0a\u57fa\u790e\u8173\u672c/\u651d\u5f71\u6a5f")]
    public static void ApplyCameraPreset()
    {
        ApplyToSelection("\u639b\u4e0a\u651d\u5f71\u6a5f\u57fa\u790e\u8173\u672c", ApplyCameraPresetToObject);
    }

    private static void ApplyToSelection(string undoLabel, System.Action<GameObject> applyPreset)
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection == null || selection.Length == 0)
        {
            Debug.LogWarning("\u8acb\u5148\u5728 Hierarchy \u9078\u53d6\u81f3\u5c11\u4e00\u500b\u7269\u4ef6\u3002");
            return;
        }

        for (int i = 0; i < selection.Length; i++)
        {
            GameObject target = selection[i];
            if (target == null || EditorUtility.IsPersistent(target))
            {
                continue;
            }

            Undo.RegisterFullObjectHierarchyUndo(target, undoLabel);
            applyPreset(target);
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            EditorUtility.SetDirty(target);
        }
    }

    private static void ApplyPlayerPresetToObject(GameObject root)
    {
        AddComponentIfMissing<Health>(root);
        AddComponentIfMissing<PlayerDamageFlash>(root);
        AddComponentIfMissing<PlayerMotor3D>(root);
        AddComponentIfMissing<PlayerCombat3D>(root);
        AddComponentIfMissing<SideScrollerPlayerRespawn>(root);

        Transform groundCheck = EnsureChild(root.transform, PlayerGroundCheckName);
        if (groundCheck != null)
        {
            SetLocalTransform(groundCheck, new Vector3(0f, -1.05f, 0f), Quaternion.identity, Vector3.one);
        }

        Transform weaponAnchor = EnsureChild(root.transform, PlayerWeaponAnchorName);
        if (weaponAnchor != null)
        {
            SetLocalTransform(weaponAnchor, new Vector3(0.45f, 0.25f, 0f), Quaternion.identity, Vector3.one);
            AddComponentIfMissing<PlayerWeaponHitbox>(weaponAnchor.gameObject);
            AddComponentIfMissing<PlayerWeaponAttackProfile>(weaponAnchor.gameObject);
        }
    }

    private static void ApplyEnemyPresetToObject(GameObject root)
    {
        AddComponentIfMissing<Health>(root);
        AddComponentIfMissing<EnemyDamageFlash>(root);
        AddComponentIfMissing<EnemyPatrol3D>(root);
        AddComponentIfMissing<EnemyVisualAnimator>(root);
        AddComponentIfMissing<EnemyHealthBar3D>(root);
        DamageHitEffect3D hitEffect = AddComponentIfMissing<DamageHitEffect3D>(root);

        Transform hurtbox = EnsureChild(root.transform, EnemyHurtboxName);
        if (hurtbox != null)
        {
            SetLocalTransform(hurtbox, new Vector3(0f, 0.05f, 0f), Quaternion.identity, new Vector3(1.15f, 1.8f, 0.9f));
            AddComponentIfMissing<EnemyHurtbox3D>(hurtbox.gameObject);
        }

        Transform effectAnchor = EnsureChild(root.transform, EnemyHitEffectName);
        if (effectAnchor != null)
        {
            SetLocalTransform(effectAnchor, new Vector3(0f, 0.95f, 0f), Quaternion.identity, Vector3.one);
            if (hitEffect != null)
            {
                hitEffect.SetEffectAnchor(effectAnchor);
                EditorUtility.SetDirty(hitEffect);
            }
        }
    }

    private static void ApplyCameraPresetToObject(GameObject root)
    {
        AddComponentIfMissing<Camera>(root);
        AddComponentIfMissing<SideScrollerCamera>(root);
    }

    private static T AddComponentIfMissing<T>(GameObject target) where T : Component
    {
        if (target == null)
        {
            return null;
        }

        if (!target.TryGetComponent(out T component))
        {
            component = Undo.AddComponent<T>(target);
        }

        return component;
    }

    private static Transform EnsureChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        Transform child = root.Find(childName);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(childName);
        Undo.RegisterCreatedObjectUndo(childObject, "Create " + childName);
        Undo.SetTransformParent(childObject.transform, root, "Parent " + childName);
        childObject.transform.localPosition = Vector3.zero;
        childObject.transform.localRotation = Quaternion.identity;
        childObject.transform.localScale = Vector3.one;
        return childObject.transform;
    }

    private static void SetLocalTransform(Transform target, Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
    {
        if (target == null)
        {
            return;
        }

        target.localPosition = localPosition;
        target.localRotation = localRotation;
        target.localScale = localScale;
    }
}
