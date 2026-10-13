namespace _Scripts.Services.Sound.Service;

public interface INotAHumanSoundService : ISoundService
{
	void DisableDayTheme(float time);

	void DisableNightTheme(float time);

	void EnableNightTheme(int day, float time);

	void EnableDayTheme(int day, float time);
}
