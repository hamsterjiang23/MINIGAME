using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;

namespace _Scripts.Services.Sound.Instance;

public sealed class SoundServiceInstance : MonoBehaviour
{
	[SerializeReference]
	[SerializeField]
	private SoundServiceSourceArray _soundSources;

	[SerializeField]
	private SoundsList _soundsList;

	public async UniTask PlayAsync(ESoundSource source, ESound sound)
	{
		Play(source, sound);
		await UniTask.Delay(TimeSpan.FromSeconds(_soundsList[sound].length));
	}

	public async UniTask PlayOneShotAsync(ESoundSource source, ESound clip)
	{
		PlayOneShot(source, clip);
		await UniTask.Delay(TimeSpan.FromSeconds(_soundsList[clip].length));
	}

	public void Play(ESoundSource source, ESound sound)
	{
		AudioSource audioSource = _soundSources[source];
		audioSource.clip = _soundsList[sound];
		audioSource.Play();
	}

	public void PlayOneShot(ESoundSource source, ESound clip)
	{
		AudioClip clip2 = _soundsList[clip];
		_soundSources[source].PlayOneShot(clip2);
	}

	public void SetEnabled(ESoundSource source, bool isEnabled)
	{
		_soundSources[source].enabled = isEnabled;
	}

	public async UniTask FadeVolume(ESoundSource source, float targetVolume, float duration, Ease ease = Ease.Unset)
	{
		if (!_soundSources.Contains(source))
		{
			Debug.LogWarning("[FadeVolume] Sound Source does not exist");
		}
		else
		{
			await (Tween)_soundSources[source].DOFade(targetVolume, duration).SetEase(ease);
		}
	}

	public void Stop(ESoundSource source)
	{
		if (!_soundSources.Contains(source))
		{
			Debug.LogWarning("[Stop] Sound Source does not exist");
		}
		else
		{
			_soundSources[source].Stop();
		}
	}

	public void Pause(ESoundSource source)
	{
		_soundSources[source].Pause();
	}

	public void ApplySettingToSource(ESoundSource source, ASoundSetting[] settings)
	{
		AudioSource audioSource = _soundSources[source];
		audioSource.pitch = 1f;
		audioSource.loop = false;
		foreach (ASoundSetting aSoundSetting in settings)
		{
			if (aSoundSetting == null)
			{
				continue;
			}
			string text = aSoundSetting.GetType().Name;
			if (!(text == "SoundSettingRandomPitch"))
			{
				if (text == "SoundSettingCycling")
				{
					audioSource.loop = true;
				}
			}
			else
			{
				audioSource.pitch = UnityEngine.Random.Range(((SoundSettingRandomPitch)aSoundSetting).MinPitch, ((SoundSettingRandomPitch)aSoundSetting).MinPitch);
			}
		}
	}

	public void StartSource(ESoundSource source)
	{
		_soundSources[source].Play();
	}

	public GameObject GetGameObject(ESoundSource source)
	{
		return _soundSources[source].gameObject;
	}

	public AudioSource GetSource(ESoundSource source)
	{
		return _soundSources[source];
	}
}
