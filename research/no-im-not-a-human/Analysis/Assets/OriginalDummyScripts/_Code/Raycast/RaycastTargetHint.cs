using UnityEngine;
using UnityEngine.Localization;
using _Code.Menues.HUD;
using _Scripts.Raycast;

namespace _Code.Raycast
{
	public sealed class RaycastTargetHint : ARaycastTarget
	{
		[SerializeField]
		private LocalizedString _subjectLocalizationKey;

		[SerializeField]
		private LocalizedString _actionLocalizationKey;

		[SerializeField]
		private ERaycastHintIcon _icon;

		protected override void OnFocused()
		{
		}

		protected override void OnLostFocus()
		{
		}

		protected override void OnTargetedWrongConditions()
		{
		}

		protected override void OnTargetedCorrectConditions()
		{
		}

		public void SetSubjectLocalizedString(LocalizedString localizedStringCigarettes)
		{
		}
	}
}
