using System;
using UnityEngine;
using Yarn.Unity;

namespace MiniGame.UnityHost
{
    /// <summary>References one validated immutable source package and its native Yarn project.</summary>
    public sealed class DialoguePackageAsset : ScriptableObject
    {
        public const string ResourceKey = "MiniGame/ActiveDialoguePackage";
        [SerializeField] private TextAsset packageSource;
        [SerializeField] private YarnProject project;
        [SerializeField] private string sourceHash = "";

        public TextAsset PackageSource => packageSource;
        public YarnProject Project => project;
        public string SourceHash => sourceHash;

        public void Configure(TextAsset source, YarnProject yarnProject, string hash)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (yarnProject == null) throw new ArgumentNullException(nameof(yarnProject));
            if (string.IsNullOrWhiteSpace(hash)) throw new ArgumentException("A source hash is required.", nameof(hash));
            packageSource = source;
            project = yarnProject;
            sourceHash = hash;
        }
    }
}
