using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            List<string> inputs = [];
            string? output = null;
            bool smoke = false;
            for (int index = 0; index < args.Length; index++)
            {
                switch (args[index])
                {
                    case "--input": inputs.Add(Path.GetFullPath(args[++index])); break;
                    case "--output": output = Path.GetFullPath(args[++index]); break;
                    case "--smoke": smoke = true; break;
                    default: throw new ArgumentException("Unknown argument: " + args[index]);
                }
            }
            if (inputs.Count == 0 || output is null) throw new ArgumentException("Provide --input directory/file (repeatable) and --output JSON path.");
            List<string> files = [];
            foreach (string input in inputs)
            {
                if (File.Exists(input))
                {
                    if (!input.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Input file is not .cs: " + input);
                    files.Add(input);
                }
                else if (Directory.Exists(input))
                {
                    files.AddRange(Directory.EnumerateFiles(input, "*.cs", SearchOption.AllDirectories).Where(path =>
                        !Path.GetRelativePath(input, path).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            .Any(segment => segment.Equals("bin", StringComparison.OrdinalIgnoreCase) || segment.Equals("obj", StringComparison.OrdinalIgnoreCase))));
                }
                else throw new DirectoryNotFoundException(input);
            }
            files = files.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(path => path, StringComparer.OrdinalIgnoreCase).ToList();
            if (files.Count == 0) throw new InvalidOperationException("No source files discovered.");
            int discovered = files.Count;
            if (smoke) files = [files[0]];
            CSharpParseOptions options = new(LanguageVersion.CSharp9, DocumentationMode.Parse, SourceCodeKind.Regular, Array.Empty<string>());
            List<object> results = [];
            int errors = 0, warnings = 0, errorFiles = 0, fileScopedNamespaces = 0;
            string[] observedFeatureKinds = ["RecordStructDeclaration", "ListPattern", "CollectionExpression"];
            HashSet<string> sdkSyntaxKindNames = Enum.GetNames<SyntaxKind>().ToHashSet(StringComparer.Ordinal);
            Dictionary<string, int> featureTotals = observedFeatureKinds.ToDictionary(kind => kind, _ => 0);
            int globalUsingTotal = 0;
            foreach (string path in files)
            {
                byte[] bytes = File.ReadAllBytes(path);
                string source = File.ReadAllText(path, Encoding.UTF8);
                SyntaxTree tree = CSharpSyntaxTree.ParseText(source, options, path, Encoding.UTF8, CancellationToken.None);
                Diagnostic[] diagnostics = tree.GetDiagnostics(CancellationToken.None).ToArray();
                SyntaxNode[] nodes = tree.GetRoot(CancellationToken.None).DescendantNodes(null, false).ToArray();
                int fileScopedCount = nodes.Count(node => node.RawKind == (int)SyntaxKind.FileScopedNamespaceDeclaration);
                fileScopedNamespaces += fileScopedCount;
                int globalUsingCount = nodes.OfType<UsingDirectiveSyntax>().Count(directive => directive.GlobalKeyword.RawKind == (int)SyntaxKind.GlobalKeyword);
                globalUsingTotal += globalUsingCount;
                Dictionary<string, int> featureCounts = observedFeatureKinds.ToDictionary(kind => kind,
                    kind => nodes.Count(node => ((SyntaxKind)node.RawKind).ToString() == kind));
                foreach ((string feature, int count) in featureCounts) featureTotals[feature] += count;
                int fileErrors = diagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error);
                int fileWarnings = diagnostics.Count(diagnostic => diagnostic.Severity == DiagnosticSeverity.Warning);
                errors += fileErrors; warnings += fileWarnings; if (fileErrors > 0) errorFiles++;
                results.Add(new
                {
                    path, byte_length = bytes.Length, sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),
                    error_count = fileErrors, warning_count = fileWarnings, file_scoped_namespace_count = fileScopedCount,
                    global_using_count = globalUsingCount, observed_feature_node_counts = featureCounts,
                    diagnostics = diagnostics.Select(diagnostic => new { id = diagnostic.Id, severity = diagnostic.Severity.ToString(), text = diagnostic.ToString() }).ToArray()
                });
            }
            var record = new
            {
                language_version = options.LanguageVersion.ToString(), roslyn_assembly = typeof(CSharpSyntaxTree).Assembly.FullName,
                input_paths = inputs, mode = smoke ? "single_file_smoke" : "all_discovered_source_files", discovered_source_files = discovered,
                inspected_source_files = files.Count, syntax_error_count = errors, syntax_warning_count = warnings, files_with_syntax_errors = errorFiles,
                syntax_passed = errors == 0, preprocessor_symbols = Array.Empty<string>(), excluded_directory_segments = new[] { "bin", "obj" },
                file_scoped_namespace_count = fileScopedNamespaces, performed_comprehensive_language_feature_compatibility_validation = false,
                global_using_count = globalUsingTotal, observed_feature_node_counts = featureTotals,
                sdk_supports_observed_feature_kind_names = observedFeatureKinds.ToDictionary(kind => kind, sdkSyntaxKindNames.Contains),
                performed_symbol_binding = false, performed_compilation_or_emit = false, performed_unity_import = false,
                loaded_or_executed_game_assemblies = false, executed_game_functions = false,
                limitations = new[]
                {
                    "Roslyn ParseText/GetDiagnostics checks only source grammar under CSharp9, not references, symbol binding, API support or code generation.",
                    "The SDK Roslyn version is not Unity's actual embedded compiler binary; no Unity compilation/import was performed.",
                    "No preprocessor symbols were supplied; inactive conditional branches are not parsed as active source.",
                    "CSharp9 syntax acceptance does not validate Unity-specific unsupported language/runtime/serialization features.",
                    "An observed original file-scoped-namespace control has zero SyntaxTree diagnostics under CSharp9. ParseText/GetDiagnostics alone does not enforce every language-version feature gate; selected modern constructs are counted separately as AST observations."
                },
                files = results
            };
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            File.WriteAllText(output, JsonSerializer.Serialize(record, new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
            Console.WriteLine(JsonSerializer.Serialize(new { saved = output, record.mode, record.language_version, record.discovered_source_files,
                record.inspected_source_files, record.syntax_error_count, record.syntax_warning_count, record.files_with_syntax_errors, record.syntax_passed,
                record.file_scoped_namespace_count, record.global_using_count, record.observed_feature_node_counts,
                record.performed_symbol_binding, record.performed_compilation_or_emit, record.performed_unity_import }));
            return errors == 0 ? 0 : 2;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex.ToString()); return 1; }
    }
}
