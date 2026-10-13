using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using _Code.Characters;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Scripts.Services.DataModel.Models.Dialogs;

namespace _Code.DialogSystem
{
	[Serializable]
	public sealed class DialogSaveData : ASavableData
	{
		[field: SerializeField]
		public Dictionary<string, bool> YarnBoolVariables { get; set; }

		[field: SerializeField]
		public Dictionary<string, float> YarnFloatVariables { get; set; }

		[field: SerializeField]
		public Dictionary<string, string> YarnStringVariables { get; set; }

		[JsonProperty]
		[field: SerializeField]
		private List<ECharacterType> CharacterWithWhomTalkedToday { get; set; }

		[JsonProperty]
		[field: SerializeField]
		private CharactersTalksCountData CharactersTalksCount { get; set; }

		[JsonProperty]
		[field: SerializeField]
		public DialogCourierOrderData CourierOrderData { get; set; }

		[field: SerializeField]
		public int DidntCheckSignsDays { get; set; }

		[field: SerializeField]
		public bool HasCheckedSignsToday { get; set; }

		[field: SerializeField]
		public bool EverRudeToFema { get; set; }

		public void AddCharacterWithWhoTalkedToday(ECharacterType characterType)
		{
		}

		public void SetToLastTalk(ECharacterType character)
		{
		}

		public void AddMaxTalksCount(ECharacterType character, int count)
		{
		}

		public void ApplyCharacterTalks()
		{
		}

		public int GetTalksCount(ECharacterType characterType)
		{
			return 0;
		}

		public void ApplyOneTalkFor(CharacterSOData character)
		{
		}

		public bool HasAlreadyTalkedToday(ECharacterType characterType)
		{
			return false;
		}
	}
}
