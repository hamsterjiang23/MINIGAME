using System;
using RoboRyanTron.SearchableEnum;
using UnityEngine;

namespace _Code.Infrastructure.Sound
{
	[Serializable]
	public sealed class MusicDayData
	{
		[field: SerializeField]
		[field: SearchableEnum]
		public ESound DayMusic { get; private set; }

		[field: SerializeField]
		[field: SearchableEnum]
		public ESound NightMusic { get; private set; }

		[field: SerializeField]
		[field: SearchableEnum]
		public ESound Noise { get; private set; }

		[field: SerializeField]
		[field: Range(0f, 1f)]
		public float NoiseVolume { get; private set; }
	}
}
