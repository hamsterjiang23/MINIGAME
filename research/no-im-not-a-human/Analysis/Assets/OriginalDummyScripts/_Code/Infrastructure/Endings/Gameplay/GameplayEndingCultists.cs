using _Code.DialogSystem;
using _Code.Infrastructure.StateObjects;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingCultists : AGameplayEnding
	{
		private GameplayEndingManagerSaveData _saveData;

		private IStateObjectController _stateObjectController;

		private IDialogManager _dialogManager;

		public override int Priority => 0;

		public override EEnding Ending => EEnding.TestEnding;

		public override bool AreConditionsMet => false;

		protected override void TriggerInner()
		{
		}

		public void InitModules(GameplayEndingManagerSaveData saveData, IStateObjectController stateObjectController, IDialogManager dialogManager)
		{
		}

		public void BeginCultists()
		{
		}

		public void SaveCultists()
		{
		}

		public bool CheckCondition(int visitIndex)
		{
			return false;
		}

		public void ReinitSaveData(GameplayEndingManagerSaveData saveData)
		{
		}
	}
}
