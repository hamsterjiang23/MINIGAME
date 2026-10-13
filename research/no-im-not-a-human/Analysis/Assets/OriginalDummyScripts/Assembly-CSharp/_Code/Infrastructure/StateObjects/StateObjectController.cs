using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using _Code.Characters;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.StateObjects;

public sealed class StateObjectController : ASavableClass<StateObjectsSaveData>, IStateObjectController, IInitializable, IDisposable
{
	private StateObjectsSaveData _saveData = new StateObjectsSaveData();

	private readonly StateObjet[] _stateObjets;

	private readonly IDataModelService _dataModelService;

	public event Action<EStateObjectType> ReachedLastState;

	public event Action<EStateObjectType, int> StateChanged;

	public StateObjectController(IStateObjectViewProvider viewProvider, IDataModelService dataModelService, ICharactersManager charactersManager)
	{
		_stateObjets = viewProvider.StateObjets;
		_dataModelService = dataModelService;
		StateObjet[] stateObjets = _stateObjets;
		foreach (StateObjet stateObjet in stateObjets)
		{
			if (!_saveData.ObjectsStates.ContainsKey(stateObjet.StateObjectType))
			{
				_saveData.ObjectsStates.Add(stateObjet.StateObjectType, 0);
			}
		}
		charactersManager.CharacterRemoved -= OnCharacterRemoved;
		charactersManager.CharacterRemoved += OnCharacterRemoved;
		charactersManager.CharacterAdded -= OnCharacterAdded;
		charactersManager.CharacterAdded += OnCharacterAdded;
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}

	private void OnCharacterAdded(ECharacterType character, bool isFromSave)
	{
		if (character == ECharacterType.Widow)
		{
			SetState(EStateObjectType.Husband, 1);
		}
	}

	private void OnCharacterRemoved(ECharacterType character)
	{
		if (character == ECharacterType.Widow)
		{
			SetState(EStateObjectType.Husband, 0);
		}
	}

	public void SetState(EStateObjectType stateObjectType, int index)
	{
		Debug.Log($"SetState {stateObjectType} to {index}");
		StateObjet stateObjet = _stateObjets.FirstOrDefault((StateObjet x) => x.StateObjectType == stateObjectType);
		_saveData.ObjectsStates[stateObjectType] = index;
		StateChanged?.Invoke(stateObjectType, index);
		stateObjet?.SetActiveState(index);
		if (index == ((stateObjet != null) ? new int?(stateObjet.StatesCount - 1) : ((int?)null)))
		{
			ReachedLastState?.Invoke(stateObjet.StateObjectType);
		}
	}

	public void SetStateDelayed(EStateObjectType state, int index, int day)
	{
		if (_saveData.DelayedChanges.ContainsKey(day))
		{
			_saveData.DelayedChanges[day] = (state, index);
		}
		else
		{
			_saveData.DelayedChanges.Add(day, (state, index));
		}
	}

	public void IncrementState(EStateObjectType stateObjectType)
	{
		StateObjet stateObjet = _stateObjets.FirstOrDefault((StateObjet x) => x.StateObjectType == stateObjectType);
		int num = _saveData.ObjectsStates[stateObjectType];
		num++;
		if (!(num >= stateObjet?.StatesCount))
		{
			_saveData.ObjectsStates[stateObjectType] = num;
			StateChanged?.Invoke(stateObjectType, num);
			stateObjet?.SetActiveState(num);
			if (num == ((stateObjet != null) ? new int?(stateObjet.StatesCount - 1) : ((int?)null)))
			{
				ReachedLastState?.Invoke(stateObjet.StateObjectType);
			}
		}
	}

	public int GetState(EStateObjectType ground)
	{
		return _saveData.ObjectsStates[ground];
	}

	public void CheckChangesForDay(int day)
	{
		List<int> list = new List<int>();
		foreach (KeyValuePair<int, (EStateObjectType, int)> delayedChange in _saveData.DelayedChanges)
		{
			if (delayedChange.Key == day)
			{
				SetState(delayedChange.Value.Item1, delayedChange.Value.Item2);
				list.Add(delayedChange.Key);
			}
		}
		foreach (int item in list)
		{
			_saveData.DelayedChanges.Remove(item);
		}
	}

	protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
	{
		_saveData = saver.LoadKey(this);
		foreach (KeyValuePair<EStateObjectType, int> item in new Dictionary<EStateObjectType, int>(_saveData.ObjectsStates))
		{
			SetState(item.Key, item.Value);
		}
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}

	public void Initialize()
	{
	}
}
