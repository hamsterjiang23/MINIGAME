using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using _Code.Characters;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Scripts.Services.DataModel.Models.Dialogs;

namespace _Code.DialogSystem;

[Serializable]
public sealed class DialogSaveData : ASavableData
{
	[field: SerializeField]
	public Dictionary<string, bool> YarnBoolVariables { get; set; } = new Dictionary<string, bool>();

	[field: SerializeField]
	public Dictionary<string, float> YarnFloatVariables { get; set; } = new Dictionary<string, float>();

	[field: SerializeField]
	public Dictionary<string, string> YarnStringVariables { get; set; } = new Dictionary<string, string>();

	[JsonProperty]
	[field: SerializeField]
	private List<ECharacterType> CharacterWithWhomTalkedToday { get; set; } = new List<ECharacterType>();

	[JsonProperty]
	[field: SerializeField]
	private CharactersTalksCountData CharactersTalksCount { get; set; } = new CharactersTalksCountData();

	[JsonProperty]
	[field: SerializeField]
	public DialogCourierOrderData CourierOrderData { get; set; } = new DialogCourierOrderData();

	[field: SerializeField]
	public int DidntCheckSignsDays { get; set; }

	[field: SerializeField]
	public bool HasCheckedSignsToday { get; set; }

	[field: SerializeField]
	public bool EverRudeToFema { get; set; }

	public void AddCharacterWithWhoTalkedToday(ECharacterType characterType)
	{
		CharacterWithWhomTalkedToday.Add(characterType);
	}

	public void SetToLastTalk(ECharacterType character)
	{
		CharactersTalksCount.SetToLast(character);
	}

	public void AddMaxTalksCount(ECharacterType character, int count)
	{
		CharactersTalksCount.SetMaxTalks(character, count);
	}

	public void ApplyCharacterTalks()
	{
		foreach (ECharacterType item in CharacterWithWhomTalkedToday.Distinct().ToList())
		{
			CharactersTalksCount.AddTalk(item);
		}
		CharacterWithWhomTalkedToday.Clear();
	}

	public int GetTalksCount(ECharacterType characterType)
	{
		return CharactersTalksCount.GetTalksCount(characterType);
	}

	public void ApplyOneTalkFor(CharacterSOData character)
	{
		CharactersTalksCount.AddTalk(character.CharacterType);
	}

	public bool HasAlreadyTalkedToday(ECharacterType characterType)
	{
		return CharacterWithWhomTalkedToday.Contains(characterType);
	}
}
