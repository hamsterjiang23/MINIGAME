using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Randomization;
using _Code.Infrastructure.Rooms;
using _Code.Rooms;
using _Code.Utils;
using _Scripts.Services.DataModel;

namespace _Code.Characters
{
	public sealed class CharactersManager : ASavableClass<CharactersManagerSaveData>, ICharactersManager, IDisposable
	{
		private CharactersManagerSaveData _saveData = new CharactersManagerSaveData();

		private readonly CharacterSOData[] _characterData;

		private readonly IGameEventsManager _gameEventsManager;

		public Func<float> GetImpostersPercent;

		private readonly OtherGameSOData _otherData;

		private readonly IDataModelService _dataModelService;

		private readonly RandomGenerationSettingsSOData _randomGenerationSettings;

		private ECharacterType _currentDialogCharacter;

		public int ImpostersCount => CharactersInside.Count((ECharacterType x) => GetCharacter(x).IsImposter);

		public int InnocentsCount => CharactersInside.Count((ECharacterType x) => !GetCharacter(x).IsImposter);

		public int Count => CharactersInside.Count;

		public int InnocentsKilledCount => _saveData.InnocentsKilledByPlayer.Count;

		public int ImpostersKilledCount => _saveData.KilledCharacters.Count((ECharacterType x) => GetCharacter(x).IsImposter);

		public int EverImposterInHouse => ImpostersCount + ImpostersKilledCount;

		public IReadOnlyList<ECharacterType> CharactersInside => _saveData.CharactersInside;

		public int KilledByImpostersCount => _saveData.KilledByImposters.Count;

		public event Func<ERoom, ARoom> GetRoom;

		public event Action ScreamerShowed;

		public event Action<ECharacterType> CharacterRemoved;

		public event Action<ECharacterType, bool> CharacterAdded;

		public event Func<(int Day, ETimeOfDay CurrentTimeOfDay, float LastChange)> GetBaseDayNightData;

		public CharacterSOData GetCharacter(ECharacterType characterType)
		{
			return _characterData.FirstOrDefault((CharacterSOData x) => x.CharacterType == characterType);
		}

		public CharactersManager(ICharactersSODataProvider dataProvider, IGameEventsManager gameEventsManager, IDataModelService dataModelService, IOtherGameSODataProvider otherGameSoDataProvider, IRandomGenerationSettingsSODataProvider randomGenerationSettingsSoDataProvider)
		{
			_characterData = dataProvider.CharactersData;
			_gameEventsManager = gameEventsManager;
			_otherData = otherGameSoDataProvider.OtherGameData;
			_dataModelService = dataModelService;
			_randomGenerationSettings = randomGenerationSettingsSoDataProvider.RandomGenerationSettingsSOData;
			InitDevUtils().Forget();
			_gameEventsManager.GetCharactersAndImpostersCount -= OnGotCharactersAndImpostersCount;
			_gameEventsManager.GetCharactersAndImpostersCount += OnGotCharactersAndImpostersCount;
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}

		private (int total, int imposters) OnGotCharactersAndImpostersCount()
		{
			return (total: _saveData.CharactersInside.Count, imposters: _saveData.CharactersInside.Count((ECharacterType x) => GetCharacter(x).IsImposter));
		}

		private async UniTask InitDevUtils()
		{
		}

		public void LetIn(CharacterSOData character, bool exileOldCharacterFromThisPlace = false)
		{
			ECharacterType eCharacterType = _saveData.CharactersInside.FirstOrDefault((ECharacterType x) => GetCharacter(x).Place == character.Place);
			if (eCharacterType != ECharacterType.Sanya && eCharacterType != character.CharacterType)
			{
				if (exileOldCharacterFromThisPlace)
				{
					ExileCharacter(eCharacterType);
				}
				else if (_saveData.CharactersInside.Select((ECharacterType x) => GetCharacter(x).Place).Contains(character.Place))
				{
					Debug.LogError("This place is already taken");
					return;
				}
			}
			_saveData.CharactersInside.Add(character.CharacterType);
			_saveData.CharactersCameInDays.Add(character.CharacterType, GetBaseDayNightData?.Invoke().Day ?? 0);
			if (!_saveData.FEMARating.ContainsKey(character.CharacterType))
			{
				_saveData.FEMARating.Add(character.CharacterType, (character.IsImposter ? 0f : _otherData.IsHumanRatingValue) + character.FEMARatingBonus);
			}
			(GetRoom?.Invoke(character.Room))?.AddCharacter(character.CharacterType);
			if (character.CharacterType == ECharacterType.Sexy)
			{
				(int Day, ETimeOfDay CurrentTimeOfDay, float LastChange)? obj = GetBaseDayNightData?.Invoke();
				_gameEventsManager.GenerateBabyEvents(obj?.Day ?? 0);
			}
			GenerateStatus(character);
			_saveData.AreCharactersImposters.Add(character.CharacterType, character.IsImposter);
			ECharacterType characterType = character.CharacterType;
			if (characterType == ECharacterType.CultistOne || characterType == ECharacterType.CultistTwo || characterType == ECharacterType.CultistThree)
			{
				_dataModelService.MetaPrefsDataHandler.MeetCharacter(character.CharacterType);
			}
			CharacterAdded?.Invoke(character.CharacterType, arg2: false);
		}

