using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;
using _Code.Characters;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure._NINAH__Endings.View;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Code.Utils.UI;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure._NINAH__MainMenu.Gacha
{
	public sealed class GachaWindow : MonoBehaviour
	{
		[SerializeField]
		private GameObject _endingsPanel;

		[SerializeField]
		private GameObject _charactersPanel;

		[SerializeField]
		private GachaEndingView[] _gachaEndingViews;

		[SerializeField]
		private GachaCharacterView[] _gachaCharacterViews;

		[SerializeField]
		private EndingView _cartoonWatcher;

		[SerializeField]
		private Image _characterSprite;

		[SerializeField]
		private TMP_Text _characterQuote;

		[SerializeField]
		private LocalizedString _baseQuote;

		[SerializeField]
		private Button _nextGachaButton;

		[SerializeField]
		private Image _currentGachaImage;

		[SerializeField]
		private Image _nextGachaImage;

		[SerializeField]
		private Sprite _endingsGachaSprite;

		[SerializeField]
		private Sprite _charactersGachaSprite;

		[SerializeField]
		private Sprite _endingsGachaButtonSprite;

		[SerializeField]
		private Sprite _charactersGachaButtonSprite;

		[SerializeField]
		private Image _progresFill;

		[SerializeField]
		private RTLTextMeshPro _progressText;

		[SerializeField]
		private GameObject _firstEndingGO;

		[SerializeField]
		private GameObject _firstCharacterGO;

		[SerializeField]
		private Button _backButton;

		[SerializeField]
		private Scrollbar _scrollbarEndings;

		[SerializeField]
		private Scrollbar _scrollbarCharacters;

		[SerializeField]
		private ScrollRect _scrollRectEndings;

		[SerializeField]
		private ScrollRect _scrollRectCharacters;

		[SerializeField]
		private GamepadTypeControlView _backGamepadHint;

		[SerializeField]
		private GamepadTypeControlView _switchGamepadHint;

		private UISelectable[] _selectablesEndings;

		private UISelectable[] _selectablesCharacters;

		private EndingViewSOData[] _endingsData;

		private CharacterSOData[] _charactersData;

		private INotAHumanSoundService _soundService;

		private ICursorController _cursorController;

		private InputHandling _inputHandler;

		private bool _isCurrentGachaIsEndings = true;

		private CharacterSOData _selectedCharacter;

		private IDataModelService _dataModelService;

		private int _endingsTotalCount;

		private int _endingsCurrentCount;

		private int _charactersTotalCount;

		private int _charactersCurrentCount;

		private ECharacterType[] _ignoreCharacters = new ECharacterType[6]
		{
			ECharacterType.Empty,
			ECharacterType.Player,
			ECharacterType.TV,
			ECharacterType.Twins,
			ECharacterType.Phone,
			ECharacterType.Miner
		};

		private EEnding[] _ignoreEndings = new EEnding[2]
		{
			EEnding.Intro,
			EEnding.TestEnding
		};

		private WatcherManager _watcherManager;

		public event Action CartoonEnded;

		public async UniTaskVoid InitEndings(IEndingDataProvider endingDataProvider, IDataModelService dataModelService, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, INotAHumanSoundService soundService, ICustomYarnReaderProvider customYarnReaderProvider, ICharactersSODataProvider charactersSODataProvider, WatcherManager watcherManager)
		{
			_endingsData = endingDataProvider.EndingsData;
			_charactersData = charactersSODataProvider.CharactersData;
			_soundService = soundService;
			_cursorController = cursorController;
			_inputHandler = inputHandlerProvider.InputHandler;
			_dataModelService = dataModelService;
			_watcherManager = watcherManager;
			_nextGachaButton.onClick.RemoveAllListeners();
			_nextGachaButton.onClick.AddListener(NextGacha);
			await dataModelService.WaitForInitialization();
			List<EEnding> unlockedEndings = dataModelService.MetaPrefsDataHandler.GetUnlockedEndings();
			List<ECharacterType> unlockedCharacters = dataModelService.MetaPrefsDataHandler.GetUnlockedCharacters();
			for (int i = 0; i < _endingsData.Length; i++)
			{
				if (_gachaEndingViews.Length > i && !_ignoreEndings.Contains(_endingsData[i].EndingType))
				{
					_gachaEndingViews[i].Init(_endingsData[i], unlockedEndings.Contains(_endingsData[i].EndingType), i);
					_gachaEndingViews[i].WatchMovieClicked -= ShowEnding;
					_gachaEndingViews[i].WatchMovieClicked += ShowEnding;
				}
			}
			ReinitCharacters(unlockedCharacters);
			_cartoonWatcher.Ended -= OnCartoonEnded;
			_cartoonWatcher.Ended += OnCartoonEnded;
			_cartoonWatcher.Init(soundService, customYarnReaderProvider.CustomYarnReader, inputHandlerProvider.InputHandler, _dataModelService, watcherManager);
			_endingsCurrentCount = unlockedEndings.Count((EEnding x) => !_ignoreEndings.Contains(x));
			_endingsTotalCount = _endingsData.Length - _ignoreEndings.Length + 1;
			_charactersCurrentCount = unlockedCharacters.Count((ECharacterType x) => !_ignoreCharacters.Contains(x));
			_charactersTotalCount = _charactersData.Count((CharacterSOData x) => !_ignoreCharacters.Contains(x.CharacterType));
			if (_isCurrentGachaIsEndings)
			{
				UpdateCountForEndings();
			}
			else
			{
				UpdateCountForCharacters();
			}
			_inputHandler.InputDeviceChanged -= OnInputChanged;
			_inputHandler.InputDeviceChanged += OnInputChanged;
		}

		private void OnInputChanged(EInputDevice device)
		{
			if (!gameObject.activeSelf)
			{
				return;
			}
			if (device == EInputDevice.MouseAndKeyboard)
			{
				EventSystem.current.SetSelectedGameObject(null);
				_backGamepadHint.gameObject.SetActive(value: false);
				_switchGamepadHint.gameObject.SetActive(value: false);
				return;
			}
			if (_isCurrentGachaIsEndings)
			{
				EventSystem.current.SetSelectedGameObject(_firstEndingGO);
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(_firstCharacterGO);
			}
			_backGamepadHint.gameObject.SetActive(value: true);
			_switchGamepadHint.gameObject.SetActive(value: true);
			_backGamepadHint.InitButton(_inputHandler.GetGaypadType());
			_switchGamepadHint.InitButton(_inputHandler.GetGaypadType());
		}

		private void OnEnable()
		{
			if (_inputHandler.CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				_backGamepadHint.gameObject.SetActive(value: false);
				_switchGamepadHint.gameObject.SetActive(value: false);
				return;
			}
			if (_isCurrentGachaIsEndings)
			{
				_watcherManager.EnterState(EWatcherState.CollectionEndings);
			}
			else
			{
				_watcherManager.EnterState(EWatcherState.CollectionCharacters);
			}
			_backGamepadHint.gameObject.SetActive(value: true);
			_switchGamepadHint.gameObject.SetActive(value: true);
			_backGamepadHint.InitButton(_inputHandler.GetGaypadType());
			_switchGamepadHint.InitButton(_inputHandler.GetGaypadType());
		}

		private void Start()
		{
			_selectablesEndings = _scrollRectEndings.GetComponentsInChildren<UISelectable>();
			_selectablesCharacters = _scrollRectCharacters.GetComponentsInChildren<UISelectable>();
			UISelectable[] selectablesCharacters = _selectablesCharacters;
			foreach (UISelectable obj in selectablesCharacters)
			{
				obj.Selected -= OnItemSelectedCharacters;
				obj.Selected += OnItemSelectedCharacters;
			}
			selectablesCharacters = _selectablesEndings;
			foreach (UISelectable obj2 in selectablesCharacters)
			{
				obj2.Selected -= OnItemSelectedEndings;
				obj2.Selected += OnItemSelectedEndings;
			}
		}

		private void OnItemSelectedEndings(BaseEventData eventData)
		{
			if (_inputHandler.CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				return;
			}
			RectTransform rectTransform = eventData.selectedObject.transform as RectTransform;
			if (!(_scrollRectEndings == null) && !(_scrollRectEndings.content == null) && !(_scrollRectEndings.viewport == null))
			{
				RectTransform content = _scrollRectEndings.content;
				RectTransform viewport = _scrollRectEndings.viewport;
				Vector3 position = rectTransform.TransformPoint(rectTransform.rect.center);
				float y = _scrollRectEndings.transform.InverseTransformPoint(position).y;
				float y2 = _scrollRectEndings.transform.InverseTransformPoint(viewport.position).y;
				float num = y - y2;
				float num2 = content.rect.height - viewport.rect.height;
				if (!Mathf.Approximately(num2, 0f))
				{
					float num3 = num / num2;
					_scrollRectEndings.verticalNormalizedPosition = Mathf.Clamp01(_scrollRectEndings.verticalNormalizedPosition + num3);
				}
			}
		}

		private void OnItemSelectedCharacters(BaseEventData eventData)
		{
			if (_inputHandler.CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				return;
			}
			RectTransform rectTransform = eventData.selectedObject.transform as RectTransform;
			if (!(_scrollRectCharacters == null) && !(_scrollRectCharacters.content == null) && !(_scrollRectCharacters.viewport == null))
			{
				RectTransform content = _scrollRectCharacters.content;
				RectTransform viewport = _scrollRectCharacters.viewport;
				Vector3 position = rectTransform.TransformPoint(rectTransform.rect.center);
				float y = _scrollRectCharacters.transform.InverseTransformPoint(position).y;
				float y2 = _scrollRectCharacters.transform.InverseTransformPoint(viewport.position).y;
				float num = y - y2;
				float num2 = content.rect.height - viewport.rect.height;
				if (!Mathf.Approximately(num2, 0f))
				{
					float num3 = num / num2;
					_scrollRectCharacters.verticalNormalizedPosition = Mathf.Clamp01(_scrollRectCharacters.verticalNormalizedPosition + num3);
				}
			}
		}

		public void ReinitLocalization()
		{
			List<EEnding> unlockedEndings = _dataModelService.MetaPrefsDataHandler.GetUnlockedEndings();
			for (int i = 0; i < _endingsData.Length; i++)
			{
				if (_gachaEndingViews.Length > i && !_ignoreEndings.Contains(_endingsData[i].EndingType))
				{
					_gachaEndingViews[i].Init(_endingsData[i], unlockedEndings.Contains(_endingsData[i].EndingType), i);
					_gachaEndingViews[i].WatchMovieClicked -= ShowEnding;
					_gachaEndingViews[i].WatchMovieClicked += ShowEnding;
				}
			}
		}

		private void ReinitCharacters(List<ECharacterType> unlockedCharacters)
		{
			int num = 0;
			for (int i = 0; i < _charactersData.Length; i++)
			{
				if (_gachaCharacterViews.Length >= i - num)
				{
					if (_ignoreCharacters.Contains(_charactersData[i].CharacterType))
					{
						num++;
						continue;
					}
					_gachaCharacterViews[i - num].Init(_charactersData[i], unlockedCharacters.Contains(_charactersData[i].CharacterType), _inputHandler);
					_gachaCharacterViews[i - num].CharacterSelected -= OnCharacterSelected;
					_gachaCharacterViews[i - num].CharacterSelected += OnCharacterSelected;
				}
			}
		}

		private void NextGacha()
		{
			_isCurrentGachaIsEndings = !_isCurrentGachaIsEndings;
			if (_isCurrentGachaIsEndings)
			{
				_watcherManager.LeaveState(EWatcherState.CollectionCharacters);
				_watcherManager.EnterState(EWatcherState.CollectionEndings);
				_nextGachaImage.sprite = _charactersGachaButtonSprite;
				_currentGachaImage.sprite = _endingsGachaSprite;
				_endingsPanel.SetActive(value: true);
				_charactersPanel.SetActive(value: false);
				UpdateCountForEndings();
				if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
				{
					EventSystem.current.SetSelectedGameObject(_firstEndingGO);
				}
			}
			else
			{
				_watcherManager.LeaveState(EWatcherState.CollectionEndings);
				_watcherManager.EnterState(EWatcherState.CollectionCharacters);
				_nextGachaImage.sprite = _endingsGachaButtonSprite;
				_currentGachaImage.sprite = _charactersGachaSprite;
				_endingsPanel.SetActive(value: false);
				_charactersPanel.SetActive(value: true);
				UpdateCountForCharacters();
				if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
				{
					EventSystem.current.SetSelectedGameObject(_firstCharacterGO);
				}
			}
			_currentGachaImage.SetNativeSize();
			_nextGachaImage.SetNativeSize();
			if (_inputHandler.CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}

		private void UpdateCountForEndings()
		{
			_progresFill.fillAmount = (float)_endingsCurrentCount / (float)_endingsTotalCount;
			_progressText.text = _endingsCurrentCount + " / " + _endingsTotalCount;
		}

		private void UpdateCountForCharacters()
		{
			_progresFill.fillAmount = (float)_charactersCurrentCount / (float)_charactersTotalCount;
			_progressText.text = _charactersCurrentCount + " / " + _charactersTotalCount;
		}

		private void OnCartoonEnded()
		{
			if (_soundService != null)
			{
				_soundService.FadeOut(ESoundSource.s0_MainMenuScene_Cartoons, 1f);
				_soundService.FadeVolume(ESoundSource.s0_MainMenuScene_MusicSource, 1f, 1f);
				_soundService.Start(ESoundSource.s0_MainMenuScene_MusicSource);
				GachaEndingView[] gachaEndingViews = _gachaEndingViews;
				for (int i = 0; i < gachaEndingViews.Length; i++)
				{
					gachaEndingViews[i].gameObject.SetActive(value: true);
				}
				CartoonEnded?.Invoke();
			}
		}

		public void ShowEnding(EndingViewSOData ending)
		{
			_soundService.FadeOut(ESoundSource.s0_MainMenuScene_MusicSource, 1f);
			_cartoonWatcher.ShowEnding(ending);
			GachaEndingView[] gachaEndingViews = _gachaEndingViews;
			for (int i = 0; i < gachaEndingViews.Length; i++)
			{
				gachaEndingViews[i].gameObject.SetActive(value: false);
			}
		}

		private void OnCharacterSelected(CharacterSOData character)
		{
			_selectedCharacter = character;
			ResetCharacter(character).Forget();
		}

		private async UniTask ResetCharacter(CharacterSOData character)
		{
			_characterSprite.sprite = character.GetEmotion(EDialogEmotionState.Base).Frames[0];
			_characterSprite.SetNativeSize();
			RectTransform rectTransform = (RectTransform)_characterSprite.transform;
			rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, rectTransform.sizeDelta.y / 2f);
		}

		private void Update()
		{
			if (_inputHandler.UISwitchTab)
			{
				NextGacha();
			}
			if (_inputHandler.UICancelClicked)
			{
				_backButton.onClick.Invoke();
			}
			if (_inputHandler.UIScroll != Vector2.zero)
			{
				if (_isCurrentGachaIsEndings)
				{
					_scrollbarEndings.value += _inputHandler.UIScroll.y * Time.deltaTime;
				}
				else
				{
					_scrollbarCharacters.value += _inputHandler.UIScroll.y * Time.deltaTime;
				}
			}
		}
	}
}
