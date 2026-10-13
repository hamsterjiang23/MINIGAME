using UnityEngine;

namespace _Code.Infrastructure.Settings.Screen
{
	public sealed class ScreenSettings : ISetting
	{
		private ScreenSettingsData _settings;

		public ASettingsData SettingsData
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public void SetResolution(int index)
		{
		}

		public void SetFullScreenMode(FullScreenMode fullScreenMode)
		{
		}

		public void SetVSync(bool isOn)
		{
		}

		private void ApplyScreenSettings()
		{
		}
	}
}
