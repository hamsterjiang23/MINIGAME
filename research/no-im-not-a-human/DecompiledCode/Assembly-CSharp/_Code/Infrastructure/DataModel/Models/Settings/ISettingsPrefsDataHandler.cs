using Cysharp.Threading.Tasks;
using _Code.Infrastructure.Settings.Control;
using _Code.Infrastructure.Settings.Language;
using _Code.Infrastructure.Settings.Screen;
using _Code.Infrastructure.Settings.Sound;

namespace _Code.Infrastructure.DataModel.Models.Settings;

public interface ISettingsPrefsDataHandler
{
	SoundSettingsData SoundSettings { get; set; }

	TextSettingsData TextSettings { get; set; }

	ScreenSettingsData ScreenSettings { get; set; }

	ControlSettingsData ControlSettings { get; set; }

	UniTask LoadData();
}
