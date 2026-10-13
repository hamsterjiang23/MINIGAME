using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.Settings.Sound;
using _Code.Player;

namespace _Code.Infrastructure.Settings.Control
{
	public sealed class ControlSettingsInstance : ASettingsInstance
	{
		[SerializeField]
		private Toggle _gamepadVibrationToggle;

		[SerializeField]
		private FakeSlider _gamepadSensitivity;

		[SerializeField]
		private FakeSlider _gamepadRoomSensitivity;

		[SerializeField]
		private FakeSlider _mouseSensitivity;

		[SerializeField]
		private Toggle _invertMouseY;

		[SerializeField]
		private Toggle _invertGamepadY;

		private readonly ControlSettings _controlSettings = new ControlSettings();

		private InputHandling _inputHandler;

		private bool _isSceneStarted;

		public override ISetting Setting => _controlSettings;

		protected override void Init()
		{
			_gamepadVibrationToggle.onValueChanged.RemoveAllListeners();
			_gamepadVibrationToggle.onValueChanged.AddListener(OnGamepadVibrationChanged);
			_invertMouseY.onValueChanged.RemoveAllListeners();
			_invertMouseY.onValueChanged.AddListener(OnMouseInvertYChanged);
			_invertGamepadY.onValueChanged.RemoveAllListeners();
			_invertGamepadY.onValueChanged.AddListener(OnGamepadInvertYChanged);
			_gamepadSensitivity.Init(OnGamepadSensitivityChanged);
			_gamepadRoomSensitivity.Init(OnGamepadRoomSensitivityChanged);
			_mouseSensitivity.Init(OnMouseSensitivityChanged);
		}

		private void OnGamepadInvertYChanged(bool isEnabled)
		{
			_controlSettings.SetGamepadInvertY(isEnabled);
		}

		private void OnMouseInvertYChanged(bool isEnabled)
		{
			_controlSettings.SetMouseInvertY(isEnabled);
		}

		public void InitModules(InputHandling inputHandler)
		{
			_inputHandler = inputHandler;
			_controlSettings.InitModules(_inputHandler);
		}

		protected override void UpdateVisualsForLoadedData()
		{
			ControlSettingsData controlSettingsData = (ControlSettingsData)_controlSettings.SettingsData;
			_gamepadVibrationToggle.isOn = controlSettingsData.GamepadVibration;
		}

		private void OnMouseSensitivityChanged(float value)
		{
			_controlSettings.SetMouseSensitivity(value);
		}

		private void OnGamepadSensitivityChanged(float value)
		{
			_controlSettings.SetGamepadSensitivity(value);
		}

		private void OnGamepadRoomSensitivityChanged(float value)
		{
			_controlSettings.SetGamepadRoomSensitivity(value);
		}

		private void OnGamepadVibrationChanged(bool isEnabled)
		{
			_controlSettings.SetGamepadVibration(isEnabled);
		}

		public void OnStarted()
		{
			ControlSettingsData controlSettingsData = (ControlSettingsData)_controlSettings.SettingsData;
			_gamepadSensitivity.SetValue(controlSettingsData.GamepadSensitivity);
			_gamepadRoomSensitivity.SetValue(controlSettingsData.GamepadRoomSensitivity);
			_mouseSensitivity.SetValue(controlSettingsData.MouseSensitivity);
			_invertGamepadY.isOn = controlSettingsData.InvertGamepadY;
			_invertMouseY.isOn = controlSettingsData.InvertMouseY;
		}
	}
}
