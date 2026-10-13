using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Scenes;
using _Code.Player;

namespace _Code.Menues.Titles
{
	public sealed class TitlesControl : MonoBehaviour
	{
		[SerializeField]
		private CanvasGroup _skipFillHint;

		[SerializeField]
		private Image _skippFillBar;

		[SerializeField]
		private ControlView _controlView;

		private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

		private float _skipProgress;

		private const float SkipDuration = 3f;

		private InputHandling _inputHandler;

		private bool _isSkipping;

		private int _resetsHintCount;

		private WatcherManager _watcherManager;

		public void Init(IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
		{
			_inputHandler = inputHandlerProvider.InputHandler;
			_watcherManager = watcherManager;
			_inputHandler.ForceSetUI();
			StartCheckingVideoEnd(100f).Forget();
		}

		private async UniTaskVoid StartCheckingVideoEnd(float length)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(length), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationToken.Token);
			gameObject.SetActive(value: false);
			_skipProgress = 0f;
			_skippFillBar.fillAmount = 0f;
			_skipFillHint.alpha = 0f;
			_watcherManager.LeaveState(EWatcherState.Credits);
			await ScenesChanger.OpenScene(0);
		}

		private void Update()
		{
			if (_inputHandler.AnyKeyClicked)
			{
				_skipFillHint.DOFade(0.5f, 0.3f).SetEase(Ease.InCubic);
				ResetSkipHint().Forget();
			}
			if (_inputHandler.SkipVideoPressed && _isSkipping)
			{
				_skipProgress += Time.deltaTime / 3f;
				_skippFillBar.fillAmount = _skipProgress;
			}
			if (_inputHandler.SkipVideoClicked)
			{
				_resetsHintCount++;
				_isSkipping = true;
			}
			if (_inputHandler.SkipVideoReleased)
			{
				_resetsHintCount--;
				_skipProgress = 0f;
				_skippFillBar.fillAmount = 0f;
				_isSkipping = false;
				ResetSkipHint().Forget();
			}
			if (_skipProgress >= 1f)
			{
				_cancellationToken.Cancel();
				_cancellationToken.Dispose();
				_cancellationToken = new CancellationTokenSource();
				StartCheckingVideoEnd(0f).Forget();
			}
		}

		private async UniTaskVoid ResetSkipHint()
		{
			_resetsHintCount++;
			await UniTask.Delay(TimeSpan.FromSeconds(1.5));
			_resetsHintCount--;
			if (_resetsHintCount == 0)
			{
				_skipFillHint.DOFade(0f, 0.3f).SetEase(Ease.InCubic);
			}
		}
	}
}
