using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using _Code.Menues.HUD.Animations;

namespace _Code.Utils.UI.ImageAnimating;

public sealed class AnimatedImage : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private bool _isPlayFromAwake;

	[SerializeField]
	private AnimationData _animationData = new AnimationData();

	[SerializeField]
	private bool _ignoreTimescale;

	private bool _isBreakingAnimation;

	private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

	public Image Image => _image;

	private void OnEnable()
	{
		if (_isPlayFromAwake)
		{
			StartPlayingAnimation().Forget();
		}
	}

	private void OnDisable()
	{
		StopAnimation();
	}

	public void SetData(AnimationData animationData)
	{
		_animationData = animationData;
	}

	public void StartAnimationOrSetFirstFrame()
	{
		if (_animationData.Frames.Length == 1)
		{
			StopAnimation();
			_image.sprite = _animationData.Frames[0];
			_image.enabled = true;
		}
		else
		{
			StartPlayingAnimation().Forget();
		}
	}

	private async UniTask StartPlayingAnimation()
	{
		StopAnimation();
		float deltaTime = ((_animationData.FramesPerSecond <= 0) ? 0f : (1f / (float)_animationData.FramesPerSecond));
		int playCount = 0;
		int targetPlayCount = ((_animationData.CyclingType != EAnimationCyclingType.Cycle) ? 1 : 2147483647);
		_isBreakingAnimation = false;
		_image.enabled = true;
		int num;
		do
		{
			for (int i = 0; i < _animationData.Frames.Length; i++)
			{
				_image.sprite = _animationData.Frames[i];
				await UniTask.Delay(TimeSpan.FromSeconds(deltaTime), cancellationToken: _cancellationTokenSource.Token, ignoreTimeScale: _ignoreTimescale);
			}
			num = playCount + 1;
			playCount = num;
		}
		while (num < targetPlayCount && !_isBreakingAnimation);
		if (_animationData.CyclingType == EAnimationCyclingType.PlayOnce)
		{
			_image.enabled = false;
		}
	}

	public void StopAnimation()
	{
		_cancellationTokenSource.Cancel();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();
		_isBreakingAnimation = true;
		_image.enabled = false;
	}
}
