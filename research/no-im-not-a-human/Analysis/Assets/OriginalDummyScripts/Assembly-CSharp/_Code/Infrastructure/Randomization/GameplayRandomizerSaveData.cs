using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Characters;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure.Randomization;

[Serializable]
public sealed class GameplayRandomizerSaveData : ASavableData
{
	[field: SerializeField]
	public int Seed { get; set; }

	[field: SerializeField]
	public List<ECharacterType> UsedCharacters { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public Dictionary<int, List<int>> ExtraCharactersVisitOrders { get; set; } = new Dictionary<int, List<int>>();

	[field: SerializeField]
	public int CompletedGameTimesCountAtBegin { get; set; } = -1;
}
