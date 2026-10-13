using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Characters;
using _Code.Events;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.Randomization;
using _Code.Infrastructure.Updatable;
using _Code.Utils;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.GameEvents;

public sealed class GameEventsManager : ASavableClass<GameEventsManagerSaveData>, IGameEventsManager, IUpdateable, IDisposable
{
	private GameEventsManagerSaveData _saveData = new GameEventsManagerSaveData();

	private readonly GameEventsManagerSOData _data;

	private readonly ICharactersSODataProvider _charactersSODataProvider;

	private readonly INotAHumanSoundService _soundService;

	private readonly IGameplayRandomizer _gameplayRandomizer;

	private readonly IDataModelService _dataModelService;

	private Dictionary<int, List<(CharacterDingDongEvent dingDong, int order)>> _preDingDongEvents;

	private const string DING_DONG_EVENT_PREFIX = "entrance";

	private const string BELLY_EVENT_PREFIX = "belly";

	private float _lastTimeUpdated;

	private readonly float _updateInterval = 1f;

	private bool _arePresetReady;

	private List<string> CompletedEvents => _saveData.CompletedEvents;

	public IUpdateable Updateable => this;

	private Func<int, bool> CheckProphetCondition { get; set; }

	private Func<int, bool> CheckMushrommeaterCondition { get; set; }

	private Func<int, bool> CheckPriestCondition { get; set; }

	private Func<int> GetDay { get; set; }

	public event Action AnyEventCompleted;

	public event Action<string> SpecifiedEventCompleted;

	public event Func<(int day, ETimeOfDay timeOfDay, float timeFromLastChange)> GetBaseDayNightData;

	public event Func<(int, int)> GetCharactersAndImpostersCount;

	public event Action<CharacterDingDongEvent> CharacterCame;

	public event Action CharacterSoonCame;

	public GameEventsManager(IGameEventsManagerSODataProvider dataProvider, ICharactersSODataProvider charactersSODataProvider, IGameplayRandomizer gameplayRandomizer, INotAHumanSoundService soundService, IDataModelService dataModelService)
	{
		_charactersSODataProvider = charactersSODataProvider;
		_soundService = soundService;
		_data = dataProvider.GameEventsManagerData;
		_gameplayRandomizer = gameplayRandomizer;
		_dataModelService = dataModelService;
		_dataModelService.GameSaveDataHandler.SavedAll -= OnSave;
		_dataModelService.GameSaveDataHandler.SavedAll += OnSave;
		LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
		GenerateDingDongEventsForPresetAsync().Forget();
	}

	public void InitializeProphetCondition(Func<int, bool> condition)
	{
		CheckProphetCondition = condition;
	}

	public void InitializeMushroomeaterCondition(Func<int, bool> condition)
	{
		CheckMushrommeaterCondition = condition;
	}

	public void InitializePriestCondition(Func<int, bool> condition)
	{
		CheckPriestCondition = condition;
	}

	public void InitializeGetDay(Func<int> func)
	{
		GetDay = func;
	}

	public void CallFema()
	{
		GenerateCharacterDingDongEventsForDay(GetDay(), ECharacterType.Gasmask);
	}

	public void BeginNewDay()
	{
	}

	public void CompleteEvent(string eventName)
	{
		if (eventName.Contains("entrance"))
		{
			_saveData.DingDongEvents.FirstOrDefault((CharacterDingDongEvent x) => x.Condition?.ConditionName == eventName)?.Complete();
			AnyEventCompleted?.Invoke();
		}
		CompletedEvents.Add(eventName);
	}

	public bool AreAllDingDongsCompletedThisNight(int day)
	{
		return (from x in _saveData.DingDongEvents
			where x.Condition.Day.Contains(day)
			select x.Condition.ConditionName).All((string x) => CompletedEvents.Contains(x));
	}

	public void CheckOtherEvents()
	{
		(int, ETimeOfDay, float) tuple = GetBaseDayNightData();
		foreach (GrowingBellyEvent otherEvent in _saveData.OtherEvents)
		{
			if (otherEvent.CheckConditions(tuple.Item1, tuple.Item2, tuple.Item3, CompletedEvents))
			{
				CompleteEvent(otherEvent.Condition.ConditionName);
				SpecifiedEventCompleted?.Invoke(otherEvent.Condition.ConditionName);
			}
		}
	}

	private void CheckDingDongEvents()
	{
		(int, ETimeOfDay, float) tuple = GetBaseDayNightData();
		if (tuple.Item2 == ETimeOfDay.Day)
		{
			return;
		}
		foreach (CharacterDingDongEvent dingDongEvent in _saveData.DingDongEvents)
		{
			if (dingDongEvent.CheckConditions(tuple.Item1, tuple.Item2, tuple.Item3 - 1f, CompletedEvents, isFake: true))
			{
				CharacterSoonCame?.Invoke();
			}
			if (dingDongEvent.CheckConditions(tuple.Item1, tuple.Item2, tuple.Item3, CompletedEvents))
			{
				CharacterCame?.Invoke(dingDongEvent);
			}
		}
	}

