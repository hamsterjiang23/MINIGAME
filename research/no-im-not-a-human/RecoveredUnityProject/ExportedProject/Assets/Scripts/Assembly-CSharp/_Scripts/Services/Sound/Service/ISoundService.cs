using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;

namespace _Scripts.Services.Sound.Service
{
	public interface ISoundService
	{
		void Init();

		void SetEnabled(ESoundSource source, bool isEnabled);

		UniTask FadeVolume(ESoundSource source, float targetVolume, float duration, Ease ease = Ease.Unset);

		UniTask FadeIn(ESoundSource source, ESound clip, float duration, Ease ease = Ease.Unset, params ASoundSetting[] settings);

		UniTask FadeOut(ESoundSource source, float duration, Ease ease = Ease.Unset);

		UniTask PlayOneShotAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings);

		UniTask PlayAsync(ESoundSource source, ESound clip, params ASoundSetting[] settings);

		void PlayOneShot(ESoundSource source, ESound clip, params ASoundSetting[] settings);

		void Play(ESoundSource source, ESound clip, params ASoundSetting[] settings);

		void Stop(ESoundSource source);

		void Pause(ESoundSource source);

		void Start(ESoundSource source);

		GameObject GetSourceObject(ESoundSource s1GameScene3DRandomEmitter);

		float GetVolume(ESoundSource source);
	}
}
