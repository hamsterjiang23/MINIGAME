using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.StateObjects;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingMushroom : AGameplayEnding
	{
		private GameplayEndingManagerSaveData _saveData;

		private IStateObjectController _stateObjectController;

		private IDayNightController _dayNightController;

		private bool _cartoonUnlocked;

		public override int Priority => 0;

		public override EEnding Ending => EEnding.TestEnding;

		public override bool AreConditionsMet => false;

		protected override void TriggerInner()
		{
		}

		public void InitModules(GameplayEndingManagerSaveData saveData, IStateObjectController stateObjectController, IDayNightController dayNightController)
		{
		}

		public void WatchClock()
		{
		}

		public void FindApple()
		{
		}

		public void EatMushroom()
		{
		}

		public void OpenHatch()
		{
		}

		public bool CheckCondition(int visitIndex)
		{
			return false;
		}

		public void ReinitSaveData(GameplayEndingManagerSaveData saveData)
		{
		}

		public void UnlockCartoon()
		{
		}
	}
}
