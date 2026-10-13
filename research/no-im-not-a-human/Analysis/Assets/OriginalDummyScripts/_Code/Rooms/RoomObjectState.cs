using System;
using UnityEngine;

namespace _Code.Rooms
{
	[Serializable]
	public sealed class RoomObjectState<T> where T : Enum
	{
		[field: SerializeField]
		public T Name { get; private set; }

		[field: SerializeField]
		public Sprite Sprite { get; private set; }

		[field: SerializeField]
		public Vector2 PositionShift { get; set; }

		[field: SerializeField]
		public Vector2 ScaleShift { get; set; }

		[field: SerializeField]
		public bool CannotInteract { get; private set; }
	}
}