		private void OnLoadLetIn(CharacterSOData character)
		{
			(GetRoom?.Invoke(character.Room))?.AddCharacter(character.CharacterType, isAddSilently: true);
			CharacterAdded?.Invoke(character.CharacterType, arg2: true);
			if (_saveData.AreCharactersImposters.ContainsKey(character.CharacterType))
			{
				character.IsImposter = _saveData.AreCharactersImposters[character.CharacterType];
			}
			else
			{
				_saveData.AreCharactersImposters.Add(character.CharacterType, character.IsImposter);
			}
		}

		private void OnLoadLetInAndKill(CharacterSOData character)
		{
			if (_saveData.AreCharactersImposters.ContainsKey(character.CharacterType))
			{
				character.IsImposter = _saveData.AreCharactersImposters[character.CharacterType];
			}
			else
			{
				_saveData.AreCharactersImposters.Add(character.CharacterType, character.IsImposter);
			}
			ARoom obj = GetRoom?.Invoke(character.Room);
			obj?.AddCharacter(character.CharacterType, isAddSilently: true);
			obj?.KillCharacter(character.CharacterType);
		}

		private void GenerateStatus(CharacterSOData character)
		{
			if (character.IsStatusRandomlyGenerated)
			{
				float num = (float)_saveData.CharactersInside.Count((ECharacterType x) => GetCharacter(x).IsImposter) / (float)_saveData.CharactersInside.Count;
				if (num < _randomGenerationSettings.MinImpostersPercent)
				{
					character.IsImposter = true;
				}
				else if (num > _randomGenerationSettings.MaxImpostersPercent)
				{
					character.IsImposter = false;
				}
				else
				{
					character.IsImposter = UnityEngine.Random.Range(0, 2) == 0;
				}
			}
		}

		public void Refuse(CharacterSOData character)
		{
			_saveData.RefusedCharacters.Add(character.CharacterType);
		}

		public void Kill(CharacterSOData character, bool isByPlayer = false)
		{
			_saveData.CharactersInside.Remove(character.CharacterType);
			_saveData.FEMARating.Remove(character.CharacterType);
			if (_saveData.CharacterWithPovistka.Contains(character.CharacterType))
			{
				_saveData.CharacterWithPovistka.Remove(character.CharacterType);
			}
			_saveData.KilledCharacters.Add(character.CharacterType);
			if (isByPlayer && !character.IsImposter)
			{
				_saveData.InnocentsKilledByPlayer.Add(character.CharacterType);
			}
			if (!isByPlayer)
			{
				_saveData.KilledByImposters.Add(character.CharacterType);
			}
			_saveData.KilledTodayCharacters.Add(character.CharacterType);
			(GetRoom?.Invoke(character.Room))?.KillCharacter(character.CharacterType);
			if (character.IsImposter && UnityEngine.Random.value < _otherData.ScreamerOnImposterChance)
			{
				ScreamerShowed?.Invoke();
			}
			if (isByPlayer)
			{
				if (character.IsImposter)
				{
					_dataModelService.MetaPrefsDataHandler.KillImposter(character.CharacterType);
				}
				else
				{
					_dataModelService.MetaPrefsDataHandler.KillInnocent(character.CharacterType);
				}
			}
			CharacterRemoved?.Invoke(character.CharacterType);
			if (isByPlayer && character.IsImposter)
			{
				SteamAchievements.Unlock("KILL_IMPOSTER");
			}
			if (isByPlayer && !character.IsImposter)
			{
				SteamAchievements.Unlock("KILL_HUMAN");
			}
		}

		public void Kill(ECharacterType character)
		{
			ECharacterType eCharacterType = CharactersInside.FirstOrDefault((ECharacterType x) => x == character);
			if (eCharacterType != ECharacterType.Sanya)
			{
				Kill(GetCharacter(eCharacterType));
			}
		}

