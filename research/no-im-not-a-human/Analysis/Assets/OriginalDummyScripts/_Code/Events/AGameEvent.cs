using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace _Code.Events
{
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
			return false;
		}

		public void Reset()
		{
		}

		public void Complete()
		{
		}
	}
}
