using System;
using System.IO;
using System.Linq;
using System.Text;
using MiniGame.Core;
using MiniGame.Presentation;
using UnityEditor;
using UnityEngine;
using Yarn.Unity;

namespace MiniGame.UnityHost.Editor
{
    public static class GeneratePrototypeYarn
    {
        public const string ContentDirectory = "Assets/MiniGame/Content";
        public const string SourcePath = ContentDirectory + "/Prototype.yarn";
        public const string ProjectPath = ContentDirectory + "/Prototype.yarnproject";
        public const string MappingPath = ContentDirectory + "/mapping.json";

        [MenuItem("MiniGame/Generate Original Prototype Yarn")]
        public static void GenerateFromMenu() => Generate();

        public static YarnProject Generate()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var outputDirectory = Path.Combine(Application.dataPath, "MiniGame", "Content");
            Directory.CreateDirectory(outputDirectory);
            File.WriteAllText(Path.Combine(outputDirectory, "Prototype.yarn"), PrototypeYarnSource.Build(content), new UTF8Encoding(false));
            // Schema and importer are from the pinned Yarn Spinner Unity 3.2.8 source.
            File.WriteAllText(Path.Combine(outputDirectory, "Prototype.yarnproject"),
                "{\n  \"projectFileVersion\": 3,\n  \"sourceFiles\": [\"Prototype.yarn\"],\n  \"excludeFiles\": [],\n  \"baseLanguage\": \"zh-Hans\",\n  \"localisation\": {}\n}\n", new UTF8Encoding(false));
            var mapping = new NodeMapping
            {
                revision = content.Revision,
                rulesFingerprint = rules.Fingerprint,
                nodes = content.Nodes.Select(n => new NodeMappingEntry
                {
                    coreNodeId = n.NodeId, yarnNodeName = PrototypeYarnSource.NodeName(n.NodeId)
                }).ToArray()
            };
            File.WriteAllText(Path.Combine(outputDirectory, "mapping.json"), JsonUtility.ToJson(mapping, true) + "\n", new UTF8Encoding(false));
            AssetDatabase.ImportAsset(SourcePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(ProjectPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(MappingPath, ImportAssetOptions.ForceSynchronousImport);
            var project = AssetDatabase.LoadAssetAtPath<YarnProject>(ProjectPath);
            if (project == null || project.Program == null)
                throw new InvalidOperationException("Original prototype Yarn failed to import. Inspect Yarn compiler diagnostics.");
            Debug.Log("Generated original development Yarn and core-node mapping: " + ProjectPath);
            return project;
        }

        [Serializable]
        private sealed class NodeMapping
        {
            public string revision = "";
            public string rulesFingerprint = "";
            public NodeMappingEntry[] nodes = Array.Empty<NodeMappingEntry>();
        }

        [Serializable]
        private sealed class NodeMappingEntry
        {
            public string coreNodeId = "";
            public string yarnNodeName = "";
        }
    }
}
