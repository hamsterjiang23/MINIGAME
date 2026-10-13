using System;
using System.Runtime.CompilerServices;
using UnityEngine.Localization;
using _Code.Language;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Localization
{
	public sealed class LocalizationManager : ILocalizationManager
	{
		[CompilerGenerated]
		private Action<ELanguage> m_LanguageChanged;

		private readonly IDataModelService _dataModelService;

		public ELanguage CurrentLanguage { get; private set; }

		public event Action<ELanguage> LanguageChanged
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

		public LocalizationManager(IDataModelService dataModelService)
		{
		}

		public void ChangeLanguage(ELanguage language)
		{
		}

		private Locale GetLocale(ELanguage language)
		{
			return null;
		}

		public void NextLanguage()
		{
		}

		public void PreviousLanguage()
		{
		}
	}
}
