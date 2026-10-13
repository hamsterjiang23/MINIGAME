using System;
using UnityEngine;
using _Code.Characters;

namespace _Code.Utils
{
	[Serializable]
	public sealed class CharacterDeathlistElement
	{
		[field: SerializeField]
		public int Priority { get; private set; }

		[field: SerializeField]
		public int FirstNight { get; private set; }

		[field: SerializeField]
		public CharacterSOData Character { get; private set; }

		public CharacterDeathlistElement(int priority, int firstNight, CharacterSOData character)
		{
			Priority = priority;
			FirstNight = firstNight;
			Character = character;
		}
	}
}
