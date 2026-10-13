namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingDeath : AGameplayEnding
	{
		private bool _isReadyToEnd;

		private GameplayEndingManagerSaveData _saveData;

		public override int Priority => 10;

		public override EEnding Ending => EEnding.TheDeath;

		public override bool AreConditionsMet => _isReadyToEnd;

		protected override void TriggerInner()
		{
		}

		public void InitModules(GameplayEndingManagerSaveData saveData)
		{
			_saveData = saveData;
		}

		public void CompleteCondition(int index)
		{
			if ((index > 0 && _saveData.ProphetConditions[index - 1]) || index == 0)
			{
				_saveData.ProphetConditions[index] = true;
			}
		}

		public bool CheckCondition(int visitIndex)
		{
			if (visitIndex != 0)
			{
				return _saveData.ProphetConditions[visitIndex - 1];
			}
			return true;
		}

		public void EnableVideoEnd()
		{
			_isReadyToEnd = true;
		}

		public void ReinitSaveData(GameplayEndingManagerSaveData saveData)
		{
			_saveData = saveData;
		}
	}
}
