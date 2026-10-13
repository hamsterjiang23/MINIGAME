using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SABI;
using _Code.Characters;

namespace _Scripts.Services.DataModel.Models.Dialogs;

[Serializable]
public sealed class CharactersTalksCountData
{
	[JsonProperty]
	private Dictionary<ECharacterType, int> _charactersTalksCount = new Dictionary<ECharacterType, int>();

	[JsonProperty]
	private Dictionary<ECharacterType, int> _charactersMaxTalksCount = new Dictionary<ECharacterType, int>();

	public void AddTalk(ECharacterType characterType)
	{
		if (!_charactersTalksCount.TryAdd(characterType, 1))
		{
			_charactersTalksCount[characterType]++;
		}
	}

	public int GetTalksCount(ECharacterType characterType)
	{
		int value = (_charactersTalksCount.TryGetValue(characterType, out var value2) ? value2 : 0);
		int max = (_charactersMaxTalksCount.TryGetValue(characterType, out var value3) ? value3 : 0);
		return value.Clamp(0, max);
	}

	public void SetToLast(ECharacterType characterType)
	{
		int value = (_charactersMaxTalksCount.TryGetValue(characterType, out var value2) ? value2 : 0);
		if (!_charactersTalksCount.TryAdd(characterType, value))
		{
			_charactersTalksCount[characterType] = value;
		}
	}

	public void SetMaxTalks(ECharacterType characterType, int count)
	{
		if (!_charactersMaxTalksCount.TryAdd(characterType, count - 1))
		{
			_charactersMaxTalksCount[characterType] = count - 1;
		}
	}
}
