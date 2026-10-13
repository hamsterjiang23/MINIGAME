using System;
using UnityEngine;
using _Code.Characters;
using _Code.Rooms;

namespace _Code.Infrastructure._NINAH__Rooms
{
	[Serializable]
	public sealed class ChangePoseData
	{
		[field: SerializeField]
		public ECharacterType Character { get; private set; }

		[field: SerializeField]
		public ERoomPeopleState State { get; private set; }

		public ChangePoseData(ECharacterType character, ERoomPeopleState state)
		{
		}
	}
}
