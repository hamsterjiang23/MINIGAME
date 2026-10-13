using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuHouseController : MonoBehaviour
	{
		[SerializeField]
		private Image _house;

		private float _minDelay = 10f;

		private float _maxDelay = 25f;

		private float _minDuration = 2f;

		private float _maxDuration = 5f;

		private float _minAlpha = 0.1f;

		private float _maxAlpha = 0.3f;

		private float _durationIn;

		private float _durationOut;

		private float _delay;

		private float _alpha;

		private float _lastAppear;

		private void Update()
		{
			if (!(Time.time < 10f) && Time.time >= _lastAppear + _delay + _durationIn + _durationOut)
			{
				_lastAppear = Time.time;
				_delay = UnityEngine.Random.Range(_minDelay, _maxDelay);
				_durationIn = UnityEngine.Random.Range(_minDuration, _maxDuration);
				_durationOut = UnityEngine.Random.Range(_minDuration, _maxDuration);
				_alpha = UnityEngine.Random.Range(_minAlpha, _maxAlpha);
				Appear().Forget();
			}
		}

		private async UniTask Appear()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(_delay));
			await (Tween)_house.DOFade(_alpha, _durationIn).SetEase(Ease.InSine);
			await (Tween)_house.DOFade(0f, _durationOut).SetEase(Ease.InSine);
		}
	}
}
