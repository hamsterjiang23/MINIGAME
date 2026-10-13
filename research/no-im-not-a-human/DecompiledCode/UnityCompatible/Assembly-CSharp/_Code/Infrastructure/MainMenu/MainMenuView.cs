using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Steamworks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure.Scenes;
using _Code.Infrastructure.Settings;
using _Code.Infrastructure._NINAH__MainMenu.Gacha;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuView : MonoBehaviour
	{
		[SerializeField]
		private EventSystem _eventSystem;

		[SerializeField]
		private GameObject _newGameButtonGO;

		[SerializeField]
		private GameObject _cloudLoadingGO;

		[SerializeField]
		private MainMenuSignLine[] _signLines;

		[SerializeField]
		private Image _fade;

		[SerializeField]
		private AudioMixer _audioMixer;

		[SerializeField]
		private MainMenuTitleView _title;

		[SerializeField]
		private MainMenuFaceController _faceController;

		[SerializeField]
		private SettingsInstance _settings;

		[SerializeField]
		private GachaWindow _gachaWindow;

		[SerializeField]
		private Button _newGameButton;

		[SerializeField]
		private Button _continueGameButton;

		[SerializeField]
		private Button _settingsButton;

		[SerializeField]
		private Button _collectionButton;

		[SerializeField]
		private Button _quitButton;

		[SerializeField]
		private Button _crButton;

		[SerializeField]
		private Button _trioskazButton;

		private Button[] _buttons;

		private Vector3 _backgroundStartPos;

		private Vector3 _notBackgroundStartPos;

		private bool _areSettingsOpened;

		private bool _areSettingsAnimating;

		private IDataModelService _dataModelService;

		private InputHandling _inputHandler;

		private ICursorController _cursorController;

		private INotAHumanSoundService _soundService;

		private IEndingDataProvider _endingDataProvider;

		private bool _isAlreadyStarted;

		private WatcherManager _watcherManager;

		public void Init(IDataModelService dataModelService, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController, IEndingDataProvider endingDataProvider, INotAHumanSoundService soundService, ICustomYarnReaderProvider customYarnReaderProvider, ICharactersSODataProvider charactersSoDataProvider, WatcherManager watcherManager)
		{
			_dataModelService = dataModelService;
			_inputHandler = inputHandlerProvider.InputHandler;
			_cursorController = cursorController;
			_soundService = soundService;
			_endingDataProvider = endingDataProvider;
			_watcherManager = watcherManager;
			MainMenuSignLine[] signLines = _signLines;
			for (int i = 0; i < signLines.Length; i++)
			{
				signLines[i].Init();
			}
			_signLines[0].Moved -= OnMovedEyes;
			_signLines[0].Moved += OnMovedEyes;
			_signLines[1].Moved -= OnMovedTeeth;
			_signLines[1].Moved += OnMovedTeeth;
			_gachaWindow.InitEndings(endingDataProvider, dataModelService, cursorController, inputHandlerProvider, soundService, customYarnReaderProvider, charactersSoDataProvider, watcherManager).Forget();
			_inputHandler.InputDeviceChanged -= OnInputDeviceChanged;
			_inputHandler.InputDeviceChanged += OnInputDeviceChanged;
			_cursorController.SetThrough(isThrough: true);
			_dataModelService.CloudSaveLoadingChanged -= OnCloudLoadingChanged;
			_dataModelService.CloudSaveLoadingChanged += OnCloudLoadingChanged;
			_dataModelService.CloudSaveLoadFailed -= OnCloudLoadingFailed;
			_dataModelService.CloudSaveLoadFailed += OnCloudLoadingFailed;
			_buttons = new Button[7] { _newGameButton, _continueGameButton, _settingsButton, _collectionButton, _quitButton, _crButton, _trioskazButton };
			InitAsync().Forget();
			_inputHandler.Init(_cursorController);
			_watcherManager.EnterState(EWatcherState.MainMenu);
		}

		private void OnCloudLoadingFailed(string error)
		{
			_cloudLoadingGO.SetActive(value: false);
			Debug.Log(error);
		}

		private void OnCloudLoadingChanged(bool isLoading)
		{
			_cloudLoadingGO.SetActive(isLoading);
		}

		private async UniTaskVoid InitAsync()
		{
			SetButtonsInteractable(isInteractable: false);
			await _dataModelService.WaitForInitialization();
			SetButtonsInteractable(isInteractable: true);
			_continueGameButton.interactable = _dataModelService.GameSaveDataHandler.HasSaveData;
		}

		private void SetButtonsInteractable(bool isInteractable)
		{
			Button[] buttons = _buttons;
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].interactable = isInteractable;
			}
		}

		private void OnInputDeviceChanged(EInputDevice device)
		{
			_watcherManager.ChangeInput(device);
		}

		private void OnMovedEyes(Sprite sprite)
		{
			_title.Change();
			_faceController.SetEyes(sprite);
		}

		private void OnMovedTeeth(Sprite sprite)
		{
			_faceController.SetTeeth(sprite);
		}

		public void StartNewGame()
		{
			if (!_isAlreadyStarted && _dataModelService.IsInitialized)
			{
				_isAlreadyStarted = true;
				StartNewGameAsync().Forget();
			}
		}

		private async UniTask StartNewGameAsync()
		{
			Button[] buttons = _buttons;
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].interactable = false;
			}
			_gachaWindow.CartoonEnded += OnIntroCartoonEnded;
			await (Tween)_fade.DOFade(1f, 1f).SetEase(Ease.InSine);
			await UniTask.Delay(TimeSpan.FromSeconds(0.5));
			_gachaWindow.ShowEnding(_endingDataProvider.EndingsData.FirstOrDefault((EndingViewSOData x) => x.EndingType == EEnding.Intro));
			_dataModelService.MetaPrefsDataHandler.ReachEnding(EEnding.Intro);
		}

		private void OnIntroCartoonEnded()
		{
			_gachaWindow.CartoonEnded -= OnIntroCartoonEnded;
			_dataModelService.GameSaveDataHandler.UnprepareToLoadGame();
			StartGameAsync(isSkipFade: true).Forget();
		}

		public void LoadGame()
		{
			if (!_isAlreadyStarted && _dataModelService.IsInitialized && _dataModelService.GameSaveDataHandler.HasSaveData)
			{
				_isAlreadyStarted = true;
				Button[] buttons = _buttons;
				for (int i = 0; i < buttons.Length; i++)
				{
					buttons[i].enabled = false;
				}
				_dataModelService.GameSaveDataHandler.PrepareToLoadGame();
				StartGameAsync().Forget();
			}
		}

		public void CloseGame()
		{
			SteamAPI.Shutdown();
			Application.Quit();
		}

		private async UniTaskVoid StartGameAsync(bool isSkipFade = false)
		{
			_signLines[0].Stop();
			_signLines[1].Stop();
			if (!isSkipFade)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(0.5));
				_fade.DOFade(1f, 1f);
				await UniTask.Delay(TimeSpan.FromSeconds(1.0));
			}
			else
			{
				_fade.color = Color.black;
			}
			_watcherManager.LeaveState(EWatcherState.MainMenu);
			ScenesChanger.OpenScene(1).Forget();
		}

		public void ShowSettings()
		{
			_settings.Show();
			_watcherManager.EnterState(EWatcherState.Settings);
		}

		public void CloseSettings()
		{
			_settings.gameObject.SetActive(value: false);
			_watcherManager.LeaveState(EWatcherState.Settings);
		}

		public void ShowGacha()
		{
			_gachaWindow.ReinitLocalization();
			_gachaWindow.gameObject.SetActive(value: true);
			Button[] buttons = _buttons;
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].gameObject.SetActive(value: false);
			}
		}

		public void CloseGacha()
		{
			Button[] buttons = _buttons;
			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i].gameObject.SetActive(value: true);
			}
			_gachaWindow.gameObject.SetActive(value: false);
			_watcherManager.LeaveState(EWatcherState.CollectionEndings);
			_watcherManager.LeaveState(EWatcherState.CollectionCharacters);
			if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
			{
				_eventSystem.SetSelectedGameObject(_collectionButton.gameObject);
			}
		}

		public void ShowTitles()
		{
			ScenesChanger.OpenScene(5).Forget();
		}

		public void VisitTrioskaz()
		{
			if (_inputHandler.CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				_eventSystem.SetSelectedGameObject(null);
			}
			string text = "https://store.steampowered.com/curator/39433050";
			if (SteamManager.Initialized)
			{
				SteamFriends.ActivateGameOverlayToWebPage(text);
			}
			else
			{
				Application.OpenURL(text);
			}
		}

		public void VisitCR()
		{
			if (_inputHandler.CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				_eventSystem.SetSelectedGameObject(null);
			}
			string text = "https://discord.gg/Dpx4FfVKbE";
			if (SteamManager.Initialized)
			{
				SteamFriends.ActivateGameOverlayToWebPage(text);
			}
			else
			{
				Application.OpenURL(text);
			}
		}

		private async void Update()
		{
			if (_inputHandler.UICancelClicked && _settings.gameObject.activeSelf)
			{
				CloseSettings();
			}
		}
	}
}
