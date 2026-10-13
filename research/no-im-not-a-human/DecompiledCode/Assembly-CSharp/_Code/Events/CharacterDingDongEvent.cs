using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Characters;

namespace _Code.Events;

[Serializable]
public sealed class CharacterDingDongEvent : AGameEvent
{
	[field: SerializeField]
	public ECharacterType Character { get; private set; }

	public CharacterDingDongEvent(ECharacterType character, GameEventConditions condition)
	{
		Condition = condition;
		Character = character;
	}

	public void SetNecessaryEvents(IReadOnlyList<string> events)
	{
		Condition.SetNecessaryEvents(events);
	}
}
