using System.IO;
using UnityEditor;
using UnityEngine;

public static class FlameFxBuilder
{
    private const string PrefabPath = "Assets/Art/Prefab/FX/FX_Unity_Flame.prefab";
    private const string MaterialFolder = "Assets/Art/FX/Materials";
    private const string TextureFolder = "Assets/Art/FX/Textures";
    private const string FireMaterialPath = MaterialFolder + "/M_Unity_Flame_Fire.mat";
    private const string SmokeMaterialPath = MaterialFolder + "/M_Unity_Flame_Smoke.mat";
    private const string EmberMaterialPath = MaterialFolder + "/M_Unity_Flame_Ember.mat";
    private const string FireTexturePath = TextureFolder + "/FX_Unity_Flame_Sheet.png";
    private const string SmokeTexturePath = TextureFolder + "/FX_Unity_Smoke_Sheet.png";

    [MenuItem("SideScroller/FX/Create Unity Flame Prefab")]
    public static void CreateFlamePrefab()
    {
        EnsureFolder("Assets/Art/Prefab/FX");
        EnsureFolder(MaterialFolder);
        EnsureFolder(TextureFolder);

        ConfigureSpriteSheetImport(FireTexturePath);
        ConfigureSpriteSheetImport(SmokeTexturePath);

        Material fireMaterial = CreateParticleMaterial(FireMaterialPath, FireTexturePath, Color.white, false);
        Material smokeMaterial = CreateParticleMaterial(SmokeMaterialPath, SmokeTexturePath, new Color(1f, 1f, 1f, 0.55f), false);
        Material emberMaterial = CreateParticleMaterial(EmberMaterialPath, string.Empty, new Color(1f, 0.54f, 0.08f, 1f), true);

        GameObject root = new GameObject("FX_Unity_Flame");
        root.transform.position = Vector3.zero;
        root.transform.rotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        ParticleSystem core = CreateParticleSystem(root.transform, "Flame_Core", fireMaterial);
        ConfigureCoreFlame(core);

        ParticleSystem outer = CreateParticleSystem(root.transform, "Flame_Outer", fireMaterial);
        ConfigureOuterFlame(outer);

        ParticleSystem embers = CreateParticleSystem(root.transform, "Flame_Embers", emberMaterial);
        ConfigureEmbers(embers);

        ParticleSystem smoke = CreateParticleSystem(root.transform, "Flame_Smoke", smokeMaterial);
        ConfigureSmoke(smoke);

        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Created flame particle prefab: " + PrefabPath);
    }

    private static ParticleSystem CreateParticleSystem(Transform parent, string name, Material material)
    {
        GameObject particleObject = new GameObject(name);
        particleObject.transform.SetParent(parent, false);

        ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = particleObject.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.material = material;
        renderer.sortingFudge = 6f;
        renderer.minParticleSize = 0.02f;
        renderer.maxParticleSize = 2f;

        return particleSystem;
    }

