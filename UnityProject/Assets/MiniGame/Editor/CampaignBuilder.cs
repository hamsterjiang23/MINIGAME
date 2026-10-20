using System;
using System.IO;
using System.Linq;
using System.Text;
using MiniGame.Presentation.Authoring;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MiniGame.UnityHost.Editor
{
    public static class CampaignBuilder
    {
        public const string PackagePath = "Assets/MiniGame/Content/Campaign/campaign.minigame.json";
        public const string ActivePath = "Assets/Resources/MiniGame/ActiveCampaignPackage.asset";
        public const string PortraitPath = "Assets/Resources/MiniGame/PortraitCatalog.asset";
        private static string Repository => Path.GetFullPath(Path.Combine(Application.dataPath, "../.."));

        public static void ValidateContent()
        {
            var compiled = DialoguePackageImporter.ValidatePackage(File.ReadAllText(PackagePath, Encoding.UTF8));
            var asset = DialoguePackageImporter.ImportPackage(Path.GetFullPath(PackagePath), false);
            WriteReport("native-content-validation.json", new ImportReport
            {
                packageId = compiled.Package.PackageId, sourceNodes = compiled.SourceNodes.Count,
                nativeNodes = asset.Project.NodeNames.Length, portraits = compiled.Package.Portraits.Count,
                sourceHash = compiled.SourceHash, rulesFingerprint = compiled.Rules.Fingerprint,
                scene = MainRoomGameplayBuilder.ScenePath, catalog = "not activated"
            });
            Debug.Log("MINIGAME_CAMPAIGN_CONTENT_VALIDATED " + compiled.Package.PackageId);
        }

        [MenuItem("MiniGame/Campaign/Import and activate")]
        public static void ImportAndActivate()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Leave Play Mode before importing the campaign.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var compiled = DialoguePackageImporter.ValidatePackage(File.ReadAllText(PackagePath, Encoding.UTF8));
            if (compiled.Package.FormatVersion != 2) throw new InvalidOperationException("Expected a V2 campaign package.");
            // Preserve the original ActiveDialoguePackage resource for the single-day demo and its fixtures.
            var candidate = DialoguePackageImporter.ImportPackage(Path.GetFullPath(PackagePath), false);
            EnsureFolder("Assets/Resources/MiniGame");
            var catalog = ImportPortraits(compiled);
            var active = AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(ActivePath);
            if (active == null)
            {
                active = ScriptableObject.CreateInstance<DialoguePackageAsset>();
                AssetDatabase.CreateAsset(active, ActivePath);
            }
            active.Configure(candidate.PackageSource, candidate.Project, candidate.SourceHash);
            EditorUtility.SetDirty(active);
            AssetDatabase.SaveAssets();
            var scene = EditorSceneManager.OpenScene(MainRoomGameplayBuilder.ScenePath, OpenSceneMode.Single);
            EnableCampaignInOpenScene();
            EditorSceneManager.SaveScene(scene);
            WriteReport("import.json", new ImportReport
            {
                packageId = compiled.Package.PackageId, sourceNodes = compiled.SourceNodes.Count,
                nativeNodes = candidate.Project.NodeNames.Length, portraits = compiled.Package.Portraits.Count,
                sourceHash = compiled.SourceHash, rulesFingerprint = compiled.Rules.Fingerprint,
                scene = scene.path, catalog = AssetDatabase.GetAssetPath(catalog)
            });
            Debug.Log("MINIGAME_CAMPAIGN_IMPORTED nodes=" + compiled.SourceNodes.Count + " portraits=" + compiled.Package.Portraits.Count);
        }

        public static void EnableCampaignInOpenScene()
        {
            if (AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(ActivePath) == null) return;
            var host = SceneManager.GetActiveScene().GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<MiniGameBootstrap>(true)).Single();
            var serialized = new SerializedObject(host);
            serialized.FindProperty("useCampaign").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PortraitCatalog ImportPortraits(CompiledDialoguePackage compiled)
        {
            PatientMonitorAssets.ImportArtwork(false);
            string originals = Path.Combine(Repository, "Art", "Characters");
            var files = Directory.GetFiles(originals, "*.png", SearchOption.AllDirectories)
                .ToDictionary(Path.GetFileNameWithoutExtension, StringComparer.Ordinal);
            var missing = compiled.Package.Portraits.Where(portrait => !files.ContainsKey(portrait.Id)).Select(portrait => portrait.Id).ToArray();
            if (missing.Length > 0) throw new InvalidOperationException("Missing campaign portraits: " + string.Join(", ", missing));
            const string destination = "Assets/MiniGame/Art/Characters";
            EnsureFolder(destination);
            var entries = compiled.Package.Portraits.Select(portrait =>
            {
                var remade = PatientMonitorAssets.FindRemadeSprite(portrait.Id);
                if (remade != null) return new PortraitCatalog.Entry { id = portrait.Id, sprite = remade };
                string path = destination + "/" + portrait.Id + ".png";
                File.Copy(files[portrait.Id], path, true);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 1024;
                importer.npotScale = TextureImporterNPOTScale.None;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) throw new InvalidOperationException("Sprite import failed: " + path);
                return new PortraitCatalog.Entry { id = portrait.Id, sprite = sprite };
            }).ToArray();
            var catalog = AssetDatabase.LoadAssetAtPath<PortraitCatalog>(PortraitPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<PortraitCatalog>();
                AssetDatabase.CreateAsset(catalog, PortraitPath);
            }
            catalog.Configure(entries);
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        [MenuItem("MiniGame/Campaign/Build Windows development player")]
        public static void BuildWindows()
        {
            if (AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(ActivePath) == null
                || AssetDatabase.LoadAssetAtPath<PortraitCatalog>(PortraitPath) == null)
                throw new InvalidOperationException("Import and validate the full campaign first.");
            string destination = Path.Combine(Repository, "Builds", "Campaign", "MiniGame.exe");
            Directory.CreateDirectory(Path.GetDirectoryName(destination));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { MainRoomGameplayBuilder.ScenePath }, locationPathName = destination,
                target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
            });
            WriteReport("windows-build.json", new BuildSummary
            {
                result = report.summary.result.ToString(), errors = report.summary.totalErrors,
                warnings = report.summary.totalWarnings, output = destination,
                sizeBytes = report.summary.totalSize, seconds = report.summary.totalTime.TotalSeconds
            });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Campaign player build failed.");
            Debug.Log("MINIGAME_CAMPAIGN_BUILD_PASSED " + destination);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        private static void WriteReport(string name, object report)
        {
            string folder = Path.Combine(Repository, "Artifacts", "Campaign");
            Directory.CreateDirectory(folder);
            File.WriteAllText(Path.Combine(folder, name), JsonUtility.ToJson(report, true) + "\n", new UTF8Encoding(false));
        }

        [Serializable] private sealed class ImportReport
        {
            public string packageId, sourceHash, rulesFingerprint, scene, catalog;
            public int sourceNodes, nativeNodes, portraits;
        }
        [Serializable] private sealed class BuildSummary
        {
            public string result, output;
            public int errors, warnings;
            public ulong sizeBytes;
            public double seconds;
        }
    }
}
