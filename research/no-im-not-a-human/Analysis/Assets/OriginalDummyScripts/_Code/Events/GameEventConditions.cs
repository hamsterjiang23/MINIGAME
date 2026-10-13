using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Code.Events
{
	[Serializable]
	public sealed class GameEventConditions
	{
		[field: SerializeField]
		public string ConditionName { get; private set; }

		[field: SerializeField]
		public int[] Day { get; private set; }

		[field: SerializeField]
		public ETimeOfDay TimeOfDay { get; private set; }

		[field: SerializeField]
		public float TimeElapsedFromStartOfTimeOfDay { get; private set; }

		[field: SerializeField]
		public IReadOnlyList<string> NecessaryEvents { get; private set; }

		[field: SerializeField]
		public bool CanRepeat { get; private set; }

		public GameEventConditions(string conditionName, int[] day, ETimeOfDay timeOfDay, float timeElapsedFromStartOfTimeOfDay, string[] necessaryEvents, bool canRepeat)
		{
		}

		public void SetNecessaryEvents(IReadOnlyList<string> events)
		{
		}
	}
}