	public void OnUpdateAction()
	{
		if (!(Time.time < _lastTimeUpdated + _updateInterval))
		{
			_lastTimeUpdated = Time.time;
			CheckDingDongEvents();
		}
	}

	public void GenerateBabyEvents(int day)
	{
		List<GrowingBellyEvent> list = new List<GrowingBellyEvent>();
		for (int i = 0; i < 4; i++)
		{
			list.Add(new GrowingBellyEvent(new GameEventConditions(string.Format("{0}.{1}", "belly", i), new int[1] { day + 1 + i }, ETimeOfDay.Day, 0f, Array.Empty<string>(), canRepeat: false)));
		}
		_saveData.OtherEvents.AddRange(list);
	}

	private async UniTaskVoid GenerateDingDongEventsForPresetAsync()
	{
		await UniTask.WaitUntil(() => _gameplayRandomizer.CompletedGameTimesCountAtBegin >= 0);
		GenerateDingDongEventsForPreset();
	}

	private void GenerateDingDongEventsForPreset()
	{
		Debug.Log($"GENERATE PRESET : {_gameplayRandomizer.CompletedGameTimesCountAtBegin}");
		Dictionary<int, List<(CharacterDingDongEvent, int)>> dictionary = new Dictionary<int, List<(CharacterDingDongEvent, int)>>();
		CharacterSOData[] charactersData = _charactersSODataProvider.CharactersData;
		foreach (CharacterSOData characterSOData in charactersData)
		{
			if (!characterSOData.IsPreset || (characterSOData.MinimumGamesCompletedToAppear > 0 && _gameplayRandomizer.CompletedGameTimesCountAtBegin < characterSOData.MinimumGamesCompletedToAppear))
			{
				continue;
			}
			foreach (CharacterVisitData visit in characterSOData.Visits)
			{
				if (!dictionary.ContainsKey(visit.DayNumber))
				{
					dictionary.Add(visit.DayNumber, new List<(CharacterDingDongEvent, int)>());
				}
				dictionary[visit.DayNumber].Add((new CharacterDingDongEvent(characterSOData.CharacterType, new GameEventConditions(string.Format("{0}.{1}.{2}", "entrance", characterSOData.CharacterType, visit.DayNumber), new int[1] { visit.DayNumber }, ETimeOfDay.Night, GetDingDongDelay(visit.DayNumber, visit.GuestNumber), Array.Empty<string>(), canRepeat: false)), visit.GuestNumber));
			}
		}
		_preDingDongEvents = dictionary;
		_arePresetReady = true;
	}

	public void DisableSuperForDay(int day)
	{
		_saveData.DayWithoutSuper = day;
	}

	public async UniTaskVoid GenerateDingDongEventsForDayAsync(int day)
	{
		await UniTask.WaitUntil(() => _arePresetReady);
		if (!_preDingDongEvents.ContainsKey(day))
		{
			_preDingDongEvents.Add(day, new List<(CharacterDingDongEvent, int)>());
		}
		if (!CheckProphetToRemove(day))
		{
			(CharacterDingDongEvent, int) item = _preDingDongEvents[day].FirstOrDefault(((CharacterDingDongEvent dingDong, int order) x) => x.dingDong.Character == ECharacterType.Prophet);
			_preDingDongEvents[day].Remove(item);
		}
		if (_saveData.DayWithoutSuper == day)
		{
			(CharacterDingDongEvent, int) item2 = _preDingDongEvents[day].FirstOrDefault(((CharacterDingDongEvent dingDong, int order) x) => x.dingDong.Character == ECharacterType.SuperFake);
			_preDingDongEvents[day].Remove(item2);
		}
		if (!CheckMushroomeaterToRemove(day))
		{
			(CharacterDingDongEvent, int) item3 = _preDingDongEvents[day].FirstOrDefault(((CharacterDingDongEvent dingDong, int order) x) => x.dingDong.Character == ECharacterType.MushroomEater);
			_preDingDongEvents[day].Remove(item3);
		}
		if (!CheckPriestToRemove(day))
		{
			(CharacterDingDongEvent, int) item4 = _preDingDongEvents[day].FirstOrDefault(((CharacterDingDongEvent dingDong, int order) x) => x.dingDong.Character == ECharacterType.CultistPriest);
			_preDingDongEvents[day].Remove(item4);
		}
		(CharacterSOData, int)[] randomCharactersForDay = _gameplayRandomizer.GetRandomCharactersForDay(day, GetCharactersAndImpostersCount?.Invoke() ?? (-1, -1));
		for (int num = 0; num < randomCharactersForDay.Length; num++)
		{
			(CharacterSOData, int) tuple = randomCharactersForDay[num];
			_preDingDongEvents[day].Add((new CharacterDingDongEvent(tuple.Item1.CharacterType, new GameEventConditions(string.Format("{0}.{1}.{2}", "entrance", tuple.Item1.CharacterType, day), new int[1] { day }, ETimeOfDay.Night, GetDingDongDelay(day, tuple.Item2), Array.Empty<string>(), canRepeat: false)), tuple.Item2));
		}
		GenerateDingDongEventsOrder(day);
	}

