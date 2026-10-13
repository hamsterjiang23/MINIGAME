using System;
using System.Linq;
using Code.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RTLTMPro;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using _Code.Characters.DialogSystem;
using _Code.DialogSystem;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Player;
using _Code.Raycast;
using _Code.Utils.CustomYarnReading;
using _Code.Utils.Extensions;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;

namespace _Code.Infrastructure.CloseUps.Views.Radio
{
	public sealed class RadioCloseUpView : ACloseUpView
	{
		[SerializeField]
		private Image _bg;

		[SerializeField]
		private RTLTextMeshPro _messageBox;

		[SerializeField]
		private RadioKnobController _radioKnobController;

		[SerializeField]
		private RadioButtonsController _radioButtonsController;

		[SerializeField]
		private Image _fill;

		[SerializeField]
		private RaycastTargetHint _raycastTarget;

		[SerializeField]
		private RadioTutorial _tutorial;

		[SerializeField]
		private RectTransform _radioContent;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _voiceSounds;

		private Vector2 _radioStartPos;

		private Vector2 _radioHidePos;

		private RadioModel _radioModel;

		private IOtherGameSODataProvider _otherGameSoDataProvider;

		private CloseUpSaveData _saveData;

		private CustomYarnReader _customYarnReader;

		private IDayNightController _dayNightController;

		private IDialogManager _dialogManager;

		private bool _isTutorActive;

		private InputHandling _inputHandler;

		private const float MONOSPACE_SCALE = 0.75f;

		private const float MONOSPACE_SCALE_HIEROGLYPHS = 1f;

		private string[] _hieroglyphsLocales = new string[4] { "ja", "ko", "zh", "zh-Hant" };

		private float _monospaceScale;

		private IDataModelService _dataModelService;

		private bool _isAnimating;

		private WatcherManager _watcherManager;

		public override IUpdateable[] Updateables => new IUpdateable[1] { this };

		public bool HasUsedToday => _saveData.DaysWhenRadioHasUsed.Contains(_dayNightController.Day);

		public event Action<int> RadioWaveFound;

		public override void Init()
		{
			base.Init();
			if (_radioKnobController != null)
			{
				_radioKnobController.ValueChanged -= OnKnobValueChanged;
				_radioKnobController.ValueChanged += OnKnobValueChanged;
				_radioKnobController.PointerPressedStateChanged -= OnKnobPointerPressedStateChanged;
				_radioKnobController.PointerPressedStateChanged += OnKnobPointerPressedStateChanged;
				_radioKnobController.InitModules(_inputHandler);
			}
			if (_radioButtonsController != null)
			{
				_radioButtonsController.Init(ERadioState.AM);
				_radioButtonsController.Pressed -= OnRadioButtonPressed;
				_radioButtonsController.Pressed += OnRadioButtonPressed;
				_radioButtonsController.InitModules(_inputHandler);
			}
			_tutorial.Init(_inputHandler);
			HoldProgressChanged -= OnHoldProgressChanged;
			HoldProgressChanged += OnHoldProgressChanged;
			TutorSwitched -= SwitchTutorActiveState;
			TutorSwitched += SwitchTutorActiveState;
			TutorLeft -= LeaveTutor;
			TutorLeft += LeaveTutor;
			IsHoldToClose = true;
			HasTutor = true;
			InitRadioModel();
		}

		private void OnKnobPointerPressedStateChanged(bool isClicked)
		{
			_radioButtonsController.SetOutlineActiveState(!isClicked);
		}

		private void OnHoldProgressChanged(float progress)
		{
			_radioContent.anchoredPosition = Vector3.Lerp(_radioStartPos, _radioHidePos, progress.Ease(Ease.InSine));
		}

		public void InitModules(IOtherGameSODataProvider otherGameSoDataProvider, ICustomYarnReaderProvider customYarnReaderProvider, CloseUpSaveData saveData, IDayNightController dayNightController, IDialogManager dialogManager, IInputHandlerProvider inputHandlerProvider, IDataModelService dataModelService, WatcherManager watcherManager)
		{
			_otherGameSoDataProvider = otherGameSoDataProvider;
			_customYarnReader = customYarnReaderProvider.CustomYarnReader;
			_saveData = saveData;
			_dayNightController = dayNightController;
			_dialogManager = dialogManager;
			_saveData = saveData;
			_dataModelService = dataModelService;
			_inputHandler = inputHandlerProvider.InputHandler;
			_watcherManager = watcherManager;
		}

