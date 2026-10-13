using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace _Code.Infrastructure.Settings.Language
{
	public sealed class TextSettings : ISetting
	{
		private TextSettingsData _settingsData = new TextSettingsData();

		public ASettingsData SettingsData
		{
			get
			{
				return _settingsData;
			}
			set
			{
				_settingsData = (TextSettingsData)value;
			}
		}

		public bool IsUseTypewriter
		{
			get
			{
				return _settingsData.IsUseTypewriter;
			}
			set
			{
				_settingsData.IsUseTypewriter = value;
			}
		}

		public void SelectLanguage(Locale selectedLanguage)
		{
			_settingsData.SelectedLanguage = selectedLanguage.Identifier.Code;
			LocalizationSettings.SelectedLocale = selectedLanguage;
		}
	}
}
