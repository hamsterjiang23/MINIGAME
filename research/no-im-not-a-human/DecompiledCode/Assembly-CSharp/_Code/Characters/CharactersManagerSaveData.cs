using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Characters;

[Serializable]
public sealed class CharactersManagerSaveData : ASavableData
{
	[field: SerializeField]
	public List<ECharacterType> CharactersInside { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public List<ECharacterType> CharacterWithPovistka { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public List<ECharacterType> KilledCharacters { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public List<ECharacterType> ExiledCharacters { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public List<ECharacterType> RefusedCharacters { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public List<ECharacterType> SelfKillNextDayCharacters { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public List<ECharacterType> InnocentsKilledByPlayer { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public List<ECharacterType> KilledByImposters { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public List<ECharacterType> KilledTodayCharacters { get; set; } = new List<ECharacterType>();

	[field: SerializeField]
	public Dictionary<ECharacterType, float> FEMARating { get; set; } = new Dictionary<ECharacterType, float>();

	[field: SerializeField]
	public Dictionary<ECharacterType, int> CharactersCameInDays { get; set; } = new Dictionary<ECharacterType, int>();

	[field: SerializeField]
	public Dictionary<ECharacterType, bool> AreCharactersImposters { get; set; } = new Dictionary<ECharacterType, bool>();

	[field: SerializeField]
	public Dictionary<int, List<ECharacterType>> ExilesOnDays { get; } = new Dictionary<int, List<ECharacterType>>();
}
