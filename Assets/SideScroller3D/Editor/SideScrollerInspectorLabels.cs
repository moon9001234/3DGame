using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

internal static class SideScrollerInspectorLabels
{
    private const string LabelAssetDirectory = "Assets/SideScroller3D/Editor/InspectorLabels";

    private static readonly Dictionary<string, string> Labels = new Dictionary<string, string>();
    private static readonly Dictionary<string, string> Tooltips = new Dictionary<string, string>();
    private static readonly Dictionary<string, string> Sections = new Dictionary<string, string>();
    private static long loadedTicks;

    public static void ReloadIfNeeded()
    {
        string fullDirectory = ToFullPath(LabelAssetDirectory);
        if (!Directory.Exists(fullDirectory))
        {
            return;
        }

        string[] files = Directory.GetFiles(fullDirectory, "*.json", SearchOption.TopDirectoryOnly);
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        long latestTicks = 0L;
        for (int i = 0; i < files.Length; i++)
        {
            latestTicks = Math.Max(latestTicks, File.GetLastWriteTimeUtc(files[i]).Ticks);
        }

        if (latestTicks == loadedTicks && Labels.Count > 0)
        {
            return;
        }

        loadedTicks = latestTicks;
        Labels.Clear();
        Tooltips.Clear();
        Sections.Clear();

        for (int i = 0; i < files.Length; i++)
        {
            LoadFile(files[i]);
        }
    }

    public static GUIContent Content(string key, string fallback)
    {
        ReloadIfNeeded();
        string text = Text(key, fallback);
        string tooltip = Tooltips.TryGetValue(key, out string foundTooltip) ? foundTooltip : string.Empty;
        return new GUIContent(text, tooltip);
    }

    public static GUIContent Content(string ownerTypeName, string propertyName, string fallback)
    {
        ReloadIfNeeded();
        string typedKey = ownerTypeName + "." + propertyName;
        string text = Labels.TryGetValue(typedKey, out string typedText)
            ? typedText
            : Text(propertyName, fallback);
        string tooltip = Tooltips.TryGetValue(typedKey, out string typedTooltip)
            ? typedTooltip
            : (Tooltips.TryGetValue(propertyName, out string genericTooltip) ? genericTooltip : string.Empty);
        return new GUIContent(text, tooltip);
    }

    public static string Text(string key, string fallback)
    {
        ReloadIfNeeded();
        return Labels.TryGetValue(key, out string foundText) ? foundText : fallback;
    }

    public static string Section(string key, string fallback)
    {
        ReloadIfNeeded();
        return Sections.TryGetValue(key, out string foundText) ? foundText : fallback;
    }

    private static void LoadFile(string fullPath)
    {
        try
        {
            string json = File.ReadAllText(fullPath, Encoding.UTF8);
            LabelFile labelFile = JsonUtility.FromJson<LabelFile>(json);
            AddEntries(Labels, labelFile != null ? labelFile.labels : null);
            AddEntries(Tooltips, labelFile != null ? labelFile.tooltips : null);
            AddEntries(Sections, labelFile != null ? labelFile.sections : null);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Could not load inspector labels from '" + fullPath + "'. Falling back to English labels.\n" + exception.Message);
        }
    }

    private static void AddEntries(Dictionary<string, string> target, LabelEntry[] entries)
    {
        if (entries == null)
        {
            return;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            LabelEntry entry = entries[i];
            if (entry == null || string.IsNullOrEmpty(entry.key))
            {
                continue;
            }

            target[entry.key] = entry.text ?? string.Empty;
        }
    }

    private static string ToFullPath(string assetPath)
    {
        string relativePath = assetPath.StartsWith("Assets/", StringComparison.Ordinal)
            ? assetPath.Substring("Assets/".Length)
            : assetPath;
        return Path.Combine(Application.dataPath, relativePath);
    }

    [Serializable]
    private class LabelFile
    {
        public LabelEntry[] labels;
        public LabelEntry[] tooltips;
        public LabelEntry[] sections;
    }

    [Serializable]
    private class LabelEntry
    {
        public string key;
        public string text;
    }
}
