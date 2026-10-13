using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Sound.Settings;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Sound
{
	public sealed class MockSoundService : INotAHumanSoundService, ISoundService
	{
		public void Init()
		{
		}

		public void SetEnabled(ESoundSource source, bool isEnabled)
		{
		}

		public UniTask FadeVolume(ESoundSource source, float targetVolume, float duration, Ease ease = Ease.Unset)
		{
			return default;
		}

		public UniTask FadeIn(ESoundSource source, ESound clip, float duration, Ease ease = Ease.Unset, params ASoundSetting[] settings)
		{
			return default;
		}

		public UniTask FadeOut(ESoundSource source, float duration, Ease ease = Ease.Unset)
		{
			return default;
		}

		public UniTask PlayOneShotAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings)
		{
			return default;
		}

		public UniTask PlayAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings)
		{
			return default;
		}

		public void PlayOneShot(ESoundSource source, ESound clip, params ASoundSetting[] settings)
		{
		}

		public void Play(ESoundSource source, ESound clip, params ASoundSetting[] settings)
		{
		}

		public void Stop(ESoundSource source)
		{
		}

		public void Pause(ESoundSource source)
		{
		}

		public void Start(ESoundSource source)
		{
		}

		public GameObject GetSourceObject(ESoundSource s1GameScene3DRandomEmitter)
		{
			return null;
		}

		public float GetVolume(ESoundSource source)
		{
			return 0f;
		}

		public void DisableDayTheme(float time)
		{
		}

		public void DisableNightTheme(float time)
		{
		}

		public void EnableNightTheme(int day, float time)
		{
		}

		public void EnableDayTheme(int day, float time)
		{
		}
	}
}
