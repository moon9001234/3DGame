using System.IO;
using UnityEditor;
using UnityEngine;

public static class SideScrollerDefinitionAssetUtility
{
    private const string DataFolder = "Assets/SideScroller3D/Data";

    public static EnemyDefinition3D CreateEnemyDefinition(EnemyPatrol3D source)
    {
        EnsureDataFolder();

        string assetName = BuildAssetName(source != null ? source.gameObject.name : "Enemy");
        string path = AssetDatabase.GenerateUniqueAssetPath($"{DataFolder}/{assetName}_Definition.asset");

        EnemyDefinition3D definition = ScriptableObject.CreateInstance<EnemyDefinition3D>();
        if (source != null)
        {
            source.SaveToDefinition(definition);
        }

        AssetDatabase.CreateAsset(definition, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return definition;
    }

    public static WeaponDefinition3D CreateWeaponDefinition(PlayerWeaponAttackProfile profile)
    {
        EnsureDataFolder();

        string assetName = BuildAssetName(profile != null ? profile.gameObject.name : "Weapon");
        string path = AssetDatabase.GenerateUniqueAssetPath($"{DataFolder}/{assetName}_Definition.asset");

        WeaponDefinition3D definition = ScriptableObject.CreateInstance<WeaponDefinition3D>();
        if (profile != null)
        {
            profile.SaveToDefinition(definition);
            PlayerWeaponHitbox hitbox = profile.GetComponent<PlayerWeaponHitbox>();
            if (hitbox != null)
            {
                hitbox.SaveToDefinition(definition);
            }
        }

        AssetDatabase.CreateAsset(definition, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return definition;
    }

    private static void EnsureDataFolder()
    {
        if (AssetDatabase.IsValidFolder(DataFolder))
        {
            return;
        }

        if (!AssetDatabase.IsValidFolder("Assets/SideScroller3D"))
        {
            AssetDatabase.CreateFolder("Assets", "SideScroller3D");
        }

        AssetDatabase.CreateFolder("Assets/SideScroller3D", "Data");
    }

    private static string BuildAssetName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return "Definition";
        }

        string fileName = rawName.Trim();
        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            fileName = fileName.Replace(invalid.ToString(), string.Empty);
        }

        return string.IsNullOrWhiteSpace(fileName) ? "Definition" : fileName;
    }
}
