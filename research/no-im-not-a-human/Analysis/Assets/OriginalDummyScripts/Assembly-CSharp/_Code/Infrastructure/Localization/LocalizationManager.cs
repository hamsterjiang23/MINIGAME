using System;
using System.Linq;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using _Code.Language;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Localization;

public sealed class LocalizationManager : ILocalizationManager
{
	private readonly IDataModelService _dataModelService;

	public ELanguage CurrentLanguage { get; private set; }

	public event Action<ELanguage> LanguageChanged;

	public LocalizationManager(IDataModelService dataModelService)
	{
		_dataModelService = dataModelService;
	}

	public void ChangeLanguage(ELanguage language)
	{
		CurrentLanguage = language;
		LanguageChanged?.Invoke(CurrentLanguage);
		LocalizationSettings.SelectedLocale = GetLocale(language);
	}

	private Locale GetLocale(ELanguage language)
	{
		Locale locale = LocalizationSettings.AvailableLocales.Locales.FirstOrDefault((Locale x) => x.Identifier.Code == language.ToString().Replace('_', '-'));
		if (locale == null)
		{
			return LocalizationSettings.AvailableLocales.Locales[0];
		}
		return locale;
	}

	public void NextLanguage()
	{
		CurrentLanguage++;
		if ((int)CurrentLanguage >= Enum.GetNames(typeof(ELanguage)).Length)
		{
			CurrentLanguage = ELanguage.en;
		}
		LocalizationSettings.SelectedLocale = GetLocale(CurrentLanguage);
		LanguageChanged?.Invoke(CurrentLanguage);
	}

	public void PreviousLanguage()
	{
		CurrentLanguage--;
		if (CurrentLanguage < ELanguage.en)
		{
			CurrentLanguage = (ELanguage)(Enum.GetNames(typeof(ELanguage)).Length - 1);
		}
		LocalizationSettings.SelectedLocale = GetLocale(CurrentLanguage);
		LanguageChanged?.Invoke(CurrentLanguage);
	}
}
