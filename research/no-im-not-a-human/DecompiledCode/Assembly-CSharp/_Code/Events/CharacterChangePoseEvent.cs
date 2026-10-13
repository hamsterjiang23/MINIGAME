using System;
using UnityEngine;
using _Code.Characters;
using _Code.Rooms;

namespace _Code.Events;

[Serializable]
public sealed class CharacterChangePoseEvent : AGameEvent
{
	[field: SerializeField]
	public CharacterSOData Character { get; private set; }

	[field: SerializeField]
	public ERoomPeopleState Pose { get; private set; }
}
