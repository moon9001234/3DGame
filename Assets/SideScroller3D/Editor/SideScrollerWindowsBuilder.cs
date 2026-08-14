using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class SideScrollerWindowsBuilder
{
    private const string DefaultScenePath = "Assets/Scenes/SampleScene_03.unity";
    private const string DefaultOutputPath = "Builds/Windows/3DGame.exe";
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;

    [MenuItem("SideScroller/Build Windows Player")]
    public static void BuildWindows64FromMenu()
    {
        BuildWindows64();
    }

    public static void BuildWindows64()
    {
        string outputPath = GetCommandLineValue("-buildOutputPath", DefaultOutputPath);
        outputPath = Path.GetFullPath(outputPath);

        string outputDirectory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        ConfigureWindowedPlayer();

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { DefaultScenePath },
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"Windows build failed: {summary.result}");
        }

        UnityEngine.Debug.Log($"Windows build succeeded: {outputPath}");
    }

    private static void ConfigureWindowedPlayer()
    {
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = WindowWidth;
        PlayerSettings.defaultScreenHeight = WindowHeight;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
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
