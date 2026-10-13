using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure.StateObjects
{
	[Serializable]
	public sealed class StateObjectsSaveData : ASavableData
	{
		[field: SerializeField]
		public Dictionary<EStateObjectType, int> ObjectsStates { get; private set; }

		[field: SerializeField]
		public Dictionary<int, (EStateObjectType state, int index)> DelayedChanges { get; private set; }
	}
}
