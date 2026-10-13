using System;
using Cysharp.Threading.Tasks;
using _Code.Characters;
using _Code.Events;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.GameEvents;

public interface IGameEventsManager
{
	IUpdateable Updateable { get; }

	event Action AnyEventCompleted;

	event Action<string> SpecifiedEventCompleted;

	event Func<(int day, ETimeOfDay timeOfDay, float timeFromLastChange)> GetBaseDayNightData;

	event Action<CharacterDingDongEvent> CharacterCame;

	event Action CharacterSoonCame;

	event Func<(int total, int imposters)> GetCharactersAndImpostersCount;

	void CompleteEvent(string eventName);

	bool AreAllDingDongsCompletedThisNight(int day);

	void GenerateBabyEvents(int day);

	void CheckOtherEvents();

	UniTaskVoid GenerateDingDongEventsForDayAsync(int day);

	void GenerateDingDongEventsForSkip();

	void GenerateCharacterDingDongEventsForDay(int day, ECharacterType characterType);

	void InitializeProphetCondition(Func<int, bool> condition);

	void InitializeMushroomeaterCondition(Func<int, bool> condition);

	void InitializePriestCondition(Func<int, bool> checkPriestCondition);

	void CallFema();

	void BeginNewDay();

	void InitializeGetDay(Func<int> func);

	void DisableSuperForDay(int day);
}
