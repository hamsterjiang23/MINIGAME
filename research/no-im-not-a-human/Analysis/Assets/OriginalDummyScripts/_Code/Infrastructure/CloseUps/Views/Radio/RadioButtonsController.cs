using System;
using System.Runtime.CompilerServices;
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

		[CompilerGenerated]
		private Action<ERadioState> m_Pressed;

		public event Action<ERadioState> Pressed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public void Init(ERadioState initState)
		{
		}

		public void InitModules(InputHandling inputHandler)
		{
		}

		private void Update()
		{
		}

		private void OnAmButtonPressed()
		{
		}

		private void OnFmButtonPressed()
		{
		}

		private void HandleButtonPressed(ERadioState state)
		{
		}

		public void UpdateButtonStates(ERadioState selectedState)
		{
		}

		public void SetOutlineActiveState(bool isEnabled)
		{
		}
	}
}
