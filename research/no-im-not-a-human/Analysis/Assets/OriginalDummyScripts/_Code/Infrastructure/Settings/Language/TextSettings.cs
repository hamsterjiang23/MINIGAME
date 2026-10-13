using UnityEngine.Localization;

namespace _Code.Infrastructure.Settings.Language
{
	public sealed class TextSettings : ISetting
	{
		private TextSettingsData _settingsData;

		public ASettingsData SettingsData
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public bool IsUseTypewriter
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public void SelectLanguage(Locale selectedLanguage)
		{
		}
	}
}
