using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace _Code.Events;

[Serializable]
public abstract class AGameEvent
{
	[JsonProperty]
	public GameEventConditions Condition { get; protected set; }

	[JsonProperty]
	public bool IsTriggered { get; private set; }

	[JsonProperty]
	public bool IsCompleted { get; private set; }

	public bool CheckConditions(int day, ETimeOfDay timeOfDay, float lastDaytimeChange, List<string> completedEvents, bool isFake = false)
	{
		if (completedEvents.Contains(Condition.ConditionName))
		{
			IsTriggered = true;
			return false;
		}
		if (IsTriggered && !Condition.CanRepeat)
		{
			return false;
		}
		if (IsCompleted)
		{
			return false;
		}
		if (!Condition.Day.Contains(day))
		{
			return false;
		}
		if (Condition.TimeOfDay != timeOfDay)
		{
			return false;
		}
		if (Condition.TimeElapsedFromStartOfTimeOfDay > Time.timeSinceLevelLoad - lastDaytimeChange)
		{
			return false;
		}
		if (!Condition.NecessaryEvents.All(completedEvents.Contains))
		{
			return false;
		}
		if (!isFake)
		{
			IsTriggered = true;
		}
		return true;
	}

	public void Reset()
	{
		IsTriggered = false;
	}

	public void Complete()
	{
		IsCompleted = true;
	}
}
