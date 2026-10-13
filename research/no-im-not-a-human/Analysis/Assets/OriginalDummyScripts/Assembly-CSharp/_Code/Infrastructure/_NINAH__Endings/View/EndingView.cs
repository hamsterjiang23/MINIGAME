using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure.Sound.Settings;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure._NINAH__Endings.View;

public sealed class EndingView : MonoBehaviour
{
	[Obsolete]
	[SerializeField]
	private AnimatedImage _image;

	[SerializeField]
	private VideoPlayer _videoPlayer;

	[SerializeField]
	private RTLTextMeshPro _subtitlesText;

	[SerializeField]
	private CanvasGroup _skipFillHint;

	[SerializeField]
	private Image _skippFillBar;

	[SerializeField]
	private ControlView _controlView;

	private INotAHumanSoundService _soundService;

	private InputHandling _inputHandler;

	private CustomYarnReader _yarnReader;

	private bool _clicked;

	private CancellationTokenSource _cancellationToken = new CancellationTokenSource();

	private CancellationTokenSource _cancellationTokenSubtitles = new CancellationTokenSource();

	private float _skipProgress;

	private int _resetsHintCount;

	private bool _isShowing;

	private bool _isSkipping;

	private IDataModelService _dataModelService;

	private EndingViewSOData _currentEnding;

	private bool _isUnlock;

	private WatcherManager _watcherManager;

	private const float SkipDuration = 1.5f;

	public event Action Ended;

	public event Action<bool> EndingUnlocked;

	public void Init(INotAHumanSoundService soundService, CustomYarnReader yarnReader, InputHandling inputHandling, IDataModelService dataModelService, WatcherManager watcherManager)
	{
		_soundService = soundService;
		_yarnReader = yarnReader;
		_inputHandler = inputHandling;
		_dataModelService = dataModelService;
		_watcherManager = watcherManager;
		_inputHandler.InputDeviceChanged -= OnInputChanged;
		_inputHandler.InputDeviceChanged += OnInputChanged;
	}

	private void OnInputChanged(EInputDevice device)
	{
		if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
		{
			_controlView.InitGamepadKey(_inputHandler.GetKey(EControl.SkipVideo), EControl.SkipVideo, _inputHandler.GetGaypadType());
			_controlView.transform.localScale = Vector3.one * 1.5f;
		}
		else
		{
			_controlView.InitKey(_inputHandler.GetKey(EControl.SkipVideo), EControl.SkipVideo);
			_controlView.transform.localScale = Vector3.one * 0.95f;
		}
	}

	public void ShowEnding(EndingViewSOData data, bool isUnlock = false)
	{
		if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
		{
			_controlView.InitGamepadKey(_inputHandler.GetKey(EControl.SkipVideo), EControl.SkipVideo, _inputHandler.GetGaypadType());
			_controlView.transform.localScale = Vector3.one * 1.5f;
		}
		else
		{
			_controlView.InitKey(_inputHandler.GetKey(EControl.SkipVideo), EControl.SkipVideo);
			_controlView.transform.localScale = Vector3.one * 0.95f;
		}
		_currentEnding = data;
		_isUnlock = isUnlock;
		gameObject.SetActive(value: true);
		_videoPlayer.clip = data.VideoClip;
		_videoPlayer.Play();
		_isShowing = true;
		_subtitlesText.text = "";
		ShowSubtitles(data.Subtitles).Forget();
		StartCheckingVideoEnd((float)data.VideoClip.length).Forget();
		_watcherManager.EnterState(EWatcherState.Movie);
	}

