using System;
using UnityEngine;

namespace _Code.Infrastructure.Settings.Screen
{
	[Serializable]
	public sealed class ScreenSettingsData : ASettingsData
	{
		[field: SerializeField]
		public int SelectedResolutionIndex { get; set; }

		[field: SerializeField]
		public FullScreenMode FullScreenMode { get; set; }

		[field: SerializeField]
		public bool IsVSync { get; set; } = true;

		[field: SerializeField]
		public bool IsNotFirstLaunch { get; set; }

		[field: SerializeField]
		public bool HasUpdatedToExclusiveFullscreenForSteamdeckSinceVersion120 { get; set; }
	}
}
