using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    public sealed class TerminalPowerTimelineTests
    {
        [Test]
        public void BootUsesReferenceBurstsAndPartitionsAndBecomesRunningOnlyAfterLoading()
        {
            Assert.That(TerminalPowerTimeline.Boot(0).Cursor, Is.True);
            Assert.That(TerminalPowerTimeline.Boot(.7f).ConsoleText, Does.Contain("Self-test"));
            Assert.That(TerminalPowerTimeline.Boot(1.2f).ConsoleText, Does.Contain("66..65..64..63.."));
            Assert.That(TerminalPowerTimeline.Boot(4).ConsoleText, Does.Contain("Tests completed."));
            Assert.That(TerminalPowerTimeline.Boot(5.2f).ConsoleText, Does.EndWith(">>> BOOT"));
            Assert.That(TerminalPowerTimeline.Boot(6.5f).ConsoleText, Does.Contain("Console ready."));
            Assert.That(TerminalPowerTimeline.Boot(7.5f).Phase, Is.EqualTo(TerminalPowerPhase.Loading));
            Assert.That(TerminalPowerTimeline.Boot(7.5f).LoadedRegions, Is.Zero);
            Assert.That(TerminalPowerTimeline.Boot(7.62f).LoadedRegions, Is.EqualTo(1));
            Assert.That(TerminalPowerTimeline.Boot(8.86f).LoadedRegions, Is.EqualTo(14));
            Assert.That(TerminalPowerTimeline.Boot(8.97f).Phase, Is.EqualTo(TerminalPowerPhase.Loading));
            Assert.That(TerminalPowerTimeline.Boot(8.99f).Phase, Is.EqualTo(TerminalPowerPhase.Running));
        }
        [Test]
        public void ShutdownKeepsConsoleHaltBeforeOpticalCollapseAndOff()
        {
            Assert.That(TerminalPowerTimeline.Shutdown(.4f).ConsoleText, Is.EqualTo("Closing session..."));
            Assert.That(TerminalPowerTimeline.Shutdown(1.3f).ConsoleText, Is.EqualTo("Session closed."));
            Assert.That(TerminalPowerTimeline.Shutdown(2).ConsoleText, Does.Contain("System halted."));
            Assert.That(TerminalPowerTimeline.Shutdown(2.5f).Phase, Is.EqualTo(TerminalPowerPhase.Safe));
            Assert.That(TerminalPowerTimeline.Shutdown(2.5f).Cursor, Is.True, "The HTML safe screen freezes its visible cursor.");
            Assert.That(TerminalPowerTimeline.Shutdown(3.125f).Phase, Is.EqualTo(TerminalPowerPhase.Collapse));
            Assert.That(TerminalPowerTimeline.Shutdown(3.505f).Collapse, Is.EqualTo(.5f).Within(.0001f));
            Assert.That(TerminalPowerTimeline.Shutdown(3.885f).Phase, Is.EqualTo(TerminalPowerPhase.Off));
            Assert.That(TerminalPowerTimeline.Boot(float.NaN).Phase, Is.EqualTo(TerminalPowerPhase.Boot));
        }
        [Test]
        public void DelayedFramesCannotSkipClosingSessionOrAnyFollowingShutdownOutput()
        {
            float elapsed = 0;
            elapsed = TerminalPowerTimeline.AdvanceShutdown(elapsed, 2);
            Assert.That(TerminalPowerTimeline.Shutdown(elapsed).ConsoleText, Is.EqualTo("Closing session..."));
            elapsed = TerminalPowerTimeline.AdvanceShutdown(elapsed, 2);
            Assert.That(TerminalPowerTimeline.Shutdown(elapsed).ConsoleText, Is.EqualTo("Session closed."));
            elapsed = TerminalPowerTimeline.AdvanceShutdown(elapsed, 2);
            Assert.That(TerminalPowerTimeline.Shutdown(elapsed).ConsoleText, Does.StartWith("Session closed.\n\nSystem halted."));
            elapsed = TerminalPowerTimeline.AdvanceShutdown(elapsed, 2);
            Assert.That(TerminalPowerTimeline.Shutdown(elapsed).Phase, Is.EqualTo(TerminalPowerPhase.Safe));
            elapsed = TerminalPowerTimeline.AdvanceShutdown(elapsed, 2);
            Assert.That(TerminalPowerTimeline.Shutdown(elapsed).Phase, Is.EqualTo(TerminalPowerPhase.Collapse));
            elapsed = TerminalPowerTimeline.AdvanceShutdown(elapsed, 2);
            Assert.That(TerminalPowerTimeline.Shutdown(elapsed).Phase, Is.EqualTo(TerminalPowerPhase.Off));
            Assert.That(TerminalPowerTimeline.AdvanceShutdown(.5f, float.NaN), Is.EqualTo(.5f));
        }
    }
}
