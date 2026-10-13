using ArabicSupport;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public sealed class ArabicTMPPreprocessor : ITextPreprocessor
{
	public string PreprocessText(string text)
	{
		if (!IsArabicLocale())
		{
			return text;
		}
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		return ArabicSupporter.Fix(text, showTashkeel: true, useHinduNumbers: true);
	}

	private static bool IsArabicLocale()
	{
		Locale selectedLocale = LocalizationSettings.SelectedLocale;
		if (selectedLocale != null)
		{
			return selectedLocale.Identifier.Code.StartsWith("ar");
		}
		return false;
	}
}
