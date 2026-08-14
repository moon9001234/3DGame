using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SideScrollerBackgroundBuilder
{
    private const string ScenePath = "Assets/SideScroller3D/Scenes/Prototype.unity";
    private const string BgFolder = "Assets/Art/BG";
    private const string BackgroundRootName = "Parallax_Background";
    private const float PixelsPerUnit = 100f;
    private const float LayerWidth = 120f;
    private const float LayerHeight = 14f;

    [MenuItem("Tools/3D \u904a\u6232\u5de5\u5177/\u5f9e Art BG \u5efa\u7acb\u8996\u5dee\u80cc\u666f")]
    public static void BuildParallaxBackground()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || scene.path != ScenePath)
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        Camera camera = Camera.main != null ? Camera.main : Object.FindFirstObjectByType<Camera>();
        if (camera == null)
        {
            Debug.LogError("\u627e\u4e0d\u5230 Main Camera\uff0c\u7121\u6cd5\u5efa\u7acb\u8996\u5dee\u80cc\u666f\u3002");
            return;
        }

        List<BackgroundAsset> assets = LoadBackgroundAssets();
        if (assets.Count == 0)
        {
            Debug.LogWarning($"\u5728 {BgFolder} \u627e\u4e0d\u5230\u53ef\u7528\u7684\u80cc\u666f\u5716\u7247\u3002");
            return;
        }

        GameObject oldRoot = GameObject.Find(BackgroundRootName);
        if (oldRoot != null)
        {
            Object.DestroyImmediate(oldRoot);
        }

        GameObject root = new GameObject(BackgroundRootName);
        root.transform.position = Vector3.zero;

        CreateLayer(root.transform, camera.transform, "BG_Static_s", assets, BackgroundGroup.Static, new Vector2(1f, 1f), new Vector3(camera.transform.position.x, 3.4f, 9f), -400);
        CreateLayer(root.transform, camera.transform, "BG_Far_c", assets, BackgroundGroup.Far, new Vector2(0.88f, 0.94f), new Vector3(camera.transform.position.x, 3.4f, 7f), -300);
        CreateLayer(root.transform, camera.transform, "BG_Mid_bs", assets, BackgroundGroup.Mid, new Vector2(0.68f, 0.9f), new Vector3(camera.transform.position.x, 3.2f, 4f), -200);
        CreateLayer(root.transform, camera.transform, "BG_Fore_bd", assets, BackgroundGroup.Fore, new Vector2(0.45f, 0.86f), new Vector3(camera.transform.position.x, 3f, 1.5f), -100);

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("\u5df2\u5f9e Art/BG \u5efa\u7acb Parallax_Background\uff1abd=\u524d\u666f\u3001bs=\u4e2d\u666f\u3001c=\u9060\u666f\u3001s/ss=\u56fa\u5b9a\u5e95\u5716\u3002");
    }

    private static void CreateLayer(Transform root, Transform cameraTransform, string layerName, List<BackgroundAsset> allAssets, BackgroundGroup group, Vector2 factor, Vector3 position, int sortingBase)
    {
        List<BackgroundAsset> layerAssets = allAssets
            .Where(asset => asset.Group == group)
            .OrderBy(asset => asset.Path)
            .ToList();

        if (layerAssets.Count == 0)
        {
            return;
        }

        GameObject layerObject = new GameObject(layerName);
        layerObject.transform.SetParent(root, false);
        layerObject.transform.position = position;

        ParallaxLayer3D parallax = layerObject.AddComponent<ParallaxLayer3D>();
        parallax.Configure(cameraTransform, factor);

        for (int i = 0; i < layerAssets.Count; i++)
        {
            CreateSpriteObject(layerObject.transform, layerAssets[i], i, sortingBase + i);
        }
    }

    private static void CreateSpriteObject(Transform parent, BackgroundAsset asset, int index, int sortingOrder)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(asset.Path);
        if (sprite == null)
        {
            Debug.LogWarning($"\u7121\u6cd5\u8f09\u5165\u80cc\u666f Sprite\uff1a{asset.Path}");
            return;
        }

        GameObject spriteObject = new GameObject(Path.GetFileNameWithoutExtension(asset.Path));
        spriteObject.transform.SetParent(parent, false);
        spriteObject.transform.localPosition = new Vector3(0f, 0f, index * -0.03f);
        spriteObject.transform.localRotation = Quaternion.identity;
        spriteObject.transform.localScale = Vector3.one;

        SpriteRenderer renderer = spriteObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = new Vector2(LayerWidth, LayerHeight);
        renderer.sortingOrder = sortingOrder;
    }

    private static List<BackgroundAsset> LoadBackgroundAssets()
    {
        List<BackgroundAsset> assets = new List<BackgroundAsset>();
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { BgFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BackgroundGroup group = ResolveGroup(path);
            if (group == BackgroundGroup.Unknown)
            {
                continue;
            }

            EnsureSpriteImport(path);
            assets.Add(new BackgroundAsset(path, group));
        }

        return assets;
    }

    private static void EnsureSpriteImport(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            changed = true;
        }

        if (importer.spriteImportMode != SpriteImportMode.Single)
        {
            importer.spriteImportMode = SpriteImportMode.Single;
            changed = true;
        }

        if (Mathf.Abs(importer.spritePixelsPerUnit - PixelsPerUnit) > 0.01f)
        {
            importer.spritePixelsPerUnit = PixelsPerUnit;
            changed = true;
        }

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            changed = true;
        }

        if (importer.wrapMode != TextureWrapMode.Repeat)
        {
            importer.wrapMode = TextureWrapMode.Repeat;
            changed = true;
        }

        if (importer.filterMode != FilterMode.Point)
        {
            importer.filterMode = FilterMode.Point;
            changed = true;
        }

        if (changed)
        {
            importer.SaveAndReimport();
        }
    }

    private static BackgroundGroup ResolveGroup(string path)
    {
        string fileName = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
        string[] parts = fileName.Split('_');
        if (parts.Length < 2)
        {
            return BackgroundGroup.Unknown;
        }

        string code = new string(parts[1].TakeWhile(char.IsLetter).ToArray());
        switch (code)
        {
            case "bd":
                return BackgroundGroup.Fore;
            case "bs":
                return BackgroundGroup.Mid;
            case "c":
                return BackgroundGroup.Far;
            case "s":
            case "ss":
                return BackgroundGroup.Static;
            default:
                return BackgroundGroup.Unknown;
        }
    }

    private readonly struct BackgroundAsset
    {
        public BackgroundAsset(string path, BackgroundGroup group)
        {
            Path = path;
            Group = group;
        }

        public string Path { get; }
        public BackgroundGroup Group { get; }
    }

    private enum BackgroundGroup
    {
        Unknown,
        Static,
        Far,
        Mid,
        Fore
    }
}
