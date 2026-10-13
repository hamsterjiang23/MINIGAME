using System;
using _Code.Characters;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Randomization
{
	public sealed class GameplayRandomizer : ASavableClass<GameplayRandomizerSaveData>, IGameplayRandomizer, IDisposable
	{
		private GameplayRandomizerSaveData _saveData;

		private readonly RandomGenerationSettingsSOData _randomGenerationSettings;

		private readonly CharacterSOData[] _charactersList;

		private readonly IDataModelService _dataModelService;

		public int CompletedGameTimesCountAtBegin => 0;

		public GameplayRandomizer(IRandomGenerationSettingsSODataProvider randomGenerationSettingsSODataProvider, ICharactersSODataProvider charactersSODataProvider, IDataModelService dataModelService)
		{
		}

		private void Init()
		{
		}

		public (CharacterSOData, int)[] GetRandomCharactersForDay(int day, (int, int) charactersCount)
		{
			return null;
		}

		public int GetFirstEmptyVisitSlot(int day)
		{
			return 0;
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}
	}
}
