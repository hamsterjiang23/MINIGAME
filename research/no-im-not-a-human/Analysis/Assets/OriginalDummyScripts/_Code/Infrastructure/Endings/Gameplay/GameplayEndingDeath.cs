namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingDeath : AGameplayEnding
	{
		private bool _isReadyToEnd;

		private GameplayEndingManagerSaveData _saveData;

		public override int Priority => 0;

		public override EEnding Ending => EEnding.TestEnding;

		public override bool AreConditionsMet => false;

		protected override void TriggerInner()
		{
		}

		public void InitModules(GameplayEndingManagerSaveData saveData)
		{
		}

		public void CompleteCondition(int index)
		{
		}

		public bool CheckCondition(int visitIndex)
		{
			return false;
		}

		public void EnableVideoEnd()
		{
		}

		public void ReinitSaveData(GameplayEndingManagerSaveData saveData)
		{
		}
	}
}
