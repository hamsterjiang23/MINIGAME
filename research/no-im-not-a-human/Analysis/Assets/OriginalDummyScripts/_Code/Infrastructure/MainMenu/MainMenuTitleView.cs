using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuTitleView : MonoBehaviour
	{
		[SerializeField]
		private LocalizeStringEvent _titleText;

		[SerializeField]
		private LocalizedString _noiseSymbols;

		private const int MIN_WORD_LENGTH = 4;

		private const int MAX_WORD_LENGTH = 7;

		private void Start()
		{
		}

		public void Change()
		{
		}
	}
}