    private static void ConfigureCoreFlame(ParticleSystem ps)
    {
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.24f, 0.48f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.22f, 0.58f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.34f);
        main.startRotation = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.82f, 0.28f, 0.9f), new Color(1f, 0.25f, 0.02f, 0.75f));
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = -0.05f;
        main.maxParticles = 160;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 58f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.08f;
        shape.length = 0.12f;

        ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = FlameGradient(new Color(1f, 0.95f, 0.45f, 0.9f), new Color(1f, 0.16f, 0.02f, 0f));

        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.65f, 1f, 0.05f));

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.12f;
        noise.frequency = 1.8f;
        noise.scrollSpeed = 0.75f;

        ConfigureTextureSheet(ps, 0.9f);
    }

    private static void ConfigureOuterFlame(ParticleSystem ps)
    {
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.duration = 1.15f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.42f, 0.82f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.36f, 0.95f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.28f, 0.58f);
        main.startRotation = new ParticleSystem.MinMaxCurve(-0.65f, 0.65f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.48f, 0.06f, 0.65f), new Color(0.95f, 0.08f, 0.01f, 0.35f));
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = -0.08f;
        main.maxParticles = 150;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 32f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 20f;
        shape.radius = 0.16f;
        shape.length = 0.18f;

        ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = FlameGradient(new Color(1f, 0.48f, 0.06f, 0.68f), new Color(0.8f, 0.02f, 0f, 0f));

        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.35f, 1f, 0.05f));

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.24f;
        noise.frequency = 1.35f;
        noise.scrollSpeed = 1.1f;

        ConfigureTextureSheet(ps, 0.75f);
    }

    private static void ConfigureEmbers(ParticleSystem ps)
    {
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.1f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.9f, 2.1f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.025f, 0.07f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.86f, 0.25f, 1f), new Color(1f, 0.22f, 0.04f, 0.85f));
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = -0.02f;
        main.maxParticles = 90;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 12f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 2, 5, 0.35f)
        });

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 28f;
        shape.radius = 0.1f;
        shape.length = 0.08f;

        ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = FlameGradient(new Color(1f, 0.9f, 0.25f, 1f), new Color(1f, 0.05f, 0f, 0f));

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.38f;
        noise.frequency = 1.9f;
    }

    private static void ConfigureSmoke(ParticleSystem ps)
    {
        ParticleSystem.MainModule main = ps.main;
        main.loop = true;
        main.duration = 2f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.9f, 1.65f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.18f, 0.48f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.24f, 0.54f);
        main.startRotation = new ParticleSystem.MinMaxCurve(-1.2f, 1.2f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.28f, 0.24f, 0.2f, 0.24f), new Color(0.08f, 0.075f, 0.07f, 0.12f));
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.gravityModifier = -0.04f;
        main.maxParticles = 70;

        ParticleSystem.EmissionModule emission = ps.emission;
        emission.rateOverTime = 7f;

        ParticleSystem.ShapeModule shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 18f;
        shape.radius = 0.12f;
        shape.length = 0.12f;

        ParticleSystem.ColorOverLifetimeModule color = ps.colorOverLifetime;
        color.enabled = true;
        color.color = SmokeGradient();

        ParticleSystem.SizeOverLifetimeModule size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.25f, 1f, 1.05f));

        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.22f;
        noise.frequency = 0.65f;
        noise.scrollSpeed = 0.35f;

        ConfigureTextureSheet(ps, 1f);
    }

    private static void ConfigureTextureSheet(ParticleSystem ps, float cycles)
    {
        ParticleSystem.TextureSheetAnimationModule sheet = ps.textureSheetAnimation;
        sheet.enabled = true;
        sheet.mode = ParticleSystemAnimationMode.Grid;
        sheet.numTilesX = 4;
        sheet.numTilesY = 4;
        sheet.animation = ParticleSystemAnimationType.WholeSheet;
        sheet.frameOverTime = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0f, 1f, 1f));
        sheet.cycleCount = Mathf.Max(1, Mathf.RoundToInt(cycles));
    }

    private static ParticleSystem.MinMaxGradient FlameGradient(Color start, Color end)
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(start, 0f),
                new GradientColorKey(new Color(1f, 0.45f, 0.08f, 1f), 0.45f),
                new GradientColorKey(end, 1f)
            },
            new[]
            {
                new GradientAlphaKey(start.a, 0f),
                new GradientAlphaKey(start.a * 0.72f, 0.45f),
                new GradientAlphaKey(end.a, 1f)
            });

        return new ParticleSystem.MinMaxGradient(gradient);
    }

    private static ParticleSystem.MinMaxGradient SmokeGradient()
    {
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.18f, 0.16f, 0.14f, 1f), 0f),
                new GradientColorKey(new Color(0.08f, 0.075f, 0.07f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.22f, 0.2f),
                new GradientAlphaKey(0f, 1f)
            });

        return new ParticleSystem.MinMaxGradient(gradient);
    }

    private static Material CreateParticleMaterial(string path, string texturePath, Color tint, bool additive)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }

        material.SetColor("_BaseColor", tint);
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", tint);
        }

        Texture2D texture = string.IsNullOrEmpty(texturePath) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture != null)
        {
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", additive ? 1f : 0f);
        }

        material.renderQueue = additive ? 3000 : 3000;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static void ConfigureSpriteSheetImport(string texturePath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Bilinear;
        importer.maxTextureSize = 1024;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static void EnsureFolder(string folderPath)
    {
        string normalized = folderPath.Replace("\\", "/");
        if (AssetDatabase.IsValidFolder(normalized))
        {
            return;
        }

        string parent = Path.GetDirectoryName(normalized)?.Replace("\\", "/");
        string name = Path.GetFileName(normalized);
        if (!string.IsNullOrEmpty(parent))
        {
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