	private async UniTaskVoid StartCheckingVideoEnd(float length)
	{
		await UniTask.Delay(TimeSpan.FromSeconds(length), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationToken.Token);
		gameObject.SetActive(value: false);
		_videoPlayer.Stop();
		_skipProgress = 0f;
		_skippFillBar.fillAmount = 0f;
		_skipFillHint.alpha = 0f;
		_cancellationTokenSubtitles.Cancel();
		_cancellationTokenSubtitles.Dispose();
		_cancellationTokenSubtitles = new CancellationTokenSource();
		if (_isUnlock)
		{
			_dataModelService.MetaPrefsDataHandler.ReachEnding(_currentEnding.EndingType);
			EndingUnlocked?.Invoke(_dataModelService.MetaPrefsDataHandler.IsGameOverEnding(_currentEnding.EndingType));
			_isUnlock = false;
		}
		_isShowing = false;
		_watcherManager.LeaveState(EWatcherState.Movie);
		Ended?.Invoke();
	}

	private async UniTask ShowSubtitles(EndingSubtitlesData dataSubtitles)
	{
		EndingSubtitleData[] endingSubtitles = dataSubtitles.EndingSubtitles;
		foreach (EndingSubtitleData subtitle in endingSubtitles)
		{
			if (!_isShowing)
			{
				break;
			}
			await UniTask.Delay(TimeSpan.FromSeconds(subtitle.BeforeNodeDelay), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationTokenSubtitles.Token);
			string[] nodeContent = _yarnReader.GetNodeContent(subtitle.Node);
			float lineDuration = subtitle.Duration / (float)nodeContent.Length;
			string[] array = nodeContent;
			foreach (string node in array)
			{
				float appearTime = lineDuration * 0.3f;
				float showTime = lineDuration - appearTime;
				for (int k = 0; k < node.Length; k++)
				{
					RTLTextMeshPro subtitlesText = _subtitlesText;
					string text = node.Substring(0, k + 1);
					string text2 = node;
					int num = k + 1;
					subtitlesText.text = text + "<color=#00000000>" + text2.Substring(num, text2.Length - num) + "</color>";
					await UniTask.Delay(TimeSpan.FromSeconds(appearTime / (float)node.Length), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationTokenSubtitles.Token);
				}
				await UniTask.Delay(TimeSpan.FromSeconds(showTime), ignoreTimeScale: false, PlayerLoopTiming.Update, _cancellationTokenSubtitles.Token);
				if (!_isShowing)
				{
					return;
				}
			}
			if (!_isShowing)
			{
				break;
			}
			_subtitlesText.text = "";
		}
	}

	[Obsolete]
	private async UniTaskVoid ShowSlides(EndingViewDataSlide[] dataSlides)
	{
		int currentSlide = 0;
		while (currentSlide < dataSlides.Length)
		{
			_image.SetData(dataSlides[currentSlide].AnimationData);
			_image.StartAnimationOrSetFirstFrame();
			if (dataSlides[currentSlide].Sound.UseSound)
			{
				_soundService.Play(ESoundSource.s1_GameScene_2DSFXSource, dataSlides[currentSlide].Sound.Sound, dataSlides[currentSlide].Sound.IsLooped ? new SoundSettingCycling() : null);
			}
			string[] textLines = _yarnReader.GetNodeContent(dataSlides[currentSlide].TextNodeName);
			for (int currentTextLine = 0; currentTextLine < textLines.Length; currentTextLine++)
			{
				_clicked = false;
				_subtitlesText.text = textLines[currentTextLine];
				await UniTask.WaitUntil(() => _clicked);
			}
			currentSlide++;
			_soundService.Stop(ESoundSource.s1_GameScene_2DSFXSource);
		}
		_soundService.Stop(ESoundSource.s1_GameScene_2DMusicSourceExtra);
		gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (_isShowing)
		{
			if (_inputHandler.AnyKeyClicked)
			{
				_skipFillHint.DOFade(0.5f, 0.3f).SetEase(Ease.InCubic);
				ResetSkipHint().Forget();
			}
			if (_inputHandler.SkipVideoPressed && _isSkipping)
			{
				_skipProgress += Time.deltaTime / 1.5f;
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
