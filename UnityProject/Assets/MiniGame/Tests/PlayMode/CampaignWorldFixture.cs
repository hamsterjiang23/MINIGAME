using MiniGame.Presentation;
using NUnit.Framework;

namespace MiniGame.UnityHost.Tests
{
    internal static class CampaignWorldFixture
    {
        // Physical-room regressions start from an explicit world checkpoint, independently of new-game presentation.
        public static void Restore(MiniGameBootstrap host)
        {
            var session = MiniGameSessionHost.Current;
            Assert.That(session, Is.Not.Null, "The combined campaign must provide its actual rules and content.");
            host.RestoreCheckpoint(new GameRuntime(session.Rules, session.Content, true).Save());
        }
    }
}
