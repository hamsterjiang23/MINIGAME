using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace _Code.Utils;

public sealed class FontChanger : MonoBehaviour
{
	[SerializeField]
	private FontChangerSOData _fontChangerSOData;

	private TextMeshProUGUI _tmp;

	public void Init()
	{
		if ((object)_tmp == null)
		{
			_tmp = GetComponent<TextMeshProUGUI>();
		}
		LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
		LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;
		_tmp.font = _fontChangerSOData.Fonts[LocalizationSettings.SelectedLocale.Identifier.Code];
	}

	private void OnLocaleChanged(Locale locale)
	{
		_tmp.font = _fontChangerSOData.Fonts[locale.Identifier.Code];
	}
}
