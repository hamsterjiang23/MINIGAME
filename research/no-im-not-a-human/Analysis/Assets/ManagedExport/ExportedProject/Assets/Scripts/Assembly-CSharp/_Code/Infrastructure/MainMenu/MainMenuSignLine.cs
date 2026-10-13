using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuSignLine : MonoBehaviour
	{
		[SerializeField]
		private MainMenuSignLineElement[] _elements;

		[SerializeField]
		private EMoveDirection _moveDirection;

		[SerializeField]
		private float _moveDelay = 1f;

		[SerializeField]
		private float _moveDuration = 1f;

		[SerializeField]
		private Sprite[] _spritesArray;

		private bool _isStopped;

		private int _currentFirstElement;

		private int _currentLastElement;

		private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

		public event Action<Sprite> Moved;

		public void Init()
		{
			_currentLastElement = _elements.Length - 1;
			MainMenuSignLineElement[] elements = _elements;
			for (int i = 0; i < elements.Length; i++)
			{
				elements[i].SetSprite(_spritesArray.GetRandomElementWithoutRepeating());
			}
			if (_moveDirection == EMoveDirection.Downwards)
			{
				MoveDown().Forget();
			}
			else
			{
				MoveUp().Forget();
			}
		}

		private async UniTask MoveDown()
		{
			if (_isStopped)
			{
				return;
			}
			Vector3 position = _elements[_currentFirstElement].transform.position;
			for (int i = 0; i < _elements.Length; i++)
			{
				if (i != _currentLastElement)
				{
					if (i < _elements.Length - 1)
					{
						_elements[i].transform.DOLocalMoveY(_elements[i + 1].transform.localPosition.y, _moveDuration).SetEase(Ease.InQuad);
					}
					else
					{
						_elements[i].transform.DOLocalMoveY(_elements[0].transform.localPosition.y, _moveDuration).SetEase(Ease.InQuad);
					}
				}
			}
			_elements[_currentLastElement].transform.position = position;
			_currentFirstElement--;
			_currentLastElement--;
			if (_currentLastElement < 0)
			{
				_currentLastElement = _elements.Length - 1;
			}
			if (_currentFirstElement < 0)
			{
				_currentFirstElement = _elements.Length - 1;
			}
			await UniTask.Delay(TimeSpan.FromSeconds(_moveDuration), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationToken.Token);
			Moved?.Invoke(GetMiddleSprite());
			await UniTask.Delay(TimeSpan.FromSeconds(_moveDelay), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationToken.Token);
			_elements[_currentFirstElement].SetSprite(_spritesArray.GetRandomElementWithoutRepeating());
			MoveDown().Forget();
		}

		private async UniTask MoveUp()
		{
			if (_isStopped)
			{
				return;
			}
			Vector3 position = _elements[_currentLastElement].transform.position;
			for (int num = _elements.Length - 1; num >= 0; num--)
			{
				if (num != _currentFirstElement)
				{
					if (num > 0)
					{
						_elements[num].transform.DOLocalMoveY(_elements[num - 1].transform.localPosition.y, _moveDuration).SetEase(Ease.InQuad);
					}
					else
					{
						Transform target = _elements[num].transform;
						MainMenuSignLineElement[] elements = _elements;
						target.DOLocalMoveY(elements[elements.Length - 1].transform.localPosition.y, _moveDuration).SetEase(Ease.InQuad);
					}
				}
			}
			_elements[_currentFirstElement].transform.position = position;
			_currentFirstElement++;
			_currentLastElement++;
			if (_currentLastElement >= _elements.Length)
			{
				_currentLastElement = 0;
			}
			if (_currentFirstElement >= _elements.Length)
			{
				_currentFirstElement = 0;
			}
			await UniTask.Delay(TimeSpan.FromSeconds(_moveDuration), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationToken.Token);
			Moved?.Invoke(GetMiddleSprite());
			await UniTask.Delay(TimeSpan.FromSeconds(_moveDelay), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationToken.Token);
			_elements[_currentLastElement].SetSprite(_spritesArray.GetRandomElementWithoutRepeating());
			MoveUp().Forget();
		}

		private Sprite GetMiddleSprite()
		{
			int num = _currentFirstElement + _elements.Length / 2;
			if (num >= _elements.Length)
			{
				num -= _elements.Length;
			}
			return _elements[num].GetSprite();
		}

		public void Stop()
		{
			_isStopped = true;
			_cancellationToken.Cancel();
			_cancellationToken.Dispose();
			_cancellationToken = new CancellationTokenSource();
		}
	}
}
