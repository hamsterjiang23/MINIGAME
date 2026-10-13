using System;
using UnityEngine;
using _Code.Player;

namespace _Code.Infrastructure.CloseUps.Views.Radio
{
	public sealed class RadioButtonsController : MonoBehaviour
	{
		[SerializeField]
		private RadioButtonView _amButton;

		[SerializeField]
		private RadioButtonView _fmButton;

		private InputHandling _inputHandler;

		public event Action<ERadioState> Pressed;

		public void Init(ERadioState initState)
		{
			_amButton.Pressed += HandleButtonPressed;
			_fmButton.Pressed += HandleButtonPressed;
			UpdateButtonStates(initState);
		}

		public void InitModules(InputHandling inputHandler)
		{
			_inputHandler = inputHandler;
		}

		private void Update()
		{
			if (_inputHandler.UIRadioLeftHandle)
			{
				OnAmButtonPressed();
			}
			if (_inputHandler.UIRadioRightHandle)
			{
				OnFmButtonPressed();
			}
		}

		private void OnAmButtonPressed()
		{
			HandleButtonPressed(ERadioState.AM);
		}

		private void OnFmButtonPressed()
		{
			HandleButtonPressed(ERadioState.FM);
		}

		private void HandleButtonPressed(ERadioState state)
		{
			Pressed?.Invoke(state);
			UpdateButtonStates(state);
		}

		public void UpdateButtonStates(ERadioState selectedState)
		{
			_amButton.SetState(selectedState == ERadioState.AM);
			_fmButton.SetState(selectedState == ERadioState.FM);
		}

		public void SetOutlineActiveState(bool isEnabled)
		{
			_amButton.SetOutlineEnabled(isEnabled);
			_fmButton.SetOutlineEnabled(isEnabled);
		}
	}
}
