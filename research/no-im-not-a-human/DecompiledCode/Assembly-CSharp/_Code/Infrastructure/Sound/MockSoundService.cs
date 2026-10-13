using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Sound.Settings;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Sound;

public sealed class MockSoundService : INotAHumanSoundService, ISoundService
{
	public void Init()
	{
		throw new NotImplementedException();
	}

	public void SetEnabled(ESoundSource source, bool isEnabled)
	{
		throw new NotImplementedException();
	}

	public UniTask FadeVolume(ESoundSource source, float targetVolume, float duration, Ease ease = Ease.Unset)
	{
		throw new NotImplementedException();
	}

	public UniTask FadeIn(ESoundSource source, ESound clip, float duration, Ease ease = Ease.Unset, params ASoundSetting[] settings)
	{
		throw new NotImplementedException();
	}

	public UniTask FadeOut(ESoundSource source, float duration, Ease ease = Ease.Unset)
	{
		throw new NotImplementedException();
	}

	public UniTask PlayOneShotAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings)
	{
		throw new NotImplementedException();
	}

	public UniTask PlayAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings)
	{
		throw new NotImplementedException();
	}

	public void PlayOneShot(ESoundSource source, ESound clip, params ASoundSetting[] settings)
	{
		throw new NotImplementedException();
	}

	public void Play(ESoundSource source, ESound clip, params ASoundSetting[] settings)
	{
		throw new NotImplementedException();
	}

	public void Stop(ESoundSource source)
	{
		throw new NotImplementedException();
	}

	public void Pause(ESoundSource source)
	{
		throw new NotImplementedException();
	}

	public void Start(ESoundSource source)
	{
		throw new NotImplementedException();
	}

	public GameObject GetSourceObject(ESoundSource s1GameScene3DRandomEmitter)
	{
		throw new NotImplementedException();
	}

	public float GetVolume(ESoundSource source)
	{
		throw new NotImplementedException();
	}

	public void DisableDayTheme(float time)
	{
		throw new NotImplementedException();
	}

	public void DisableNightTheme(float time)
	{
		throw new NotImplementedException();
	}

	public void EnableNightTheme(int day, float time)
	{
		throw new NotImplementedException();
	}

	public void EnableDayTheme(int day, float time)
	{
		throw new NotImplementedException();
	}
}
