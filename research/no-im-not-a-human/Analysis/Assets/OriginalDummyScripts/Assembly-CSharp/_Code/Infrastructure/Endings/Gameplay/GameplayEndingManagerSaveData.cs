using System;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure.Endings.Gameplay;

[Serializable]
public sealed class GameplayEndingManagerSaveData : ASavableData
{
	[field: SerializeField]
	public bool HasFoundBedroomBaby { get; set; }

	[field: SerializeField]
	public bool HasFoundOfficeBaby { get; set; }

	[field: SerializeField]
	public bool HasFoundBathroomBaby { get; set; }

	[field: SerializeField]
	public bool HasWatchedMushroomClock { get; set; }

	[field: SerializeField]
	public bool HasFoundMushroomApple { get; set; }

	[field: SerializeField]
	public bool HasEatenMushroom { get; set; }

	[field: SerializeField]
	public bool HasBegunCultists { get; set; }

	[field: SerializeField]
	public bool SavedCultists { get; set; }

	[field: SerializeField]
	public bool[] ProphetConditions { get; set; } = new bool[3];

	[field: SerializeField]
	public bool HatchOpened { get; set; }
}
