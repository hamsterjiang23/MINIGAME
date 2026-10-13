using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using _Code.Characters;
using _Code.Events;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Rooms;

namespace _Code.Infrastructure.DayNight;

public interface IDayNightController
{
	int MaxDayActions { get; }

	int DayActions { get; }

	int Day { get; }

	ETimeOfDay CurrentTimeOfDay { get; }

	float LastChange { get; }

	bool CanLeaveRooms { get; }

	bool IsEndingDay { get; }

	event Action WatchedTV;

	event Action<ETimeOfDay> Changed;

	event Action<int> DayChanged;

	event Action<List<ChangePoseData>> PosesChanged;

	event Action BodyEaterAppeared;

	event Action BodyEaterDisappeared;

	event Action WentToBed;

	event Action WokeUp;

	void Act();

	void UpdateTimeOfDayStartedOrDialogEndedTime();

	bool HasCompletedDaytimeGoal();

	void ActAll();

	void Change();

	UniTaskVoid SetDayForEnding(EEnding ending);

	void AddEnergySlot();

	void AddEnergy();

	void RefillEnergy();

	void RemoveExtraEnergySlotsForTomorrow(int energyCount);

	void AddChangePosTomorrow(ECharacterType character, ERoomPeopleState pose);
}