	public void GenerateDingDongEventsForSkip()
	{
		(CharacterDingDongEvent, int) item = _preDingDongEvents[1].FirstOrDefault(((CharacterDingDongEvent dingDong, int order) x) => x.dingDong.Character == ECharacterType.Daughter);
		_preDingDongEvents[1].Remove(item);
		GenerateDingDongEventsForDayAsync(1).Forget();
	}

	private bool CheckProphetToRemove(int day)
	{
		if (day == 17)
		{
			return true;
		}
		if (!_preDingDongEvents[day].Select(((CharacterDingDongEvent dingDong, int order) x) => x.dingDong.Character).Contains(ECharacterType.Prophet))
		{
			return true;
		}
		return CheckProphetCondition(_saveData.ProphetVisitsCount++);
	}

	private bool CheckMushroomeaterToRemove(int day)
	{
		if (!_preDingDongEvents[day].Select(((CharacterDingDongEvent dingDong, int order) x) => x.dingDong.Character).Contains(ECharacterType.MushroomEater))
		{
			return true;
		}
		return CheckMushrommeaterCondition(_saveData.MushromeaterVisitsCount++);
	}

	private bool CheckPriestToRemove(int day)
	{
		if (!_preDingDongEvents[day].Select(((CharacterDingDongEvent dingDong, int order) x) => x.dingDong.Character).Contains(ECharacterType.CultistPriest))
		{
			return true;
		}
		return CheckPriestCondition(_saveData.PriestVisitCount++);
	}

	public void GenerateCharacterDingDongEventsForDay(int day, ECharacterType characterType)
	{
		if (_preDingDongEvents == null)
		{
			BetterUniTask.InvokeAfter(() =>
			{
				GenerateCharacterDingDongEventsForDay(day, characterType);
			}, () => _preDingDongEvents != null).Forget();
			return;
		}
		CharacterSOData characterSOData = _charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => x.CharacterType == characterType);
		if (!(characterSOData == null))
		{
			int firstEmptyVisitSlot = _gameplayRandomizer.GetFirstEmptyVisitSlot(day);
			if (!_preDingDongEvents.ContainsKey(day))
			{
				_preDingDongEvents.Add(day, new List<(CharacterDingDongEvent, int)>());
			}
			_preDingDongEvents[day].Add((new CharacterDingDongEvent(characterSOData.CharacterType, new GameEventConditions(string.Format("{0}.{1}.{2}", "entrance", characterSOData.CharacterType, day), new int[1] { day }, ETimeOfDay.Night, GetDingDongDelay(day, firstEmptyVisitSlot), Array.Empty<string>(), canRepeat: false)), firstEmptyVisitSlot));
		}
	}

	private void GenerateDingDongEventsOrder(int day)
	{
		List<(CharacterDingDongEvent, int)> list = _preDingDongEvents.FirstOrDefault((KeyValuePair<int, List<(CharacterDingDongEvent dingDong, int order)>> x) => x.Key == day).Value.OrderBy(((CharacterDingDongEvent dingDong, int order) x) => x.order).ToList();
		List<string> list2 = new List<string>();
		for (int num = 0; num < list.Count; num++)
		{
			for (int num2 = 0; num2 < list2.Count; num2++)
			{
				List<string> necessaryEvents = new List<string>(list2);
				list[num].Item1.SetNecessaryEvents(necessaryEvents);
			}
			list2.Add(list[num].Item1.Condition.ConditionName);
		}
		foreach (var item in list)
		{
			_saveData.DingDongEvents.Add(item.Item1);
		}
	}

	private float GetDingDongDelay(int dayNmber, int guestNumber)
	{
		if (dayNmber == 0 && guestNumber == 1)
		{
			return 1f;
		}
		if (guestNumber == 1)
		{
			return UnityEngine.Random.Range(_data.MinDelayBeforeDingDongForFirstGuest, _data.MaxDelayBeforeDingDongForFirstGuest);
		}
		return UnityEngine.Random.Range(_data.MinDelayBeforeDingDongForGuest, _data.MaxDelayBeforeDingDongForGuest);
	}

	protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
	{
		_saveData = saver.LoadKey(this);
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}

	private void OnSave(bool isReserve)
	{
		foreach (CharacterDingDongEvent dingDongEvent in _saveData.DingDongEvents)
		{
			if (dingDongEvent.IsTriggered && !dingDongEvent.IsCompleted)
			{
				dingDongEvent.Reset();
			}
		}
	}
}
