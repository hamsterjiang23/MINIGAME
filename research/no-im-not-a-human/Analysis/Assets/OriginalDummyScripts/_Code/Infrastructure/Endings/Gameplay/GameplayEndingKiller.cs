using _Code.Characters;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Data;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingKiller : AGameplayEnding
	{
		private IDayNightController _dayNightController;

		private ICharactersManager _charactersManager;

		private IEndingSODataProvider _endingSoDataProvider;

		private bool _isReady;

		public override int Priority => 0;

		public override EEnding Ending => EEnding.TestEnding;

		public override bool AreConditionsMet => false;

		protected override void TriggerInner()
		{
		}

		public void InitModules(IDayNightController dayNightController, ICharactersManager charactersManager, IEndingSODataProvider endingSoDataProvider)
		{
		}

		public void UnlockCartoon()
		{
		}
	}
}
