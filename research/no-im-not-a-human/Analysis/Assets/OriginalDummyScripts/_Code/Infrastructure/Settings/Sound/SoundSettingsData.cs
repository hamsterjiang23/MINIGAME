using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Code.Infrastructure.Settings.Sound
{
	[Serializable]
	public sealed class SoundSettingsData : ASettingsData
	{
		[field: SerializeField]
		public Dictionary<string, float> VolumesByChanel { get; set; }

		[field: SerializeField]
		public bool IsNotFirstLaunch { get; set; }
	}
}
