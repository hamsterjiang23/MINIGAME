using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using _Code.Utils.Extensions;
using _Code.Utils.UI;

namespace _Code.Infrastructure.Settings.Language;

public sealed class TextSettingsDropdownInstance : ATextSettingsInstance
{
	[SerializeField]
	private ScrollableDropdown _dropdown;

	[SerializeField]
	private LocalizeStringEvent _textLocalizeEvent;

	[SerializeField]
	private RectTransform _textRectTransform;

	[SerializeField]
	private RTLTextMeshPro _text;

	private IReadOnlyList<Locale> _locales;

	private List<TMP_FontAsset> _fonts = new List<TMP_FontAsset>();

	private void OnEnable()
	{
		_textLocalizeEvent.OnUpdateString.AddListener(OnTextChanged);
		RefreshText().Forget();
	}

	private async UniTaskVoid RefreshText()
	{
		await UniTask.DelayFrame(1);
		_text.text = "";
		_textLocalizeEvent.RefreshString();
		LayoutRebuilder.ForceRebuildLayoutImmediate(_textRectTransform);
	}

	private void OnDisable()
	{
		_textLocalizeEvent.OnUpdateString.RemoveAllListeners();
	}

	protected override void InitInner()
	{
		_fonts.Clear();
		_dropdown.Dropdown.ClearOptions();
		_locales = LocalizationSettings.AvailableLocales.Locales;
		_dropdown.InitModules(InputHandler);
		foreach (Locale locale in _locales)
		{
			string localization = ("language_" + locale.Identifier.Code).GetLocalization("UI");
			_dropdown.Dropdown.options.Add(new TMP_Dropdown.OptionData(localization));
		}
		_dropdown.Dropdown.onValueChanged.RemoveAllListeners();
		_dropdown.Dropdown.onValueChanged.AddListener(OnLanguageSelected);
	}

	private void OnLanguageSelected(int index)
	{
		TextSettings.SelectLanguage(_locales[index]);
		CallLanguageChanged();
	}

	private void OnTextChanged(string text)
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(_textRectTransform);
	}

	protected override void UpdateVisualsForLoadedData()
	{
		base.UpdateVisualsForLoadedData();
		string localeId = ((TextSettingsData)TextSettings.SettingsData).SelectedLanguage;
		Locale item = LocalizationSettings.AvailableLocales.Locales.FirstOrDefault((Locale l) => l.Identifier.Code == localeId);
		_dropdown.Dropdown.value = ((List<Locale>)_locales).IndexOf(item);
	}

	public override void RequestChangeLanguage()
	{
		int value = _dropdown.Dropdown.value;
		value++;
		if (value >= _dropdown.Dropdown.options.Count)
		{
			value = 0;
		}
		_dropdown.Dropdown.value = value;
	}
}
