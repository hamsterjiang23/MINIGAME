using _Code.Player;
using _Code.Utils;

namespace _Code.Infrastructure.Settings.Control;

public sealed class ControlSettings : ISetting
{
	private ControlSettingsData _settings = new ControlSettingsData();

	private InputHandling _inputHandler;

	public ASettingsData SettingsData
	{
		get
		{
			return _settings;
		}
		set
		{
			_settings = (ControlSettingsData)value;
		}
	}

	public void SetGamepadVibration(bool value)
	{
		_settings.GamepadVibration = value;
		Vibrator.SetEnabledState(value);
	}

	public void SetMouseSensitivity(float value)
	{
		_settings.MouseSensitivity = value;
		_inputHandler.SetMouseSensitivity(value, _settings.InvertMouseY);
	}

	public void SetMouseInvertY(bool value)
	{
		_settings.InvertMouseY = value;
		_inputHandler.SetMouseSensitivity(_settings.MouseSensitivity, value);
	}

	public void SetGamepadSensitivity(float value)
	{
		_settings.GamepadSensitivity = value;
		_inputHandler.SetGamepadSensitivity(value, _settings.InvertGamepadY);
	}

	public void SetGamepadRoomSensitivity(float value)
	{
		_settings.GamepadRoomSensitivity = value;
		_inputHandler.SetGamepadRoomSensitivity(value);
	}

	public void SetGamepadInvertY(bool value)
	{
		_settings.InvertGamepadY = value;
		_inputHandler.SetGamepadSensitivity(_settings.GamepadSensitivity, value);
	}

	public void InitModules(InputHandling inputHandler)
	{
		_inputHandler = inputHandler;
	}
}
