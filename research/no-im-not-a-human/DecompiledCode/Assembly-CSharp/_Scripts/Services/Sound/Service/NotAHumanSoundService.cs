using Cysharp.Threading.Tasks;
using DG.Tweening;
using DefaultNamespace.EnumEventBus;
using UnityEngine.SceneManagement;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;

namespace _Scripts.Services.Sound.Service;

public sealed class NotAHumanSoundService : SoundService, INotAHumanSoundService, ISoundService
{
	private readonly IMusicSOData _musicData;

	private readonly CommonEnumEventus _commonEnumEventus;

	public NotAHumanSoundService(ISoundServiceInstanceProvider soundServiceInstanceProvider, IMusicSOData musicSoData, CommonEnumEventus commonEnumEventus)
		: base(soundServiceInstanceProvider)
	{
		_musicData = musicSoData;
		_commonEnumEventus = commonEnumEventus;
		if (SceneManager.GetActiveScene().buildIndex == 1)
		{
			_commonEnumEventus.SubscribeAction<ESoundSource, ESound, float, bool>(ECommonEvents.PlaySound, PlayFromEventus);
			_commonEnumEventus.SubscribeAction<ESoundSource, float>(ECommonEvents.StopSound, StopFromEventus);
			_commonEnumEventus.SubscribeAction<ESoundSource, ESound>(ECommonEvents.PlaySoundOneShot, PlayFromEventusOneShot);
		}
	}

	private void PlayFromEventus(ESoundSource source, ESound sound, float fadeTime, bool isCycle)
	{
		PlayFromEventusAsync(source, sound, fadeTime, isCycle).Forget();
	}

	private void StopFromEventus(ESoundSource source, float fadeTime)
	{
		StopFromEventusAsync(source, fadeTime).Forget();
	}

	private void PlayFromEventusOneShot(ESoundSource source, ESound sound)
	{
		PlayOneShot(source, sound);
	}

	private async UniTask PlayFromEventusAsync(ESoundSource source, ESound sound, float fadeTime, bool isCycle)
	{
		if (isCycle)
		{
			FadeIn(source, sound, fadeTime, Ease.InQuad, new SoundSettingCycling()).Forget();
		}
		else
		{
			FadeIn(source, sound, fadeTime, Ease.InQuad).Forget();
		}
	}

	private async UniTask StopFromEventusAsync(ESoundSource source, float fadeTime)
	{
		FadeOut(source, fadeTime, Ease.OutQuad).Forget();
	}

	public void DisableDayTheme(float time)
	{
		FadeOut(ESoundSource.s1_GameScene_2DMusicSourceDay, time, Ease.InQuad).Forget();
	}

	public void DisableNightTheme(float time)
	{
		FadeOut(ESoundSource.s1_GameScene_2DMusicSourceNight, time, Ease.InQuad).Forget();
		FadeOut(ESoundSource.s1_GameScene_2DMusicSourceNoise, time, Ease.InQuad).Forget();
	}

	public void EnableNightTheme(int day, float time)
	{
		ESound nightMusic = _musicData.MusicByDay[day].NightMusic;
		ESound noise = _musicData.MusicByDay[day].Noise;
		float noiseVolume = _musicData.MusicByDay[day].NoiseVolume;
		FadeIn(ESoundSource.s1_GameScene_2DMusicSourceNight, nightMusic, time, Ease.InQuad, new SoundSettingCycling()).Forget();
		Play(ESoundSource.s1_GameScene_2DMusicSourceNoise, noise, new SoundSettingCycling());
		FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceNoise, noiseVolume, time, Ease.InQuad).Forget();
	}

	public void EnableDayTheme(int day, float time)
	{
		ESound dayMusic = _musicData.MusicByDay[day].DayMusic;
		FadeIn(ESoundSource.s1_GameScene_2DMusicSourceDay, dayMusic, time, Ease.InQuad, new SoundSettingCycling()).Forget();
	}
}
