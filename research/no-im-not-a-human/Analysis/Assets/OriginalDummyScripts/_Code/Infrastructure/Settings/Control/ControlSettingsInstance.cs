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

		private readonly ControlSettings _controlSettings;

		private InputHandling _inputHandler;

		private bool _isSceneStarted;

		public override ISetting Setting => null;

		protected override void Init()
		{
		}

		private void OnGamepadInvertYChanged(bool isEnabled)
		{
		}

		private void OnMouseInvertYChanged(bool isEnabled)
		{
		}

		public void InitModules(InputHandling inputHandler)
		{
		}

		protected override void UpdateVisualsForLoadedData()
		{
		}

		private void OnMouseSensitivityChanged(float value)
		{
		}

		private void OnGamepadSensitivityChanged(float value)
		{
		}

		private void OnGamepadRoomSensitivityChanged(float value)
		{
		}

		private void OnGamepadVibrationChanged(bool isEnabled)
		{
		}

		public void OnStarted()
		{
		}
	}
}
