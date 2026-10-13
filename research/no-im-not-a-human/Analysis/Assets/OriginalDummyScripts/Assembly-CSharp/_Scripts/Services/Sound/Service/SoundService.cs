using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Scripts.Services.Sound.Instance;

namespace _Scripts.Services.Sound.Service;

public class SoundService : ISoundService
{
	private readonly SoundServiceInstance _soundServiceInstance;

	public SoundService(ISoundServiceInstanceProvider soundServiceInstanceProvider)
	{
		_soundServiceInstance = soundServiceInstanceProvider.SoundServiceInstance;
	}

	public void Init()
	{
	}

	public void SetEnabled(ESoundSource source, bool isEnabled)
	{
		_soundServiceInstance.SetEnabled(source, isEnabled);
	}

	public async UniTask FadeVolume(ESoundSource source, float targetVolume, float duration, Ease ease = Ease.Unset)
	{
		await _soundServiceInstance.FadeVolume(source, targetVolume, duration, ease);
	}

	public async UniTask FadeIn(ESoundSource source, ESound clip, float duration, Ease ease = Ease.Unset, params ASoundSetting[] settings)
	{
		Play(source, clip, settings);
		await _soundServiceInstance.FadeVolume(source, 1f, duration, ease);
	}

	public async UniTask FadeOut(ESoundSource source, float duration, Ease ease = Ease.Unset)
	{
		await _soundServiceInstance.FadeVolume(source, 0f, duration, ease);
		_soundServiceInstance.Stop(source);
	}

	public async UniTask PlayOneShotAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings)
	{
		ApplySettingToSource(source, settings);
		await _soundServiceInstance.PlayOneShotAsync(source, clip);
	}

	public async UniTask PlayAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings)
	{
		ApplySettingToSource(source, settings);
		await _soundServiceInstance.PlayOneShotAsync(source, clip);
	}

	public void PlayOneShot(ESoundSource source, ESound clip, params ASoundSetting[] settings)
	{
		ApplySettingToSource(source, settings);
		_soundServiceInstance.PlayOneShot(source, clip);
	}

	public void Play(ESoundSource source, ESound clip, params ASoundSetting[] settings)
	{
		ApplySettingToSource(source, settings);
		_soundServiceInstance.Play(source, clip);
	}

	public void Stop(ESoundSource source)
	{
		_soundServiceInstance.Stop(source);
	}

	public void Pause(ESoundSource source)
	{
		_soundServiceInstance.Pause(source);
	}

	public void Start(ESoundSource source)
	{
		_soundServiceInstance.StartSource(source);
	}

	private void ApplySettingToSource(ESoundSource source, ASoundSetting[] settings)
	{
		_soundServiceInstance.ApplySettingToSource(source, settings);
	}

	public GameObject GetSourceObject(ESoundSource source)
	{
		return _soundServiceInstance.GetGameObject(source);
	}

	public float GetVolume(ESoundSource source)
	{
		return _soundServiceInstance.GetSource(source).volume;
	}
}
