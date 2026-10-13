using Cysharp.Threading.Tasks;
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
			if ((!(LinkedActionableObject != null) || LinkedActionableObject.CanShowHint) && LockedCount <= 0)
			{
				HUDPresenter.ShowHint(_subjectLocalizationKey.GetLocalizedString(), _actionLocalizationKey.GetLocalizedString(), base.transform, _icon).Forget();
			}
		}

		protected override void OnLostFocus()
		{
			HUDPresenter.HideHint().Forget();
		}

		protected override void OnTargetedWrongConditions()
		{
			HUDPresenter.SetHintFadedState(isFaded: true);
		}

		protected override void OnTargetedCorrectConditions()
		{
			HUDPresenter.SetHintFadedState(isFaded: false);
		}

		public void SetSubjectLocalizedString(LocalizedString localizedStringCigarettes)
		{
			_subjectLocalizationKey = localizedStringCigarettes;
		}
	}
}
