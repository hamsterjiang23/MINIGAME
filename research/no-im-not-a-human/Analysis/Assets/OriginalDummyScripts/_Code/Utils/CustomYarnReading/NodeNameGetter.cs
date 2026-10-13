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
		}

		public string GetNodeName()
		{
			return null;
		}

		private void IncrementNode()
		{
		}

		public void ResetDay()
		{
		}
	}
}
