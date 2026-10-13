using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MiniGame.Core;
using MiniGame.Presentation;
using NUnit.Framework;

namespace MiniGame.Presentation.Tests
{
    [TestFixture]
    public sealed class PrototypeContentTests
    {
        [Test]
        public void ContentCoversEveryConfiguredNormalSpecialBranchAndReactionNodeExactlyOnce()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            var meetings = rules.Days.SelectMany(day => day.Meetings).ToArray();
            var rounds = meetings.SelectMany(meeting => meeting.NormalRounds.Concat(new[] { meeting.SpecialRound! })).ToArray();
            var expectedIds = rounds.Select(round => round.NodeId)
                .Concat(rounds.SelectMany(round => round.Responses.Select(response => response.BranchNodeId)))
                .Concat(meetings.SelectMany(meeting => meeting.ReactionNodes.Values)).ToArray();
            Assert.That(content.Nodes.Select(node => node.NodeId), Is.EquivalentTo(expectedIds));
            Assert.That(content.Nodes.Select(node => node.NodeId).Distinct().Count(), Is.EqualTo(expectedIds.Length));
            Assert.That(content.Nodes.All(node => node.Speaker.Length > 0 && node.Text.Length > 0), Is.True);
            foreach (var round in rounds)
            {
                var node = content.Nodes.Single(item => item.NodeId == round.NodeId);
                Assert.That(node.Responses.Select(response => response.Id), Is.EqualTo(round.Responses.Select(response => response.Id)));
            }
            var sideEffectFreeNodes = rounds.SelectMany(round => round.Responses.Select(response => response.BranchNodeId))
                .Concat(meetings.SelectMany(meeting => meeting.ReactionNodes.Values));
            Assert.That(content.Nodes.Where(node => sideEffectFreeNodes.Contains(node.NodeId)).All(node => node.Responses.Count == 0), Is.True);
        }

        [Test]
        public void ContentCannotSilentlyPresentAStaleOrUnknownCoreNode()
        {
            var rules = PrototypeRules.Create();
            var content = new PrototypeContent(rules);
            Assert.Throws<InvalidOperationException>(() => content.GetDialogue(new GameSession(rules).GetPlayerView()));
            var patient = new PatientDefinition("custom-patient", "测试", 1, 0, 3, PatientPresence.Deceased);
            var unrelatedRules = new GameRules("unrelated-content", new[] { patient },
                new[] { new DayDefinition(1, 0, new[] { PrototypeRules.CreateMeeting(patient.Id, 1) }) }, rules.EndingPriority);
            var unrelatedSession = new GameSession(unrelatedRules);
            Assert.That(unrelatedSession.Execute(new CompleteMorningInspection()).Accepted, Is.True);
            Assert.That(unrelatedSession.Execute(new CompleteBriefing()).Accepted, Is.True);
            Assert.That(unrelatedSession.Execute(new Interact(InteractionTarget.Computer)).Accepted, Is.True);
            Assert.Throws<InvalidOperationException>(() => content.GetDialogue(unrelatedSession.GetPlayerView()));
        }

        [TestCase("day1.former-ruler.normal.1")]
        [TestCase("day1.former.ruler.normal.1")]
        [TestCase("患者.答复一")]
        public void YarnIdentifierMappingPreservesTheOriginalUtf8Identifier(string id)
        {
            var encoded = PrototypeYarnSource.EncodeIdentifier(id);
            Assert.That(Encoding.UTF8.GetString(Convert.FromHexString(encoded)), Is.EqualTo(id));
            Assert.That(PrototypeYarnSource.NodeName(id), Does.Match("^[A-Za-z_][A-Za-z0-9_]*$"));
        }

        [Test]
        public void YarnMappingKeepsHyphenAndDotIdentifiersDistinct()
        {
            Assert.That(PrototypeYarnSource.NodeName("day1.former-ruler.normal.1"),
                Is.Not.EqualTo(PrototypeYarnSource.NodeName("day1.former.ruler.normal.1")));
        }

        [Test]
        public void GeneratedYarnHasUniqueTitlesAndBindsChoicesToTheirCoreNodeAndResponse()
        {
            var content = new PrototypeContent(PrototypeRules.Create());
            var source = PrototypeYarnSource.Build(content);
            var titles = Regex.Matches(source, @"^title: ([A-Za-z_][A-Za-z0-9_]*)\r?$", RegexOptions.Multiline)
                .Cast<Match>().Select(match => match.Groups[1].Value).ToArray();
            Assert.That(titles.Distinct().Count(), Is.EqualTo(titles.Length));
            foreach (var node in content.Nodes)
            {
                Assert.That(titles, Does.Contain(PrototypeYarnSource.NodeName(node.NodeId)));
                foreach (var response in node.Responses)
                {
                    var command = "<<select_response \"" + PrototypeYarnSource.EncodeIdentifier(node.NodeId)
                        + "\" \"" + PrototypeYarnSource.EncodeIdentifier(response.Id) + "\">>";
                    Assert.That(source, Does.Contain(command));
                }
            }
            Assert.That(titles, Does.Contain(PrototypeYarnSource.CueNodeName(PresentationKind.Ending)));
        }
    }
}
