using UnityEngine;

namespace _Code.Infrastructure.Settings.Screen
{
	public sealed class ScreenSettings : ISetting
	{
		private ScreenSettingsData _settings = new ScreenSettingsData();

		public ASettingsData SettingsData
		{
			get
			{
				return _settings;
			}
			set
			{
				_settings = (ScreenSettingsData)value;
			}
		}

		public void SetResolution(int index)
		{
			_settings.SelectedResolutionIndex = index;
			ApplyScreenSettings();
		}

		public void SetFullScreenMode(FullScreenMode fullScreenMode)
		{
			_settings.FullScreenMode = fullScreenMode;
			ApplyScreenSettings();
		}

		public void SetVSync(bool isOn)
		{
			_settings.IsVSync = isOn;
			QualitySettings.vSyncCount = (isOn ? 1 : 0);
		}

		private void ApplyScreenSettings()
		{
			Resolution[] resolutions = UnityEngine.Screen.resolutions;
			Resolution resolution = resolutions[resolutions.Length - (_settings.SelectedResolutionIndex + 1)];
			UnityEngine.Screen.SetResolution(resolution.width, resolution.height, _settings.FullScreenMode, resolution.refreshRateRatio);
		}
	}
}