		public void KillTomorrow(CharacterSOData character)
		{
			_saveData.SelfKillNextDayCharacters.Add(character.CharacterType);
		}

		public void ExileAfterTomorrow(CharacterSOData character)
		{
			Func<(int Day, ETimeOfDay CurrentTimeOfDay, float LastChange)> func = GetBaseDayNightData;
			int key = ((func != null) ? (func().Item1 + 2) : (-1));
			if (_saveData.ExilesOnDays.ContainsKey(key))
			{
				_saveData.ExilesOnDays[key].Add(character.CharacterType);
				return;
			}
			_saveData.ExilesOnDays[key] = new List<ECharacterType> { character.CharacterType };
		}

		public bool EverRefused()
		{
			return _saveData.RefusedCharacters.Count > 0;
		}

		public void RunMorningOfSuicides()
		{
			foreach (ECharacterType selfKillNextDayCharacter in _saveData.SelfKillNextDayCharacters)
			{
				Kill(selfKillNextDayCharacter);
			}
			_saveData.SelfKillNextDayCharacters.Clear();
		}

		public CharacterSOData[] ExilyByFEMA(int count)
		{
			List<ECharacterType> list = new List<ECharacterType>(count);
			List<ECharacterType> favoritesList = GetFavoritesList();
			bool flag = false;
			for (int i = 0; i < count; i++)
			{
				List<ECharacterType> list2 = new List<ECharacterType>();
				int num = 10;
				list2 = ((_saveData.CharacterWithPovistka.Count > 0) ? _saveData.CharacterWithPovistka : favoritesList);
				list2.RemoveAll((ECharacterType x) => (from y in _saveData.CharactersCameInDays
					where y.Value > GetBaseDayNightData?.Invoke().Day
					select y.Key).Contains(x));
				ECharacterType randomElement = list2.GetRandomElement();
				while (list.Contains(randomElement) || randomElement == ECharacterType.Sanya)
				{
					randomElement = list2.GetRandomElement();
					num--;
					if (num <= 0)
					{
						break;
					}
				}
				if (num > 0)
				{
					list.Add(randomElement);
					flag = true;
					if (_saveData.CharacterWithPovistka.Contains(randomElement))
					{
						_saveData.CharacterWithPovistka.Remove(randomElement);
					}
					ExileCharacter(list[i]);
				}
			}
			IEnumerable<CharacterSOData> source = list.Select((ECharacterType x) => _characterData.FirstOrDefault((CharacterSOData y) => y.CharacterType == x));
			if (!flag)
			{
				return null;
			}
			return source.ToArray();
		}

		private List<ECharacterType> GetFavoritesList()
		{
			List<ECharacterType> list = new List<ECharacterType>(5);
			List<ECharacterType> list2 = ((UnityEngine.Random.Range(0f, 1f) > _otherData.BadFemaRatingChance) ? (from x in _saveData.FEMARating
				orderby x.Value
				select x.Key).ToList() : (from x in _saveData.FEMARating
				orderby x.Value descending
				select x.Key).ToList());
			int num = Mathf.Min(5, list2.Count);
			for (int num2 = 0; num2 < num; num2++)
			{
				long num3 = MyMath.Fibonacci(num - num2 + 2);
				for (int num4 = 0; num4 < num3; num4++)
				{
					list.Add(list2[num2]);
				}
			}
			return list;
		}

		public CharacterSOData GetRandomCharacterToExile()
		{
			ECharacterType randomElement = CharactersInside.Where((ECharacterType x) => GetCharacter(x).CanExile).ToArray().GetRandomElement();
			ExileCharacter(randomElement);
			return GetCharacter(randomElement);
		}

		public void ExileCharacter(ECharacterType character)
		{
			if (_saveData.CharactersInside.Contains(character))
			{
				_saveData.CharactersInside.Remove(character);
				_saveData.FEMARating.Remove(character);
				_saveData.ExiledCharacters.Add(character);
				if (_saveData.CharacterWithPovistka.Contains(character))
				{
					_saveData.CharacterWithPovistka.Remove(character);
				}
				(GetRoom?.Invoke(GetCharacter(character).Room))?.ExileCharacter(character);
				CharacterRemoved?.Invoke(GetCharacter(character).CharacterType);
			}
		}

