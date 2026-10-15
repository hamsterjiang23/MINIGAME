using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MiniGame.Presentation.Authoring;
using UnityEditor;
using UnityEngine;
using Yarn.Compiler;
using Yarn.Unity;

namespace MiniGame.UnityHost.Editor
{
    /// <summary>Validates before creating assets, and changes the active reference only after native import succeeds.</summary>
    public static class DialoguePackageImporter
    {
        public const string DefaultPackagePath = "Assets/MiniGame/Content/TerminalDemo/terminal-day1.minigame.json";
        public const string ActiveAssetPath = "Assets/Resources/MiniGame/ActiveDialoguePackage.asset";
        public const string ImportedDirectory = "Assets/MiniGame/ImportedDialogue";
        private const string ProjectJson = "{\n  \"projectFileVersion\": 3,\n  \"sourceFiles\": [\"Runtime.yarn\"],\n  \"excludeFiles\": [],\n  \"baseLanguage\": \"zh-Hans\",\n  \"localisation\": {}\n}\n";

        public static DialoguePackageAsset EnsureDefaultPackage()
        {
            var active = AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(ActiveAssetPath);
            if (active != null)
            {
                RequireUsable(active);
                return active;
            }
            return ImportPackage(Path.GetFullPath(DefaultPackagePath));
        }

