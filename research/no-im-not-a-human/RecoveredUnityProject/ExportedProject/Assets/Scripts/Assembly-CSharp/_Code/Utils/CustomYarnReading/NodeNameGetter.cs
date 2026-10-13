using System;
using UnityEngine;

namespace _Code.Utils.CustomYarnReading
{
	[Serializable]
	public sealed class NodeNameGetter
	{
		[SerializeField]
		private string[] _nodeNames;

		[SerializeField]
		private ENodeGetType _getType;

		[SerializeField]
		private ENodeNextType _nextType;

		private INodeNameGetterSaveData _saveData;

		private int _index;

		private int _currentNodeIndex;

		public void Init(int getterIndex, INodeNameGetterSaveData saveData)
		{
			_saveData = saveData;
			_index = getterIndex;
			if (saveData.NodeNamesIndexes.TryGetValue(_index, out var value))
			{
				_currentNodeIndex = value;
			}
		}

		public string GetNodeName()
		{
			string result = _nodeNames[_currentNodeIndex];
			if (_nextType == ENodeNextType.AfterTalk)
			{
				IncrementNode();
			}
			else
			{
				_saveData.TodayDialogInteractables.Add(_index);
			}
			if (_saveData.NodeNamesIndexes.ContainsKey(_index))
			{
				_saveData.NodeNamesIndexes[_index] = _currentNodeIndex;
				return result;
			}
			_saveData.NodeNamesIndexes.Add(_index, _currentNodeIndex);
			return result;
		}

		private void IncrementNode()
		{
			_currentNodeIndex++;
			if (_currentNodeIndex >= _nodeNames.Length)
			{
				switch (_getType)
				{
				case ENodeGetType.ByOrder_RepeatAll:
					_currentNodeIndex = 0;
					break;
				case ENodeGetType.ByOrder_RepeatLast:
					_currentNodeIndex = _nodeNames.Length - 1;
					break;
				}
			}
		}

		public void ResetDay()
		{
			if (_saveData.TodayDialogInteractables.Contains(_index))
			{
				IncrementNode();
				_saveData.TodayDialogInteractables.Remove(_index);
			}
		}
	}
}
