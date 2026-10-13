using System;
using UnityEngine;

namespace _Code.Menues.Titles
{
	[Serializable]
	public sealed class TitlesFadeData
	{
		[field: SerializeField]
		public float Time { get; set; }

		[field: SerializeField]
		public bool IsFadeIn { get; set; }

		[field: SerializeField]
		public float Duration { get; set; }
	}
}
