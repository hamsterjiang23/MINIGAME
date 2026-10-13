using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace _Code.Infrastructure.MainMenu;

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
		Change();
	}

	public void Change()
	{
		string text = "";
		string array = _noiseSymbols.GetLocalizedString() + _noiseSymbols.GetLocalizedString() + "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~";
		for (int i = 0; i < Random.Range(4, 7); i++)
		{
			text += array.GetRandomElement();
		}
		_titleText.StringReference.Arguments = new List<object> { text };
		_titleText.RefreshString();
	}
}
