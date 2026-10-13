using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using _Code.Characters;
using _Code.Events;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Rooms;

namespace _Code.Infrastructure._NINAH__DayNight;

public sealed class MockDayNightController : IDayNightController
{
	public int MaxDayActions { get; }

	public int DayActions { get; }

	public int Day { get; }

	public ETimeOfDay CurrentTimeOfDay { get; }

	public float LastChange { get; }

	public bool CanLeaveRooms { get; }

	public bool IsEndingDay { get; }

	public event Action WatchedTV;

	public event Action<ETimeOfDay> Changed;

	public event Action<int> DayChanged;

	public event Action<List<ChangePoseData>> PosesChanged;

	public event Action BodyEaterAppeared;

	public event Action BodyEaterDisappeared;

	public event Action WentToBed;

	public event Action WokeUp;

	public void Act()
	{
		throw new NotImplementedException();
	}

	public void UpdateTimeOfDayStartedOrDialogEndedTime()
	{
		throw new NotImplementedException();
	}

	public bool HasCompletedDaytimeGoal()
	{
		throw new NotImplementedException();
	}

	public void ActAll()
	{
		throw new NotImplementedException();
	}

	public void Change()
	{
		throw new NotImplementedException();
	}

	public UniTaskVoid SetDayForEnding(EEnding ending)
	{
		throw new NotImplementedException();
	}

	public void AddEnergySlot()
	{
		throw new NotImplementedException();
	}

	public void AddEnergy()
	{
		throw new NotImplementedException();
	}

	public void RefillEnergy()
	{
		throw new NotImplementedException();
	}

	public void RemoveExtraEnergySlotsForTomorrow(int energyCount)
	{
		throw new NotImplementedException();
	}

	public void AddChangePosTomorrow(ECharacterType character, ERoomPeopleState pose)
	{
		throw new NotImplementedException();
	}
}
