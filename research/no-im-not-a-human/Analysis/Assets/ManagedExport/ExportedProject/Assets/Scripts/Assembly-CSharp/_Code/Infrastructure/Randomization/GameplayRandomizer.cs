using System;
using System.Collections.Generic;
using System.Linq;
using _Code.Characters;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DataModel.Models.Meta;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Randomization
{
	public sealed class GameplayRandomizer : ASavableClass<GameplayRandomizerSaveData>, IGameplayRandomizer, IDisposable
	{
		private GameplayRandomizerSaveData _saveData = new GameplayRandomizerSaveData();

		private readonly RandomGenerationSettingsSOData _randomGenerationSettings;

		private readonly CharacterSOData[] _charactersList;

		private readonly IDataModelService _dataModelService;

		public int CompletedGameTimesCountAtBegin => _saveData.CompletedGameTimesCountAtBegin;

		public GameplayRandomizer(IRandomGenerationSettingsSODataProvider randomGenerationSettingsSODataProvider, ICharactersSODataProvider charactersSODataProvider, IDataModelService dataModelService)
		{
			_randomGenerationSettings = randomGenerationSettingsSODataProvider.RandomGenerationSettingsSOData;
			_charactersList = charactersSODataProvider.CharactersData;
			_dataModelService = dataModelService;
			LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
			if (!dataModelService.GameSaveDataHandler.NeedToLoadGame)
			{
				Init();
			}
		}

		private void Init()
		{
			if (_saveData.CompletedGameTimesCountAtBegin == -1)
			{
				_saveData.CompletedGameTimesCountAtBegin = _dataModelService.MetaPrefsDataHandler.GetCompletedGameTimesCount(EGameCompleteType.ExcludeEarlyEndings);
			}
		}

		public (CharacterSOData character, int order)[] GetRandomCharactersForDay(int day, (int, int) charactersCount)
		{
			int num = _randomGenerationSettings.RandomGuestsNumberByNights[day];
			List<(CharacterSOData, int)> list = new List<(CharacterSOData, int)>(num);
			List<int> list2 = (from x in _charactersList
				where x.IsPreset
				select x.Visits.FirstOrDefault((CharacterVisitData y) => y.DayNumber == day)?.GuestNumber ?? (-1)).ToList();
			if (_saveData.ExtraCharactersVisitOrders.TryGetValue(day, out var value))
			{
				list2.AddRange(value);
			}
			for (int num2 = 0; num2 < num; num2++)
			{
				CharacterSOData randomElement = _charactersList.Where((CharacterSOData x) => !x.IsPreset && !_saveData.UsedCharacters.Contains(x.CharacterType) && x.AllowedDays.Contains(day) && (x.MinimumGamesCompletedToAppear == 0 || _saveData.CompletedGameTimesCountAtBegin >= x.MinimumGamesCompletedToAppear)).GetRandomElement();
				_saveData.UsedCharacters.Add(randomElement.CharacterType);
				int num3;
				for (num3 = 1; list2.Contains(num3); num3++)
				{
				}
				list2.Add(num3);
				list.Add((randomElement, num3));
			}
			return list.ToArray();
		}

		public int GetFirstEmptyVisitSlot(int day)
		{
			List<int> list = (from x in _charactersList
				where x.IsPreset
				select x.Visits.FirstOrDefault((CharacterVisitData y) => y.DayNumber == day)?.GuestNumber ?? (-1)).ToList();
			if (_saveData.ExtraCharactersVisitOrders.TryGetValue(day, out var value))
			{
				list.AddRange(value);
			}
			int num;
			for (num = 1; list.Contains(num); num++)
			{
			}
			if (!_saveData.ExtraCharactersVisitOrders.ContainsKey(day))
			{
				_saveData.ExtraCharactersVisitOrders.Add(day, new List<int>());
			}
			_saveData.ExtraCharactersVisitOrders[day].Add(num);
			return num;
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
			_saveData = saver.LoadKey(this);
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}
	}
}
