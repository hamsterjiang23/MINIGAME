using System;
using UnityEngine;

namespace _Code.Infrastructure.StateObjects
{
	[Serializable]
	public sealed class StateObjet
	{
		[SerializeField]
		private GameObject[] _states;

		[field: SerializeField]
		public EStateObjectType StateObjectType { get; private set; }

		public int StatesCount => 0;

		public void SetActiveState(int stateIndex)
		{
		}
	}
}
