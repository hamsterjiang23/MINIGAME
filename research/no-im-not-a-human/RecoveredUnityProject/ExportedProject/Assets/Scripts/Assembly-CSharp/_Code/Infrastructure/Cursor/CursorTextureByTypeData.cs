using System;
using UnityEngine;

namespace _Code.Infrastructure.Cursor
{
	[Serializable]
	public sealed class CursorTextureByTypeData
	{
		[field: SerializeField]
		public ECursorType Type { get; private set; }

		[field: SerializeField]
		public Texture2D Sprite { get; private set; }
	}
}
