using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Yarn.Unity;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Checks runtime story dependencies before any Player build can ship.</summary>
    public sealed class StoryBuildValidation : IPreprocessBuildWithReport
    {
        public int callbackOrder => -1000;

        public void OnPreprocessBuild(BuildReport report) => Validate();

        public static void Validate()
        {
            const string supervisorFolder = "Assets/MiniGame/Resources/MiniGame/Supervisor/";
            if (!File.Exists(supervisorFolder + "Flow.yarn"))
                throw new BuildFailedException("Required supervisor story source is missing: " + supervisorFolder + "Flow.yarn");

            var project = AssetDatabase.LoadAssetAtPath<YarnProject>(supervisorFolder + "DialogueProject.yarnproject");
            var content = SupervisorDialogueContent.Load();
            if (project == null || content == null)
                throw new BuildFailedException("Supervisor dialogue content has not been imported.");

            var expected = content.conversations.SelectMany(conversation => conversation.day <= 3
                ? new[] { "Supervisor_D" + conversation.day + "_MorningInspection", "Supervisor_D" + conversation.day + "_Briefing" }
                : new[] { "Supervisor_D4_Ending" }).ToArray();
            var missing = expected.Except(project.NodeNames).ToArray();
            if (missing.Length != 0)
                throw new BuildFailedException("Supervisor dialogue nodes are missing: " + string.Join(", ", missing));

            foreach (string path in Directory.GetFiles("Assets", "*.yarnproject", SearchOption.AllDirectories))
            {
                if (path.Replace('\\', '/').Contains("/Editor/")) continue;
                var definition = JsonUtility.FromJson<ProjectSources>(File.ReadAllText(path));
                if (definition == null || definition.sourceFiles == null || definition.sourceFiles.Length == 0)
                    throw new BuildFailedException("Yarn project has no source dependencies: " + path);
                foreach (string source in definition.sourceFiles)
                {
                    string folder = Path.GetDirectoryName(path);
                    bool exists = source.IndexOfAny(new[] { '*', '?' }) < 0
                        ? File.Exists(Path.Combine(folder, source))
                        : Directory.GetFiles(folder, source, SearchOption.AllDirectories).Length > 0;
                    if (!exists) throw new BuildFailedException("Yarn source dependency is missing: " + path + " -> " + source);
                }
            }
        }

        [Serializable] private sealed class ProjectSources
        {
            public string[] sourceFiles = Array.Empty<string>();
        }
    }
}
