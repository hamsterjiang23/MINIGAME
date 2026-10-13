using _Code.Characters;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.StateObjects;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingStayAliveAlone : AGameplayEnding
	{
		private IDayNightController _dayNightController;

		private ICharactersManager _charactersManager;

		private IStateObjectController _stateObjectController;

		private int _windowsNailedUp;

		public override int Priority => 0;

		public override EEnding Ending => EEnding.TestEnding;

		public override bool AreConditionsMet => false;

		public int NailedUpWindowsCount => 0;

		public void InitModules(IDayNightController dayNightController, ICharactersManager charactersManager, IStateObjectController stateObjectController)
		{
		}

		protected override void TriggerInner()
		{
		}

		public bool NailUpWindow()
		{
			return false;
		}
	}
}
