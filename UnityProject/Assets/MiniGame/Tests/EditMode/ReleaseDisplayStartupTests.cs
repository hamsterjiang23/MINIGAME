using System.Reflection;
using NUnit.Framework;

namespace MiniGame.UnityHost.Tests
{
    public sealed class ReleaseDisplayStartupTests
    {
        private static bool UsesNative(params string[] arguments) => (bool)typeof(ReleaseDisplayStartup)
            .GetMethod("ShouldUseNativeResolution", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { arguments });

        [Test]
        public void OrdinaryAndGraphicalBatchStartsUseTheSameNativeResolutionPath()
        {
            Assert.That(UsesNative("NORM.exe"), Is.True);
            Assert.That(UsesNative("NORM.exe", "-batchmode", "-force-d3d11", "-logFile", "startup.log"), Is.True);
        }

        [TestCase("-nographics")]
        [TestCase("-screen-fullscreen")]
        [TestCase("-SCREEN-FULLSCREEN")]
        [TestCase("-screen-width")]
        [TestCase("-screen-height")]
        [TestCase("-window-mode")]
        public void ExplicitDisplayOverridesArePreserved(string option)
        {
            Assert.That(UsesNative("NORM.exe", option, "0"), Is.False);
        }

        [Test]
        public void UnrelatedArgumentsAndFlagTextInsideAnOutputFilenameDoNotDisableNativeResolution()
        {
            Assert.That(UsesNative("NORM.exe", "-logFile", "C:/checks/-screen-width.log"), Is.True);
        }
    }
}
