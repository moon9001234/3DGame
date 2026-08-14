using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class SideScrollerWebGLBuilder
{
    private const string DefaultScenePath = "Assets/Scenes/SampleScene_03.unity";
    private const string DefaultOutputPath = "Builds/WebGL";
    private const int CanvasWidth = 1280;
    private const int CanvasHeight = 720;
    private const string WebGLProductVersion = "0.1.2";

    [MenuItem("SideScroller/Build WebGL Player")]
    public static void BuildWebGLFromMenu()
    {
        BuildWebGL();
    }

    public static void BuildWebGL()
    {
        string outputPath = GetCommandLineValue("-buildOutputPath", DefaultOutputPath);
        outputPath = Path.GetFullPath(outputPath);
        Directory.CreateDirectory(outputPath);

        ConfigureWebGLPlayer();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { DefaultScenePath },
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"WebGL build failed: {summary.result}");
        }

        UnityEngine.Debug.Log($"WebGL build succeeded: {outputPath}");
    }

    private static void ConfigureWebGLPlayer()
    {
        PlayerSettings.defaultScreenWidth = CanvasWidth;
        PlayerSettings.defaultScreenHeight = CanvasHeight;
        PlayerSettings.bundleVersion = WebGLProductVersion;
        SetWebGLOption("compressionFormat", "Disabled");
        SetWebGLOption("decompressionFallback", true);
    }

    private static void SetWebGLOption(string propertyName, object value)
    {
        Type webGLSettings = typeof(PlayerSettings).GetNestedType("WebGL", BindingFlags.Public);
        PropertyInfo property = webGLSettings?.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
        if (property == null || !property.CanWrite)
        {
            return;
        }

        object convertedValue = value;
        if (property.PropertyType.IsEnum && value is string enumName)
        {
            convertedValue = Enum.Parse(property.PropertyType, enumName);
        }

        property.SetValue(null, convertedValue);
    }

    private static string GetCommandLineValue(string key, string fallback)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return fallback;
    }
}
