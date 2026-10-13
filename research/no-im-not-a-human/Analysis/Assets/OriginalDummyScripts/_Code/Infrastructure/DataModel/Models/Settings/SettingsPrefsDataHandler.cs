using _Code.Infrastructure.Settings.Control;
using _Code.Infrastructure.Settings.Language;
using _Code.Infrastructure.Settings.Screen;
using _Code.Infrastructure.Settings.Sound;
using _Scripts.Services.DataModel.DataStorages;
using _Scripts.Services.DataModel.Models;

namespace _Code.Infrastructure.DataModel.Models.Settings
{
	public sealed class SettingsPrefsDataHandler : ABaseDataHandler<SettingsPrefsData>, ISettingsPrefsDataHandler
	{
		protected override bool UseSteamCloud => false;

		public SoundSettingsData SoundSettings
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public ScreenSettingsData ScreenSettings
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public TextSettingsData TextSettings
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public ControlSettingsData ControlSettings
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public SettingsPrefsDataHandler(IDataStorage dataStorage)
			: base((IDataStorage)null)
		{
		}
	}
}
