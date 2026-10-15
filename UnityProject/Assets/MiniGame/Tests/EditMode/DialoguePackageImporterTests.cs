using System;
using System.IO;
using MiniGame.Presentation.Authoring;
using MiniGame.UnityHost.Editor;
using NUnit.Framework;
using UnityEditor;

namespace MiniGame.UnityHost.Tests
{
    public sealed class DialoguePackageImporterTests
    {
        [Test]
        public void RejectedPackagePreservesActiveSourceProjectAndAssetBytes()
        {
            var active = DialoguePackageImporter.EnsureDefaultPackage();
            var source = active.PackageSource;
            var project = active.Project;
            var hash = active.SourceHash;
            var activeBytes = File.ReadAllBytes(DialoguePackageImporter.ActiveAssetPath);
            var temporary = Path.Combine(Path.GetTempPath(), "MiniGame-invalid-" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                File.WriteAllText(temporary, "{}");
                Assert.Throws<DialoguePackageException>(() => DialoguePackageImporter.ImportPackage(temporary));
                var after = AssetDatabase.LoadAssetAtPath<DialoguePackageAsset>(DialoguePackageImporter.ActiveAssetPath);
                Assert.That(after, Is.SameAs(active));
                Assert.That(after.PackageSource, Is.SameAs(source));
                Assert.That(after.Project, Is.SameAs(project));
                Assert.That(after.SourceHash, Is.EqualTo(hash));
                Assert.That(File.ReadAllBytes(DialoguePackageImporter.ActiveAssetPath), Is.EqualTo(activeBytes));
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        [Test]
        public void NativeImportIsIdempotentAndKeepsCanonicalSourceSeparateFromExecutableYarn()
        {
            var first = DialoguePackageImporter.ImportPackage(Path.GetFullPath(DialoguePackageImporter.DefaultPackagePath), false);
            var second = DialoguePackageImporter.ImportPackage(Path.GetFullPath(DialoguePackageImporter.DefaultPackagePath), false);
            Assert.That(second, Is.SameAs(first));
            Assert.That(first.Project.NodeNames.Length, Is.GreaterThan(0));
            var compiled = DialoguePackageImporter.ValidatePackage(first.PackageSource.text);
            Assert.That(first.SourceHash, Is.EqualTo(compiled.SourceHash));
            var directory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(first.Project));
            Assert.That(File.ReadAllText(Path.Combine(directory, "Runtime.yarn")), Is.EqualTo(compiled.RuntimeYarn));
            Assert.That(first.PackageSource.text, Is.EqualTo(File.ReadAllText(DialoguePackageImporter.DefaultPackagePath)));
            Assert.That(File.ReadAllText(Path.Combine(directory, "Runtime.yarnproject")), Does.Contain("\"Runtime.yarn\""));
        }
    }
}
