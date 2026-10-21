using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    public sealed class TerminalPowerTimelineTests
    {
        private const string Prefix = "PMS console 3.14\n\nSelf-test\n";
        private const string Closing = "Closing session...", Closed = "Session closed.", Halted = "System halted.\n\n>>>";
        private const string Ready = "PMS / 3.14\n\nConsole ready.\n";
        private const string FullBoot = Prefix + "\n"
            + "66..65..64..63..62..61..60..59..58..57..56..55..54..53..52..51..\n"
            + "50..49..48..47..46..45..44..43..42..41..40..39..38..37..36..35..\n"
            + "34..33..32..31..30..29..28..27..26..25..24..23..22..21..20..19..\n"
            + "18..17..16..15..14..13..12..11..10..09..08..07..06..05..04..03.."
            + "\n\nTests completed.\n\n>>> BOOT";

        [Test]
        public void BootUsesReferenceBurstsAndPartitionsAndBecomesRunningOnlyAfterLoading()
        {
            Assert.That(TerminalPowerTimeline.Boot(0).Cursor, Is.True);
            Assert.That(TerminalPowerTimeline.Boot(1.05f).ConsoleText, Is.EqualTo(Prefix));
            Assert.That(TerminalPowerTimeline.Boot(1.249f).ConsoleText, Does.EndWith("66..65..64..63.."));
            Assert.That(TerminalPowerTimeline.Boot(4.8f).ConsoleText, Does.Contain("Tests completed."));
            Assert.That(TerminalPowerTimeline.Boot(5.8f).ConsoleText, Is.EqualTo(FullBoot));
            Assert.That(TerminalPowerTimeline.Boot(7.4f).ConsoleText, Is.EqualTo(Ready));
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
            Assert.That(TerminalPowerTimeline.Shutdown(1.1f).ConsoleText, Is.EqualTo(Closing));
            Assert.That(TerminalPowerTimeline.Shutdown(1.7f).ConsoleText, Is.EqualTo(Closed));
            Assert.That(TerminalPowerTimeline.Shutdown(2.4f).ConsoleText, Is.EqualTo(Closed + "\n\n" + Halted));
            Assert.That(TerminalPowerTimeline.Shutdown(2.5f).Phase, Is.EqualTo(TerminalPowerPhase.Safe));
            Assert.That(TerminalPowerTimeline.Shutdown(2.5f).ConsoleText, Is.EqualTo(Halted));
            Assert.That(TerminalPowerTimeline.Shutdown(2.5f).Cursor, Is.True, "The HTML safe screen freezes its visible cursor.");
            Assert.That(TerminalPowerTimeline.Shutdown(3.125f).Phase, Is.EqualTo(TerminalPowerPhase.Collapse));
            Assert.That(TerminalPowerTimeline.Shutdown(3.125f).ConsoleText, Is.EqualTo(Halted));
            Assert.That(TerminalPowerTimeline.Shutdown(3.125f).Cursor, Is.True);
            Assert.That(TerminalPowerTimeline.Shutdown(3.505f).Collapse, Is.EqualTo(.5f).Within(.0001f));
            Assert.That(TerminalPowerTimeline.Shutdown(3.885f).Phase, Is.EqualTo(TerminalPowerPhase.Off));
        }

        [Test]
        public void BootRevealsEveryCharacterInOrderBeforeAndAfterItsClearScreen()
        {
            AssertProgressive(TerminalPowerTimeline.Boot, .625f, 5.875f, "", FullBoot);
            AssertProgressive(TerminalPowerTimeline.Boot, 6.25f, 7.5f, "", Ready);
            Assert.That(TerminalPowerTimeline.Boot(.624f).ConsoleText, Is.Empty);
            Assert.That(TerminalPowerTimeline.Boot(.625f).ConsoleText, Is.EqualTo("P"));
            Assert.That(TerminalPowerTimeline.Boot(.7f).ConsoleText.Length, Is.LessThan(Prefix.Length));
            Assert.That(TerminalPowerTimeline.Boot(5.875f).ConsoleText, Is.Empty);
            Assert.That(TerminalPowerTimeline.Boot(6.249f).ConsoleText, Is.Empty);
            Assert.That(TerminalPowerTimeline.Boot(6.25f).ConsoleText, Is.EqualTo("P"));
            Assert.That(TerminalPowerTimeline.Boot(7.5f).ConsoleText, Is.Empty);
        }

        [Test]
        public void ShutdownTypesBothReplacementPagesThenAppendsTheHaltWithoutRewritingClosed()
        {
            AssertProgressive(TerminalPowerTimeline.Shutdown, .375f, 1.25f, "", Closing);
            AssertProgressive(TerminalPowerTimeline.Shutdown, 1.25f, 1.75f, "", Closed);
            AssertProgressive(TerminalPowerTimeline.Shutdown, 1.75f, 2.5f, Closed, Closed + "\n\n" + Halted);
            Assert.That(TerminalPowerTimeline.Shutdown(.374f).ConsoleText, Is.Empty);
            Assert.That(TerminalPowerTimeline.Shutdown(.375f).ConsoleText, Is.EqualTo("C"));
            Assert.That(TerminalPowerTimeline.Shutdown(1.249f).ConsoleText, Is.EqualTo(Closing));
            Assert.That(TerminalPowerTimeline.Shutdown(1.25f).ConsoleText, Is.EqualTo("S"));
            Assert.That(TerminalPowerTimeline.Shutdown(1.75f).ConsoleText, Is.EqualTo(Closed + "\n"));
            Assert.That(TerminalPowerTimeline.Shutdown(2.5f).ConsoleText, Is.EqualTo(Halted),
                "Safe retains the existing reference snapshot, after the completed halt was visible.");
        }

        [Test]
        public void OrdinaryThirtyFrameUpdatesDoNotRevealWholeConsoleBlocksAtOnce()
        {
            AssertFrameAdditions(TerminalPowerTimeline.Boot, TerminalPowerTimeline.BootDuration, 10);
            AssertFrameAdditions(TerminalPowerTimeline.Shutdown, TerminalPowerTimeline.ShutdownDuration, 3);
        }

        [Test]
        public void DelayedFramesCannotSkipClosingSessionOrAnyFollowingShutdownOutput()
        {
            float elapsed = 0;
            var times = new List<float>();
            var texts = new List<string>();
            var phases = new List<TerminalPowerPhase>();
            for (int step = 0; elapsed < TerminalPowerTimeline.ShutdownDuration && step < 20; step++)
            {
                float next = TerminalPowerTimeline.AdvanceShutdown(elapsed, 2);
                Assert.That(next, Is.GreaterThan(elapsed));
                elapsed = next;
                var frame = TerminalPowerTimeline.Shutdown(elapsed);
                times.Add(elapsed); texts.Add(frame.ConsoleText); phases.Add(frame.Phase);
            }
            Assert.That(times, Does.Contain(.375f).And.Contain(1.25f).And.Contain(1.75f).And.Contain(2.5f).And.Contain(3.125f));
            Assert.That(texts, Does.Contain(Closing).And.Contain(Closed).And.Contain(Closed + "\n\n" + Halted));
            Assert.That(phases, Does.Contain(TerminalPowerPhase.Safe).And.Contain(TerminalPowerPhase.Collapse));
            Assert.That(TerminalPowerTimeline.Shutdown(elapsed).Phase, Is.EqualTo(TerminalPowerPhase.Off));
            Assert.That(elapsed, Is.EqualTo(3.885f));
        }

        [Test]
        public void NonFiniteAndNegativeTimesAreBoundedAndDoNotOverflowCharacterCounts()
        {
            foreach (float invalid in new[] { float.NaN, float.NegativeInfinity, -1f })
            {
                Assert.That(TerminalPowerTimeline.Boot(invalid).Phase, Is.EqualTo(TerminalPowerPhase.Boot));
                Assert.That(TerminalPowerTimeline.Boot(invalid).ConsoleText, Is.Empty);
                Assert.That(TerminalPowerTimeline.Shutdown(invalid).Phase, Is.EqualTo(TerminalPowerPhase.Shutdown));
                Assert.That(TerminalPowerTimeline.Shutdown(invalid).ConsoleText, Is.Empty);
                Assert.That(TerminalPowerTimeline.AdvanceShutdown(.5f, invalid), Is.EqualTo(.5f));
            }
            foreach (float completed in new[] { float.PositiveInfinity, float.MaxValue })
            {
                Assert.That(TerminalPowerTimeline.Boot(completed).Phase, Is.EqualTo(TerminalPowerPhase.Running));
                Assert.That(TerminalPowerTimeline.Shutdown(completed).Phase, Is.EqualTo(TerminalPowerPhase.Off));
                Assert.That(TerminalPowerTimeline.AdvanceShutdown(completed, 1), Is.EqualTo(TerminalPowerTimeline.ShutdownDuration));
            }
            Assert.That(TerminalPowerTimeline.AdvanceShutdown(.5f, float.PositiveInfinity), Is.EqualTo(.875f));
        }

        private static void AssertProgressive(Func<float, TerminalPowerFrame> timeline, float start, float end, string baseline, string complete)
        {
            string previous = baseline;
            for (int step = 0; start + step * .001f < end; step++)
            {
                string current = timeline(start + step * .001f).ConsoleText;
                Assert.That(current, Does.StartWith(previous), "A typing interval cannot delete or replace previously shown characters.");
                Assert.That(current.Length - previous.Length, Is.LessThanOrEqualTo(1), "Each character must have its own reveal time.");
                Assert.That(complete, Does.StartWith(current));
                previous = current;
            }
            Assert.That(previous, Is.EqualTo(complete), "The full message must be visible before its page or phase ends.");
        }

        private static void AssertFrameAdditions(Func<float, TerminalPowerFrame> timeline, float duration, int maximum)
        {
            string previous = "";
            for (int frame = 0; frame / 30f < duration; frame++)
            {
                string current = timeline(frame / 30f).ConsoleText;
                if (current.StartsWith(previous, StringComparison.Ordinal))
                    Assert.That(current.Length - previous.Length, Is.LessThanOrEqualTo(maximum));
                previous = current;
            }
        }
    }
}