		private CharacterSOData[] GetPossibleCharactersToKill()
		{
			return (from x in _saveData.CharactersCameInDays
				where _saveData.CharactersInside.Contains(x.Key)
				where x.Value + 1 < GetBaseDayNightData?.Invoke().Day
				select GetCharacter(x.Key) into x
				where !x.IsImposter
				select x).ToArray();
		}

		public int GetPossibleCharactersToKillCount()
		{
			return GetPossibleCharactersToKill().Length;
		}

		public bool IsCharacterAliveInside(ECharacterType characterType)
		{
			return _saveData.CharactersInside.Contains(characterType);
		}

		public void KillRandom()
		{
			CharacterSOData[] possibleCharactersToKill = GetPossibleCharactersToKill();
			if (possibleCharactersToKill.Length != 0)
			{
				CharacterSOData randomElement = possibleCharactersToKill.GetRandomElement();
				Kill(randomElement);
			}
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
			_saveData = saver.LoadKey(this);
			foreach (ECharacterType item in new List<ECharacterType>(CharactersInside))
			{
				OnLoadLetIn(GetCharacter(item));
			}
			foreach (ECharacterType item2 in new List<ECharacterType>(_saveData.KilledTodayCharacters))
			{
				OnLoadLetInAndKill(GetCharacter(item2));
			}
			_saveData.CharactersInside = _saveData.CharactersInside.Distinct().ToList();
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}

		public void GivePovistka(ECharacterType character)
		{
			CharacterSOData characterSOData = _characterData.FirstOrDefault((CharacterSOData x) => x.CharacterType == character);
			if (!(characterSOData == null))
			{
				_saveData.CharacterWithPovistka.Add(characterSOData.CharacterType);
			}
		}

		public void KillRoom(CharacterSOData characterOfRoom)
		{
			List<ECharacterType> list = new List<ECharacterType>(_saveData.CharactersInside.Count);
			foreach (ECharacterType item in _saveData.CharactersInside)
			{
				CharacterSOData character = GetCharacter(item);
				if (character.Room == characterOfRoom.Room && character.CharacterType != characterOfRoom.CharacterType)
				{
					list.Add(character.CharacterType);
				}
			}
			foreach (ECharacterType item2 in list)
			{
				Kill(item2);
			}
		}

		public void KillEveryone()
		{
			foreach (ECharacterType item in new List<ECharacterType>(_saveData.CharactersInside))
			{
				if (item != ECharacterType.TV && item != ECharacterType.Player && item != ECharacterType.Empty && item != ECharacterType.Miner)
				{
					Kill(item);
				}
			}
		}

		public void ClearPovistkas()
		{
			_saveData.CharacterWithPovistka.Clear();
		}

		public ERoom GetCharacterRoom(ECharacterType character)
		{
			return _characterData.FirstOrDefault((CharacterSOData x) => x.CharacterType == character)?.Room ?? ERoom.Entrance;
		}

		public bool IsImposter(ECharacterType character)
		{
			return GetCharacter(character).IsImposter;
		}

		public ECharacterPlace GetPlace(ECharacterType character)
		{
			return GetCharacter(character).Place;
		}

		public void BeginNewDay(int day)
		{
			foreach (ECharacterType item in _saveData.CharactersInside)
			{
				_saveData.FEMARating[item] += _otherData.EveryDayIncrementRatingValue;
			}
			if (_saveData.ExilesOnDays.TryGetValue(day, out var value))
			{
				foreach (ECharacterType item2 in value)
				{
					ExileCharacter(item2);
				}
			}
			_saveData.KilledTodayCharacters.Clear();
			if (day == 7 && _saveData.KilledByImposters.Count == 0)
			{
				SteamAchievements.Unlock("WEEK1_IMPOSTERS_DOESNT_KILL");
			}
		}

		public void OnGunStateSelected(bool isTaken)
		{
			if (isTaken)
			{
				_saveData.FEMARating[_currentDialogCharacter] += _otherData.TakeGunAfterCheckRatingValue;
			}
			else
			{
				_saveData.FEMARating[_currentDialogCharacter] += _otherData.DontTakeGunAfterCheckRatingValue;
			}
		}

		public void OnDialogStarted(ECharacterType character)
		{
			_currentDialogCharacter = character;
		}

		public void OnDayDialogTalked(bool hasAlreadyTalkedToday)
		{
			if (hasAlreadyTalkedToday)
			{
				_saveData.FEMARating[_currentDialogCharacter] += _otherData.ListenDialogRepeatRatingValue;
			}
			else
			{
				_saveData.FEMARating[_currentDialogCharacter] += _otherData.ListenDialogFirstTimeRatingValue;
			}
		}
	}
}
