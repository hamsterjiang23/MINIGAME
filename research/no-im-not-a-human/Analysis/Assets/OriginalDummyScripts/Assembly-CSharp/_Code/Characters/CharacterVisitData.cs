using System;
using UnityEngine;

namespace _Code.Characters;

[Serializable]
public sealed class CharacterVisitData
{
	[field: SerializeField]
	public int DayNumber { get; private set; }

	[field: SerializeField]
	public int GuestNumber { get; private set; }
}
