using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>The shared settings for externally distributed Windows players.</summary>
    public static class ReleasePlayerBuilder
    {
        public const string IconPath = "Assets/MiniGame/Art/Branding/NormGameIcon.png";

        [MenuItem("MiniGame/Build/Apply external release settings")]
        public static void ApplySettings()
        {
            var importer = AssetImporter.GetAtPath(IconPath) as TextureImporter;
            if (importer != null && (importer.mipmapEnabled || !importer.alphaIsTransparency
                || importer.textureCompression != TextureImporterCompression.Uncompressed))
            {
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
            if (icon == null) throw new BuildFailedException("Missing shared NORM icon: " + IconPath);
            PlayerSettings.productName = "NORM";
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new[] { icon }, IconKind.Any);
            var sizes = PlayerSettings.GetIconSizesForTargetGroup(BuildTargetGroup.Standalone, IconKind.Any);
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Standalone,
                sizes.Select(_ => icon).ToArray(), IconKind.Any);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.connectProfiler = false;
            AssetDatabase.SaveAssets();
        }

        [MenuItem("MiniGame/Build/Build Windows external release")]
        public static void Build()
        {
            ApplySettings();
            StoryBuildValidation.Validate();
            var root = Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            if (string.IsNullOrEmpty(root)) root = Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));
            var output = Environment.GetEnvironmentVariable("MINIGAME_RELEASE_OUTPUT");
            if (string.IsNullOrEmpty(output)) output = Path.Combine(root, "Builds/NORM/NORM.exe");
            var reportPath = Environment.GetEnvironmentVariable("MINIGAME_RELEASE_REPORT");
            if (string.IsNullOrEmpty(reportPath)) reportPath = Path.Combine(root, "Artifacts/Packages/NORM/windows-build.json");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            var scenes = new[] { "Assets/MiniGame/Scenes/MainRoomCombined.unity",
                "Assets/MiniGame/Scenes/InspectionApartment.unity", "Assets/MiniGame/Scenes/DreamEndingBeach.unity" };
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes = scenes, locationPathName = output, target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            var summary = report.summary;
            var result = new {
                result = summary.result.ToString(), output, unity = Application.unityVersion,
                productName = PlayerSettings.productName, developmentBuild = (summary.options & BuildOptions.Development) != 0,
                backend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone).ToString(),
                fullScreenMode = PlayerSettings.fullScreenMode.ToString(), nativeResolution = PlayerSettings.defaultIsNativeResolution,
                icon = AssetDatabase.GetAssetPath(PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Unknown, IconKind.Any).FirstOrDefault()),
                standaloneIcons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.Standalone, IconKind.Any)
                    .Select(AssetDatabase.GetAssetPath).ToArray(),
                scenes, errors = summary.totalErrors, warnings = summary.totalWarnings, seconds = summary.totalTime.TotalSeconds,
                messages = report.steps.SelectMany(s => s.messages).Where(m => m.type == LogType.Error || m.type == LogType.Warning)
                    .Select(m => new { type = m.type.ToString(), message = m.content }).ToArray()
            };
            File.WriteAllText(reportPath, Newtonsoft.Json.JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented));
            if (summary.result != BuildResult.Succeeded) throw new BuildFailedException("Windows external release failed.");
            Debug.Log("NORM_EXTERNAL_BUILD_PASSED");
        }
    }
}
