using _Code.Player;

namespace _Code.Infrastructure.Settings.Control
{
	public sealed class ControlSettings : ISetting
	{
		private ControlSettingsData _settings;

		private InputHandling _inputHandler;

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

		public void SetGamepadVibration(bool value)
		{
		}

		public void SetMouseSensitivity(float value)
		{
		}

		public void SetMouseInvertY(bool value)
		{
		}

		public void SetGamepadSensitivity(float value)
		{
		}

		public void SetGamepadRoomSensitivity(float value)
		{
		}

		public void SetGamepadInvertY(bool value)
		{
		}

		public void InitModules(InputHandling inputHandler)
		{
		}
	}
}
