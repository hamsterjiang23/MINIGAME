using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Utils.CustomYarnReading;

namespace _Code.Infrastructure
{
	[Serializable]
	public sealed class InteractablesSaveData : ASavableData, INodeNameGetterSaveData
	{
		[field: SerializeField]
		public Dictionary<int, int> NodeNamesIndexes { get; set; }

		[field: SerializeField]
		public List<int> TodayDialogInteractables { get; set; }
	}
}
