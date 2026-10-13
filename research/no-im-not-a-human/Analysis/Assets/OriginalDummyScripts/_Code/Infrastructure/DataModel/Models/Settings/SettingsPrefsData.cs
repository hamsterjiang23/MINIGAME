using System;
using UnityEngine;
using _Code.Infrastructure.Settings.Control;
using _Code.Infrastructure.Settings.Language;
using _Code.Infrastructure.Settings.Screen;
using _Code.Infrastructure.Settings.Sound;
using _Scripts.Services.DataModel.Models;

namespace _Code.Infrastructure.DataModel.Models.Settings
{
	[Serializable]
	public sealed class SettingsPrefsData : BaseDataStorage
	{
		[field: SerializeField]
		public ScreenSettingsData ScreenSettingsData { get; set; }

		[field: SerializeField]
		public TextSettingsData TextSettingsData { get; set; }

		[field: SerializeField]
		public SoundSettingsData SoundSettingsData { get; set; }

		[field: SerializeField]
		public ControlSettingsData ControlSettingsData { get; set; }
	}
}
