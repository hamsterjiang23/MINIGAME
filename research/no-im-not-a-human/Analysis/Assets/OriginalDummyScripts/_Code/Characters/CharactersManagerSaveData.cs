using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Characters
{
	[Serializable]
	public sealed class CharactersManagerSaveData : ASavableData
	{
		[field: SerializeField]
		public List<ECharacterType> CharactersInside { get; set; }

		[field: SerializeField]
		public List<ECharacterType> CharacterWithPovistka { get; set; }

		[field: SerializeField]
		public List<ECharacterType> KilledCharacters { get; set; }

		[field: SerializeField]
		public List<ECharacterType> ExiledCharacters { get; set; }

		[field: SerializeField]
		public List<ECharacterType> RefusedCharacters { get; set; }

		[field: SerializeField]
		public List<ECharacterType> SelfKillNextDayCharacters { get; set; }

		[field: SerializeField]
		public List<ECharacterType> InnocentsKilledByPlayer { get; set; }

		[field: SerializeField]
		public List<ECharacterType> KilledByImposters { get; set; }

		[field: SerializeField]
		public List<ECharacterType> KilledTodayCharacters { get; set; }

		[field: SerializeField]
		public Dictionary<ECharacterType, float> FEMARating { get; set; }

		[field: SerializeField]
		public Dictionary<ECharacterType, int> CharactersCameInDays { get; set; }

		[field: SerializeField]
		public Dictionary<ECharacterType, bool> AreCharactersImposters { get; set; }

		[field: SerializeField]
		public Dictionary<int, List<ECharacterType>> ExilesOnDays { get; }
	}
}
