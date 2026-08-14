using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class EnemyContactDamageInstaller
{
    private static readonly string[] EnemyPrefabPaths =
    {
        "Assets/Art/Prefab/Enemy_A.prefab",
        "Assets/Art/Prefab/Enemy_B.prefab",
        "Assets/Art/Prefab/Boss.prefab"
    };

    private const string ScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";
    private const string ContactDamageName = "Enemy_ContactDamage";

    public static void Install()
    {
        foreach (string path in EnemyPrefabPaths)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            EnsureContactDamage(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            Debug.Log($"Enemy contact damage installed: {path}");
        }

        var scene = EditorSceneManager.OpenScene(ScenePath);
        foreach (GameObject enemy in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (enemy.layer == LayerMask.NameToLayer("Enemy") && enemy.transform.parent == null)
            {
                EnsureContactDamage(enemy);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureContactDamage(GameObject enemyRoot)
    {
        Transform existing = enemyRoot.transform.Find(ContactDamageName);
        GameObject contactObject = existing != null ? existing.gameObject : new GameObject(ContactDamageName);
        contactObject.layer = LayerMask.NameToLayer("Default");
        contactObject.transform.SetParent(enemyRoot.transform, false);
        contactObject.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        contactObject.transform.localRotation = Quaternion.identity;
        contactObject.transform.localScale = new Vector3(1.15f, 1.8f, 0.9f);

        BoxCollider collider = contactObject.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = contactObject.AddComponent<BoxCollider>();
        }

        collider.isTrigger = true;
        collider.size = Vector3.one;
        collider.center = Vector3.zero;

        DamageOnTouch damage = contactObject.GetComponent<DamageOnTouch>();
        if (damage == null)
        {
            damage = contactObject.AddComponent<DamageOnTouch>();
        }

        SerializedObject serializedDamage = new SerializedObject(damage);
        serializedDamage.FindProperty("contactDamageEnabled").boolValue = true;
        serializedDamage.FindProperty("damage").intValue = 1;
        serializedDamage.FindProperty("targetMask").intValue = 1 << LayerMask.NameToLayer("Player");
        serializedDamage.FindProperty("contactDamageCooldown").floatValue = 0.8f;
        serializedDamage.ApplyModifiedPropertiesWithoutUndo();

        EditorUtility.SetDirty(contactObject);
        EditorUtility.SetDirty(enemyRoot);
    }
}
