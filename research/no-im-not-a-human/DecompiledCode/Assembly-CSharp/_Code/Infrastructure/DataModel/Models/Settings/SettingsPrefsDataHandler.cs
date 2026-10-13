using Cysharp.Threading.Tasks;
using _Code.Infrastructure.Settings.Control;
using _Code.Infrastructure.Settings.Language;
using _Code.Infrastructure.Settings.Screen;
using _Code.Infrastructure.Settings.Sound;
using _Scripts.Services.DataModel.DataStorages;
using _Scripts.Services.DataModel.Models;

namespace _Code.Infrastructure.DataModel.Models.Settings;

public sealed class SettingsPrefsDataHandler : ABaseDataHandler<SettingsPrefsData>, ISettingsPrefsDataHandler
{
	protected override bool UseSteamCloud => false;

	public SoundSettingsData SoundSettings
	{
		get
		{
			return Data.SoundSettingsData;
		}
		set
		{
			Data.SoundSettingsData = value;
			SaveData().Forget();
		}
	}

	public ScreenSettingsData ScreenSettings
	{
		get
		{
			return Data.ScreenSettingsData;
		}
		set
		{
			Data.ScreenSettingsData = value;
			SaveData().Forget();
		}
	}

	public TextSettingsData TextSettings
	{
		get
		{
			return Data.TextSettingsData;
		}
		set
		{
			Data.TextSettingsData = value;
			SaveData().Forget();
		}
	}

	public ControlSettingsData ControlSettings
	{
		get
		{
			return Data.ControlSettingsData;
		}
		set
		{
			Data.ControlSettingsData = value;
			SaveData().Forget();
		}
	}

	public SettingsPrefsDataHandler(IDataStorage dataStorage)
		: base(dataStorage)
	{
	}
}
