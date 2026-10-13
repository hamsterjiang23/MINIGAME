using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure.Consumables;

[Serializable]
public sealed class ConsumablesSaveData : ASavableData
{
	[field: SerializeField]
	public Dictionary<EConsumable, int> Storage { get; set; } = new Dictionary<EConsumable, int>();

	[field: SerializeField]
	public int DeficitItemDay { get; set; }

	[field: SerializeField]
	public int PovistkasUsed { get; set; }

	[field: SerializeField]
	public List<EConsumable> ConsumablesTodayUsed { get; private set; } = new List<EConsumable>();

	[field: SerializeField]
	public List<EConsumable> ConsumablesEverUsed { get; } = new List<EConsumable>();
}
