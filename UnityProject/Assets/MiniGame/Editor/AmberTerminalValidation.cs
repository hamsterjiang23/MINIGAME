using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace MiniGame.UnityHost.Editor
{
    public static class AmberTerminalValidation
    {
        [Serializable] private sealed class Report
        {
            public string result, output, unity;
            public string[] scenes;
            public int errors, warnings;
            public double seconds;
        }
        public static void BuildWindows() => BuildWindowsVariant("TerminalAmber");
        public static void BuildWindowsClosing() => BuildWindowsVariant("TerminalClosing");
        public static void BuildWindowsScreenFit() => BuildWindowsVariant("TerminalScreenFit");
        public static void BuildWindowsNormOpening() => BuildWindowsVariant("NormOpening");
        public static void BuildWindowsNormBugFix() => BuildWindowsVariant("NormBugFix", false);
        public static void BuildWindowsTerminalTypingAudio() => BuildWindowsVariant("TerminalTypingAudio", false);
        public static void BuildWindowsWallLighting() => BuildWindowsVariant("WallLighting", false);
        public static void BuildWindowsLightingContinuity() => BuildWindowsVariant("LightingContinuity", false);
        public static void BuildWindowsWorldDialogue() => BuildWindowsVariant("WorldDialogue", false);
        public static void BuildWindowsBedtimeParticles() => BuildWindowsVariant("BedtimeParticles", false);
        public static void BuildWindowsMonitorComposition() => BuildWindowsVariant("MonitorComposition", false);
        public static void BuildWindowsCompletePortraits() => BuildWindowsVariant("CompletePortraits", false);
        public static void BuildWindowsPixelSettings() => BuildWindowsVariant("PixelSettings", false);
        private static void BuildWindowsVariant(string variant, bool development = true)
        {
            string root=Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT");
            if(string.IsNullOrEmpty(root)||!File.Exists(Path.Combine(root,"src","MiniGame.sln"))) throw new InvalidOperationException("Set MINIGAME_REPOSITORY_ROOT to this repository.");
            string destination=Path.Combine(root,"Builds",variant,"MiniGame.exe");
            string[] scenes={"Assets/MiniGame/Scenes/MainRoomCombined.unity","Assets/MiniGame/Scenes/InspectionApartment.unity","Assets/MiniGame/Scenes/DreamEndingBeach.unity"};
            foreach(string scene in scenes) if(!File.Exists(scene)) throw new FileNotFoundException("Required campaign scene",scene);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone,ScriptingImplementation.Mono2x);
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            BuildReport built=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=scenes,locationPathName=destination,target=BuildTarget.StandaloneWindows64,
                options=development?BuildOptions.Development:BuildOptions.None,extraScriptingDefines=development?null:new[]{"MINIGAME_FEEDBACK_PROBE"}});
            var report=new Report {result=built.summary.result.ToString(),output=destination,unity=Application.unityVersion,scenes=scenes,errors=(int)built.summary.totalErrors,warnings=(int)built.summary.totalWarnings,seconds=built.summary.totalTime.TotalSeconds};
            string folder=Path.Combine(root,"Artifacts",variant);Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder,"windows-build.json"),JsonUtility.ToJson(report,true));
            if(built.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("Amber campaign build failed.");
            Debug.Log("MINIGAME_AMBER_BUILD_PASSED "+destination);
        }
    }
}