		private void OnKnobValueChanged(float position)
		{
			_radioModel.CurrentPosition = position;
			_messageBox.text = ((LocalizationSettings.SelectedLocale.Identifier.Code != "ar") ? _radioModel.GetDisplayedMessage().ToMonospace(_monospaceScale) : _radioModel.GetDisplayedMessage());
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DSFXSourceExtra, 1f - _radioModel.NormalisedDistance, 0.2f);
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DSFXSourceExtra2, _radioModel.NormalisedDistance, 0.2f);
		}

		private void OnRadioButtonPressed(ERadioState state)
		{
			_radioModel.SwitchState(state);
			_radioButtonsController.UpdateButtonStates(state);
			_messageBox.text = ((LocalizationSettings.SelectedLocale.Identifier.Code != "ar") ? _radioModel.GetDisplayedMessage().ToMonospace(_monospaceScale) : _radioModel.GetDisplayedMessage());
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DSFXSourceExtra, 1f - _radioModel.NormalisedDistance, 0.2f);
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DSFXSourceExtra2, _radioModel.NormalisedDistance, 0.2f);
		}

		private void OnEnable()
		{
			_radioStartPos = _radioContent.anchoredPosition;
			_radioHidePos = _radioContent.anchoredPosition + Vector2.down * _radioContent.sizeDelta.y * 2f;
		}

		private void OnDestroy()
		{
			if (_radioKnobController != null)
			{
				_radioKnobController.ValueChanged -= OnKnobValueChanged;
			}
			if (_radioButtonsController != null)
			{
				_radioButtonsController.Pressed -= OnRadioButtonPressed;
			}
		}

		private void InitRadioModel()
		{
			_radioModel = new RadioModel(_otherGameSoDataProvider.OtherGameData.RadioDelta, _otherGameSoDataProvider.OtherGameData.RadioThresholdStart, _otherGameSoDataProvider.OtherGameData.WrongStateMultiplier, _customYarnReader, _saveData, _inputHandler);
			_radioModel.MessageFinished -= OnMessageFinished;
			_radioModel.MessageFinished += OnMessageFinished;
			_radioModel.MessageLineFinished -= OnMessageLineFinished;
			_radioModel.MessageLineFinished += OnMessageLineFinished;
			_radioModel.TimeLeftChanged -= OnTimeLeftChanged;
			_radioModel.TimeLeftChanged += OnTimeLeftChanged;
		}

		private void OnTimeLeftChanged(float value)
		{
			_fill.fillAmount = 1f - value;
			_fill.color = Color.Lerp(Color.black, Color.white, 1f - value);
		}

		private void OnMessageFinished(bool isWaveFound)
		{
			if (isWaveFound)
			{
				RadioWaveFound?.Invoke(_dayNightController.Day);
				_dataModelService.MetaPrefsDataHandler.DecryptRadio(_dayNightController.Day);
			}
			_messageBox.text = "";
			Hide().Forget();
		}

		private void OnMessageLineFinished()
		{
			_messageBox.text = ((LocalizationSettings.SelectedLocale.Identifier.Code != "ar") ? _radioModel.GetDisplayedMessage().ToMonospace(_monospaceScale) : _radioModel.GetDisplayedMessage());
		}

		public override async UniTask Show()
		{
			if (_isAnimating)
			{
				return;
			}
			_radioKnobController.SetActiveMode(b: true);
			_raycastTarget.SetLockedState(isLocked: true);
			int day = _dayNightController.Day;
			if (_saveData.DaysWhenRadioHasUsed.Contains(day))
			{
				_dialogManager.ShowSubtitlePopup(EInfoMessageType.NoMoreRadioToday);
				UnlockRaycast(3f).Forget();
				return;
			}
			_isAnimating = true;
			SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Radio_pickup);
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceDay, 0.5f, 1f);
			SoundService.Play(ESoundSource.s1_GameScene_2DSFXSourceExtra, ESound.Radio_noise_loop, new SoundSettingCycling());
			SoundService.Play(ESoundSource.s1_GameScene_2DSFXSourceExtra2, _voiceSounds.GetRandomElementWithoutRepeating(), new SoundSettingCycling());
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DSFXSourceExtra2, 0f, 0f);
			_bg.DOFade(0.25f, 0.5f).SetEase(Ease.InSine);
			_saveData.DaysWhenRadioHasUsed.Add(day);
			_radioModel.ResetDay();
			_radioModel.InitNode($"radio_{day}");
			_messageBox.text = ((LocalizationSettings.SelectedLocale.Identifier.Code != "ar") ? _radioModel.GetDisplayedMessage().ToMonospace(_monospaceScale) : _radioModel.GetDisplayedMessage());
			PlayerService.MakeImmovable();
			_watcherManager.EnterState(EWatcherState.Radio);
			HUDPresenter.SetupAndShowControlsView(EControlsList.InRadio);
			await base.Show();
			if (!_saveData.HasOpenedRadio)
			{
				_saveData.HasOpenedRadio = true;
				SwitchTutorActiveState();
			}
			_monospaceScale = (_hieroglyphsLocales.Contains(LocalizationSettings.SelectedLocale.Identifier.Code) ? 1f : 0.75f);
			await UniTask.DelayFrame(1);
			_isAnimating = false;
		}

		private async UniTaskVoid UnlockRaycast(float delay)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(delay));
			_raycastTarget.SetLockedState(isLocked: false);
		}

		public override async UniTask Hide()
		{
			if (!_isAnimating)
			{
				_isAnimating = true;
				SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Radio_put_down);
				SoundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceDay, 1f, 1f).Forget();
				SoundService.FadeOut(ESoundSource.s1_GameScene_2DSFXSourceExtra, 0.5f).Forget();
				SoundService.FadeOut(ESoundSource.s1_GameScene_2DSFXSourceExtra2, 0.5f).Forget();
				_bg.DOFade(0f, 0.5f).SetEase(Ease.OutSine);
				_radioModel.ForceStop();
				PlayerService.MakeMovable();
				_watcherManager.LeaveState(EWatcherState.Radio);
				HUDPresenter.HideControlsView();
				_raycastTarget.SetLockedState(isLocked: false);
				await base.Hide();
				_radioContent.anchoredPosition = _radioStartPos;
				await UniTask.DelayFrame(1);
				_isAnimating = false;
			}
		}

		private void SwitchTutorActiveState()
		{
			_isTutorActive.Switch();
			if (_isTutorActive)
			{
				_tutorial.Actualize();
				CanHold = false;
				_tutorial.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutQuad);
			}
			else
			{
				CanHold = true;
				_tutorial.transform.DOLocalMove(Vector3.down * 1080f, 0.5f).SetEase(Ease.OutQuad);
			}
			_radioKnobController.SetActiveMode(!_isTutorActive);
			_radioModel.SetPauseState(_isTutorActive);
		}

		private void LeaveTutor()
		{
			if (_isTutorActive)
			{
				_isTutorActive.Switch();
				CanHold = true;
				_tutorial.transform.DOLocalMove(Vector3.down * 1080f, 0.5f).SetEase(Ease.OutQuad);
				_radioKnobController.SetActiveMode(!_isTutorActive);
				_radioModel.SetPauseState(_isTutorActive);
			}
		}

		public void ReinitSaveData(CloseUpSaveData saveData)
		{
			_saveData = saveData;
		}

		private void Update()
		{
			if (_inputHandler.CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				if (_inputHandler.SpeedUpPerformedThisFrame)
				{
					_radioModel.SetSpeedUpStrength(1f);
				}
				if (_inputHandler.SpeedUpReleasedThisFrame)
				{
					_radioModel.SetSpeedUpStrength(0f);
				}
			}
			else
			{
				_radioModel.SetSpeedUpStrength(_inputHandler.SpeedUpValue);
			}
		}
	}
}
