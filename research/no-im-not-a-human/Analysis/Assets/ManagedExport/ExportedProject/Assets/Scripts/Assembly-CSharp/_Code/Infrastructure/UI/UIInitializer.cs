using UnityEngine;
using _Code.Characters.DialogSystem;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.UI
{
	public sealed class UIInitializer : IUIInitializer
	{
		private HoverableButton[] _hoverableButtons;

		public UIInitializer(INotAHumanSoundService soundService)
		{
			_hoverableButtons = Object.FindObjectsByType<HoverableButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			HoverableButton[] hoverableButtons = _hoverableButtons;
			for (int i = 0; i < hoverableButtons.Length; i++)
			{
				hoverableButtons[i].Init(soundService);
			}
		}
	}
}
