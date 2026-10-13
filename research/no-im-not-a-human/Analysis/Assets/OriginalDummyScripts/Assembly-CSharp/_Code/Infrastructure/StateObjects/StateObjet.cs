using System;
using UnityEngine;

namespace _Code.Infrastructure.StateObjects;

[Serializable]
public sealed class StateObjet
{
	[SerializeField]
	private GameObject[] _states;

	[field: SerializeField]
	public EStateObjectType StateObjectType { get; private set; }

	public int StatesCount => _states.Length;

	public void SetActiveState(int stateIndex)
	{
		for (int i = 0; i < _states.Length; i++)
		{
			if (!(_states[i] == null))
			{
				_states[i].SetActive(i == stateIndex);
			}
		}
	}
}