        public static CompiledDialoguePackage ValidatePackage(string json)
        {
            var compiled = DialoguePackageCompiler.Compile(json);
            var temporaryDirectory = Path.Combine(Path.GetTempPath(), "MiniGame-Yarn-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryDirectory);
            var temporarySource = Path.Combine(temporaryDirectory, "Runtime.yarn");
            try
            {
                File.WriteAllText(temporarySource, compiled.RuntimeYarn, new UTF8Encoding(false));
                var job = CompilationJob.CreateFromFiles(new[] { temporarySource });
                var result = Yarn.Compiler.Compiler.Compile(job);
                var errors = result.Diagnostics.Where(d => d.Severity == Diagnostic.DiagnosticSeverity.Error).ToArray();
                if (errors.Length != 0)
                    throw new InvalidOperationException("Native Yarn validation failed:\n" + string.Join("\n", errors.Select(d =>
                        "Runtime.yarn:" + (d.Range.Start.Line + 1) + ": " + d.Message)));
                if (result.Program == null)
                    throw new InvalidOperationException("Native Yarn validation produced no executable program.");
            }
            finally
            {
                if (File.Exists(temporarySource)) File.Delete(temporarySource);
                Directory.Delete(temporaryDirectory);
            }
            return compiled;
        }

        public static DialoguePackageAsset ImportPackage(string absoluteFilePath, bool activate = true)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Leave Play Mode before importing dialogue content.");
            var sourcePath = Path.GetFullPath(absoluteFilePath);
            var json = File.ReadAllText(sourcePath, Encoding.UTF8);
            var compiled = ValidatePackage(json);
            // The identity includes JSON metadata as well as Yarn and rules. Short hashes keep Windows paths usable.
            var identity = Hash(compiled.SourceHash + ":" + compiled.Rules.Fingerprint + ":" + Hash(json));
            var packageId = new string(compiled.Package.PackageId.Select(c => char.IsLetterOrDigit(c) || c == '-' || c == '_' ? c : '_').ToArray());
            var directory = ImportedDirectory + "/" + packageId + "/" + identity.Substring(0, 32);
            EnsureFolder(directory);
            var packagePath = directory + "/Package.minigame.json";
            var yarnPath = directory + "/Runtime.yarn";
            var projectPath = directory + "/Runtime.yarnproject";
            WriteImmutable(packagePath, json);
            WriteImmutable(yarnPath, compiled.RuntimeYarn);
            WriteImmutable(projectPath, ProjectJson);
            AssetDatabase.ImportAsset(packagePath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(yarnPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(projectPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            var project = AssetDatabase.LoadAssetAtPath<YarnProject>(projectPath);
            var source = AssetDatabase.LoadAssetAtPath<TextAsset>(packagePath);
            if (source == null || project == null || project.compiledYarnProgram == null || project.compiledYarnProgram.Length == 0
                || project.Program == null || project.NodeNames.Length == 0)
                throw new InvalidOperationException(projectPath + ": Native Yarn import failed. The previous active package was preserved.");

            var candidatePath = directory + "/DialoguePackage.asset";
            var candidate = AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(candidatePath);
            if (candidate == null)
            {
                candidate = ScriptableObject.CreateInstance<DialoguePackageAsset>();
                candidate.Configure(source, project, compiled.SourceHash);
                AssetDatabase.CreateAsset(candidate, candidatePath);
                AssetDatabase.SaveAssets();
            }
            else if (candidate.PackageSource != source || candidate.Project != project || candidate.SourceHash != compiled.SourceHash)
                throw new InvalidOperationException(candidatePath + ": Immutable package references do not match its validated source.");

            if (activate) Activate(candidate);
            Debug.Log("MINIGAME_DIALOGUE_PACKAGE_IMPORTED package=" + compiled.Package.PackageId + " source=" + compiled.SourceHash
                + " rules=" + compiled.Rules.Fingerprint + " active=" + activate + " path=" + candidatePath);
            return activate ? AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(ActiveAssetPath) : candidate;
        }

        private static void Activate(DialoguePackageAsset candidate)
        {
            RequireUsable(candidate);
            EnsureFolder("Assets/Resources/MiniGame");
            var active = AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(ActiveAssetPath);
            if (active == null)
            {
                active = ScriptableObject.CreateInstance<DialoguePackageAsset>();
                active.Configure(candidate.PackageSource, candidate.Project, candidate.SourceHash);
                AssetDatabase.CreateAsset(active, ActiveAssetPath);
                AssetDatabase.SaveAssets();
                return;
            }
            var previousSource = active.PackageSource;
            var previousProject = active.Project;
            var previousHash = active.SourceHash;
            try
            {
                active.Configure(candidate.PackageSource, candidate.Project, candidate.SourceHash);
                EditorUtility.SetDirty(active);
                AssetDatabase.SaveAssets();
            }
            catch
            {
                if (previousSource != null && previousProject != null && !string.IsNullOrWhiteSpace(previousHash))
                {
                    active.Configure(previousSource, previousProject, previousHash);
                    EditorUtility.SetDirty(active);
                    AssetDatabase.SaveAssets();
                }
                throw;
            }
        }

        private static void RequireUsable(DialoguePackageAsset asset)
        {
            if (asset.PackageSource == null || asset.Project == null || asset.Project.compiledYarnProgram == null
                || asset.Project.compiledYarnProgram.Length == 0 || string.IsNullOrWhiteSpace(asset.SourceHash)
                || asset.Project.Program == null || asset.Project.NodeNames.Length == 0)
                throw new InvalidOperationException(AssetDatabase.GetAssetPath(asset) + ": The dialogue package has missing source or native Yarn output. Import a validated package first.");
            var compiled = DialoguePackageCompiler.Compile(asset.PackageSource.text);
            var projectPath = AssetDatabase.GetAssetPath(asset.Project);
            var runtimeSourcePath = Path.Combine(Path.GetDirectoryName(projectPath), "Runtime.yarn");
            if (compiled.SourceHash != asset.SourceHash || !File.Exists(runtimeSourcePath)
                || !string.Equals(File.ReadAllText(runtimeSourcePath, Encoding.UTF8), compiled.RuntimeYarn, StringComparison.Ordinal))
                throw new InvalidOperationException(AssetDatabase.GetAssetPath(asset) + ": Source package and native Yarn output do not match. Import the package again.");
        }

        private static void WriteImmutable(string assetPath, string text)
        {
            if (File.Exists(assetPath))
            {
                if (!string.Equals(File.ReadAllText(assetPath, Encoding.UTF8), text, StringComparison.Ordinal))
                    throw new InvalidOperationException(assetPath + ": Existing immutable source differs from this package. Import was stopped.");
                return;
            }
            File.WriteAllText(assetPath, text, new UTF8Encoding(false));
        }

        private static void EnsureFolder(string assetDirectory)
        {
            if (AssetDatabase.IsValidFolder(assetDirectory)) return;
            var parent = Path.GetDirectoryName(assetDirectory).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(assetDirectory));
        }

        private static string Hash(string text)
        {
            using var algorithm = SHA256.Create();
            return string.Concat(algorithm.ComputeHash(Encoding.UTF8.GetBytes(text)).Select(value => value.ToString("x2")));
        }

        [MenuItem("MiniGame/Validate Active Dialogue Package")]
        public static void ValidateActivePackage()
        {
            var active = AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(ActiveAssetPath);
            if (active == null) throw new InvalidOperationException("No active dialogue package. Open MiniGame/Dialogue Package Importer.");
            RequireUsable(active);
            var compiled = ValidatePackage(active.PackageSource.text);
            if (compiled.SourceHash != active.SourceHash) throw new InvalidOperationException("The active package source hash does not match.");
            Debug.Log("MINIGAME_ACTIVE_DIALOGUE_VALIDATED " + compiled.Package.PackageId + " nodes=" + compiled.SourceNodes.Count);
        }

        public static void ValidateDefaultAndWriteReport()
        {
            var repository = Path.GetFullPath(Environment.GetEnvironmentVariable("MINIGAME_REPOSITORY_ROOT")
                ?? Path.Combine(Application.dataPath, "../.."));
            var reportPath = Path.Combine(repository, "Artifacts", "Unity", "terminal-package-import.json");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            var report = new ImportReport { status = "failed", activeAssetPath = ActiveAssetPath };
            try
            {
                var compiled = ValidatePackage(File.ReadAllText(DefaultPackagePath, Encoding.UTF8));
                var asset = ImportPackage(Path.GetFullPath(DefaultPackagePath), false);
                report.status = "passed";
                report.packageId = compiled.Package.PackageId;
                report.sourceHash = compiled.SourceHash;
                report.rulesFingerprint = compiled.Rules.Fingerprint;
                report.sourceNodes = compiled.SourceNodes.Count;
                report.nativeNodes = asset.Project.NodeNames.Length;
                report.packageAssetPath = AssetDatabase.GetAssetPath(asset);
                report.diagnostics = compiled.Diagnostics.Select(d => d.ToString()).ToArray();
                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true) + "\n", new UTF8Encoding(false));
                Debug.Log("MINIGAME_DIALOGUE_IMPORT_VALIDATION_PASSED " + reportPath);
            }
            catch (Exception error)
            {
                report.error = error.Message;
                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true) + "\n", new UTF8Encoding(false));
                throw;
            }
        }

        [Serializable]
        private sealed class ImportReport
        {
            public string status = "";
            public string packageId = "";
            public string sourceHash = "";
            public string rulesFingerprint = "";
            public int sourceNodes;
            public int nativeNodes;
            public string packageAssetPath = "";
            public string activeAssetPath = "";
            public string[] diagnostics = Array.Empty<string>();
            public string error = "";
            public string scope = "Restricted dialect validation, native Yarn compilation and Unity asset import; visual playability is tested separately.";
        }
    }

    public sealed class DialoguePackageImporterWindow : EditorWindow
    {
        private string selectedPath = "";
        private string validatedJson = "";
        private CompiledDialoguePackage compiled;
        private string summary = "选择策划工具导出的 .minigame.json 内容包。";
        private string error = "";
        private Vector2 scroll;

        [MenuItem("MiniGame/Dialogue Package Importer")]
        public static void Open() => GetWindow<DialoguePackageImporterWindow>("对话内容包");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("对话内容包", EditorStyles.boldLabel);
            if (GUILayout.Button("选择内容包 JSON"))
            {
                var path = EditorUtility.OpenFilePanel("选择对话内容包", string.IsNullOrEmpty(selectedPath) ? "" : Path.GetDirectoryName(selectedPath), "json");
                if (!string.IsNullOrEmpty(path)) { selectedPath = path; compiled = null; validatedJson = ""; error = ""; }
            }
            EditorGUILayout.SelectableLabel(selectedPath, EditorStyles.wordWrappedLabel, GUILayout.Height(38));
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(selectedPath) || EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button("校验并比较")) ValidateSelection();
            scroll = EditorGUILayout.BeginScrollView(scroll);
            EditorGUILayout.HelpBox(summary, MessageType.Info);
            if (!string.IsNullOrEmpty(error)) EditorGUILayout.HelpBox(error, MessageType.Error);
            EditorGUILayout.EndScrollView();
            using (new EditorGUI.DisabledScope(compiled == null || EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button("导入并设为当前内容"))
                {
                    try
                    {
                        if (!string.Equals(File.ReadAllText(selectedPath, Encoding.UTF8), validatedJson, StringComparison.Ordinal))
                            throw new InvalidOperationException("内容包在校验后发生变化，请重新校验并比较。");
                        Selection.activeObject = DialoguePackageImporter.ImportPackage(selectedPath);
                        summary = "导入完成，当前内容已切换。\n" + summary;
                        compiled = null;
                        error = "";
                    }
                    catch (Exception exception) { error = exception.Message; compiled = null; }
                }
        }

        private void ValidateSelection()
        {
            compiled = null;
            error = "";
            try
            {
                validatedJson = File.ReadAllText(selectedPath, Encoding.UTF8);
                var result = DialoguePackageImporter.ValidatePackage(validatedJson);
                var active = AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(DialoguePackageImporter.ActiveAssetPath);
                var old = active != null && active.PackageSource != null ? DialoguePackageCompiler.Compile(active.PackageSource.text) : null;
                var previousIds = old?.SourceNodes.Select(node => node.Id).ToArray() ?? Array.Empty<string>();
                var nextIds = result.SourceNodes.Select(node => node.Id).ToArray();
                summary = "内容包：" + result.Package.PackageId + "  版本：" + result.Package.ContentVersion
                    + "\n对象：" + result.Package.Profile.PatientName + "  对话节点：" + nextIds.Length
                    + "\n新增节点：" + nextIds.Except(previousIds).Count() + "  删除节点：" + previousIds.Except(nextIds).Count()
                    + "\n对白内容：" + (old != null && old.SourceHash == result.SourceHash ? "未变化" : "已变化")
                    + "\n规则参数：" + (old != null && old.Rules.Fingerprint == result.Rules.Fingerprint ? "未变化" : "已变化")
                    + "\n初始配额：" + result.Package.Profile.InitialQuota + "  离所阈值：" + result.Package.Profile.ReleaseThreshold
                    + "\n\n" + string.Join("\n", result.Diagnostics.Select(d => d.ToString()));
                compiled = result;
            }
            catch (Exception exception) { error = exception.Message; }
        }
    }
}
