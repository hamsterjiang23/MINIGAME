using System;
using UnityEngine;

namespace _Code.Rooms
{
	[Serializable]
	public sealed class WindowDayImageData
	{
		[field: SerializeField]
		public WindowImageData[] WindowImageData { get; private set; }
	}
}
