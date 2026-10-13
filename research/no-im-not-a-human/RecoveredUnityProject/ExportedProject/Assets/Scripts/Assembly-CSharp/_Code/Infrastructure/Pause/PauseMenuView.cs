using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Steamworks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Localization;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Scenes;
using _Code.Infrastructure.Settings;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.Extensions;
using _Code.Utils.UI.Popup;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Pause
{
	public sealed class PauseMenuView : MonoBehaviour
	{
		[SerializeField]
		private EventSystem _eventSystem;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private Material _distortMaterial;

		[SerializeField]
		private GameObject _pauseContent;

		[SerializeField]
		private SettingsInstance _settingsContent;

		[SerializeField]
		private Button _continueButton;

		[SerializeField]
		private Button _settingsButton;

		[SerializeField]
		private Button _mainMenuButton;

		[SerializeField]
		private Button _exitButton;

		[SerializeField]
		private Button _closeSettingsButton;

		[SerializeField]
		private PopupWindow _popupWindow;

		[SerializeField]
		private CanvasGroup _pauseFade;

		private IPlayerService _playerService;

		private ICursorController _cursorController;

		private ILocalizationManager _localizationManager;

		private IHUDPresenter _hudPresenter;

		private InputHandling _inputHandler;

		private IDataModelService _dataModelService;

		private IDayNightController _dayNightController;

		private ILocationsManager _locationsManager;

		private WatcherManager _watcherManager;

		private static readonly int Show1 = Animator.StringToHash("Show");

		private static readonly int Hide1 = Animator.StringToHash("Hide");

		public bool IsChanging { get; private set; }

		public bool IsPaused { get; private set; }

		public bool IsSaveAnQuitting { get; private set; }

		public event Action Switched;

		public void Init(IPlayerService playerService, ICursorController cursorController, ILocalizationManager localizationManager, IInputHandlerProvider inputHandlerProvider, IHUDPresenter hudPresenter, IDataModelService dataModelService, IDayNightController dayNightController, ILocationsManager locationsManager, WatcherManager watcherManager)
		{
			_playerService = playerService;
			_cursorController = cursorController;
			_localizationManager = localizationManager;
			_inputHandler = inputHandlerProvider.InputHandler;
			_hudPresenter = hudPresenter;
			_dataModelService = dataModelService;
			_dayNightController = dayNightController;
			_locationsManager = locationsManager;
			_watcherManager = watcherManager;
			_continueButton.onClick.RemoveAllListeners();
			_settingsButton.onClick.RemoveAllListeners();
			_mainMenuButton.onClick.RemoveAllListeners();
			_exitButton.onClick.RemoveAllListeners();
			_closeSettingsButton.onClick.RemoveAllListeners();
			_continueButton.onClick.AddListener(SwitchState);
			_settingsButton.onClick.AddListener(OpenSettings);
			_mainMenuButton.onClick.AddListener(GoToMenu);
			_exitButton.onClick.AddListener(ExitGame);
			_closeSettingsButton.onClick.AddListener(CloseSettings);
			_popupWindow.InitModules(_inputHandler, _cursorController, watcherManager);
		}

		private void OpenSettings()
		{
			_pauseContent.SetActive(value: false);
			_settingsContent.Show();
			_watcherManager.EnterState(EWatcherState.Settings);
		}

		private void CloseSettings()
		{
			_pauseContent.SetActive(value: true);
			_settingsContent.gameObject.SetActive(value: false);
			_settingsContent.SaveSettings();
			_watcherManager.LeaveState(EWatcherState.Settings);
			if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
			{
				_eventSystem.SetSelectedGameObject(_continueButton.gameObject);
			}
		}

		private async UniTaskVoid Show()
		{
			IsChanging = true;
			Time.timeScale = 0f;
			_inputHandler.SetIsInUIState(isInUi: true);
			_watcherManager.EnterState(EWatcherState.Pause);
			gameObject.SetActive(value: true);
			_playerService.MakeImmovable();
			await (Tween)_canvasGroup.DOFade(1f, 0.2f).SetEase(Ease.InSine).SetUpdate(isIndependentUpdate: true);
			IsPaused = true;
			IsChanging = false;
			_eventSystem.SetSelectedGameObject((_inputHandler.CurrentDevice == EInputDevice.Gaypad) ? _continueButton.gameObject : null);
			_cursorController.SetThrough(isThrough: true);
		}

		private async UniTaskVoid Hide()
		{
			_cursorController.SetThrough(isThrough: false);
			IsChanging = true;
			_inputHandler.SetIsInUIState(isInUi: false);
			if (!_playerService.IsInRoom)
			{
				_playerService.MakeMovable();
			}
			await (Tween)_canvasGroup.DOFade(0f, 0.2f).SetEase(Ease.OutSine).SetUpdate(isIndependentUpdate: true);
			OnHiddenAnimationEvent();
			Time.timeScale = 1f;
			IsChanging = false;
			if (_settingsContent.gameObject.activeSelf)
			{
				CloseSettings();
			}
			_watcherManager.LeaveState(EWatcherState.Pause);
		}

		private void Update()
		{
			if (_inputHandler.UICancelClicked && _settingsContent.gameObject.activeSelf)
			{
				CloseSettings();
			}
		}

		public void OnHiddenAnimationEvent()
		{
			gameObject.SetActive(value: false);
			IsPaused = false;
		}

		public void Switch(bool state)
		{
			if (_popupWindow.gameObject.activeSelf)
			{
				_popupWindow.Hide();
			}
			if (state)
			{
				Show().Forget();
			}
			else
			{
				Hide().Forget();
			}
		}

		private void SwitchState()
		{
			if (!IsChanging)
			{
				Switched?.Invoke();
			}
		}

		public void GoToMenu()
		{
			_popupWindow.Show("common_sureToClose".GetLocalization("UI"), new PopupButtonData("common_no", () =>
			{
				_popupWindow.Hide();
				if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
				{
					_eventSystem.SetSelectedGameObject(_mainMenuButton.gameObject);
				}
			}), new PopupButtonData("common_yes", () =>
			{
				GoToMenuAsync().Forget();
			}));
		}

		public void ExitGame()
		{
			_popupWindow.Show("common_sureToCloseSave".GetLocalization("UI"), new PopupButtonData("common_no", () =>
			{
				_popupWindow.Hide();
				if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
				{
					_eventSystem.SetSelectedGameObject(_exitButton.gameObject);
				}
			}), new PopupButtonData("common_yes", () =>
			{
				ExitGameAsync().Forget();
			}));
		}

		private async UniTaskVoid ExitGameAsync()
		{
			Time.timeScale = 1f;
			if (_dayNightController.Day < 14)
			{
				IsSaveAnQuitting = true;
				_pauseFade.gameObject.SetActive(value: true);
				await _dataModelService.GameSaveDataHandler.SaveAllReserve();
				_pauseFade.DOFade(1f, 0.66f);
				await _hudPresenter.ShowGameSavedOnClose();
			}
			SteamAPI.Shutdown();
			Application.Quit();
		}

		private async UniTaskVoid GoToMenuAsync()
		{
			_dataModelService.GameSaveDataHandler.ResetEvents();
			Time.timeScale = 1f;
			ScenesChanger.OpenScene(0).Forget();
			_watcherManager.LeaveState(EWatcherState.Popup);
			_watcherManager.LeaveState(EWatcherState.Pause);
			_watcherManager.LeaveState(EWatcherState.World3d);
		}
	}
}
