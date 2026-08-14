using UnityEditor;
using UnityEngine;

public static class WebGLMaterialCompatibilityInstaller
{
    private static readonly string[] MaterialPaths =
    {
        "Assets/Art/FBX/TV_Man.mat",
        "Assets/Art/FBX/TV_Face.mat",
        "Assets/Art/FBX/TV_Monster_01.mat",
        "Assets/Art/FBX/TV_Monster_02.mat",
        "Assets/Art/FBX/TV_Monster_03.mat",
        "Assets/Art/FBX/100_o01002.mat",
        "Assets/SideScroller3D/Materials/Player_Mat.mat",
        "Assets/SideScroller3D/Materials/Enemy_Mat.mat",
        "Assets/SideScroller3D/Materials/Ground_Mat.mat",
        "Assets/SideScroller3D/Materials/Platform_Mat.mat"
    };

    public static void Install()
    {
        Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
        if (unlit == null)
        {
            Debug.LogError("WebGL material fix failed: URP Unlit shader not found.");
            return;
        }

        foreach (string path in MaterialPaths)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Debug.LogWarning($"WebGL material fix skipped missing material: {path}");
                continue;
            }

            Texture mainTexture = GetTexture(material, "_BaseMap", "_MainTex");
            Color baseColor = GetColor(material, "_BaseColor", "_Color", Color.white);

            material.shader = unlit;
            if (mainTexture != null)
            {
                SetTexture(material, mainTexture, "_BaseMap", "_MainTex");
            }

            SetColor(material, baseColor, "_BaseColor", "_Color");
            SetFloat(material, "_Surface", 0f);
            SetFloat(material, "_AlphaClip", 0f);
            SetFloat(material, "_ZWrite", 1f);
            SetFloat(material, "_SrcBlend", 1f);
            SetFloat(material, "_DstBlend", 0f);
            material.renderQueue = 2000;

            EditorUtility.SetDirty(material);
            Debug.Log($"WebGL material fix applied: {path}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static Texture GetTexture(Material material, params string[] names)
    {
        foreach (string name in names)
        {
            if (material.HasProperty(name))
            {
                Texture texture = material.GetTexture(name);
                if (texture != null)
                {
                    return texture;
                }
            }
        }

        return null;
    }

    private static Color GetColor(Material material, string primary, string fallback, Color defaultValue)
    {
        if (material.HasProperty(primary))
        {
            return material.GetColor(primary);
        }

        return material.HasProperty(fallback) ? material.GetColor(fallback) : defaultValue;
    }

    private static void SetTexture(Material material, Texture texture, params string[] names)
    {
        foreach (string name in names)
        {
            if (material.HasProperty(name))
            {
                material.SetTexture(name, texture);
            }
        }
    }

    private static void SetColor(Material material, Color color, params string[] names)
    {
        foreach (string name in names)
        {
            if (material.HasProperty(name))
            {
                material.SetColor(name, color);
            }
        }
    }

    private static void SetFloat(Material material, string name, float value)
    {
        if (material.HasProperty(name))
        {
            material.SetFloat(name, value);
        }
    }
}
