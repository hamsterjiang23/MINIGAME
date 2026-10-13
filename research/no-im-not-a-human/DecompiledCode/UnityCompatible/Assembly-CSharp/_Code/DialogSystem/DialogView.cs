using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using _Code.Characters;
using _Code.Characters.DialogSystem;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Settings;
using _Code.Player;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.Sound.Service;

namespace _Code.DialogSystem
{
	public sealed class DialogView : MonoBehaviour
	{
		[SerializeField]
		private Canvas _parentCanvas;

		[SerializeField]
		private FakeDialogRunner _fakeDialogRunner;

		[SerializeField]
		private DialogOverlayData[] _dialogOverlays;

		[SerializeField]
		private AnimatedImage _character;

		[SerializeField]
		private AnimatedImage[] _backgroundCharacters;

		[SerializeField]
		private Image _overlay;

		[SerializeField]
		private GameObject _raycastBlocker;

		[SerializeField]
		private Transform _dialogPanelBase;

		[SerializeField]
		private Transform _dialogPanelAnswers;

		[SerializeField]
		private Camera _reserveCamera;

		private DialogOverlayData _currentOverlay;

		private Action _onDialogEndActions;

		private Action _onDialogStarted;

		private bool _isActive;

		private CharacterSOData _currentCharacter;

		private Vector2 _backgroundCharacterPositionRelativeToMainCharacter;

		private Action _temporaryDialogAction;

		private ICursorController _cursorController;

		private bool _isSubtitleActive;

		private EDialogEmotionState _currentEmotion;

		private const float USUAL_DIALOG_X_POSITION = 385f;

		public Camera LastUsedCamera => _parentCanvas.worldCamera;

		public event Action EnergyConsumed;

		public event Action DayDialogSelected;

		public event Action BaseDialogLineShowed;

		public event Action ButtonsDialogLineShowed;

		public event Action WereBadBoy;

		public void Init(Action onDialogStarted, Action onDialogEndActions, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, DialogSaveData saveData, ISettingsInstanceProvider settingsInstanceProvider, ICursorController cursorController)
		{
			_onDialogStarted = onDialogStarted;
			_onDialogEndActions = onDialogEndActions;
			_cursorController = cursorController;
			_character.gameObject.SetActive(value: false);
			_fakeDialogRunner.Init(soundService, inputHandlerProvider, saveData, settingsInstanceProvider, () => _isActive || _isSubtitleActive);
			_fakeDialogRunner.onDialogueComplete.AddListener(EndDialog);
			_currentOverlay = _dialogOverlays[0];
			_fakeDialogRunner.EnergyConsumed -= OnEnergyConsumed;
			_fakeDialogRunner.EnergyConsumed += OnEnergyConsumed;
			_fakeDialogRunner.DayDialogSelected -= OnDayDialogSelected;
			_fakeDialogRunner.DayDialogSelected += OnDayDialogSelected;
			_fakeDialogRunner.WereBadBoy -= OnWereBadBoy;
			_fakeDialogRunner.WereBadBoy += OnWereBadBoy;
			_fakeDialogRunner.BaseDialogLineShowed -= OnBaseDialogLineShowed;
			_fakeDialogRunner.BaseDialogLineShowed += OnBaseDialogLineShowed;
			_fakeDialogRunner.ButtonsDialogLineShowed -= OnButtonsDialogLineShowed;
			_fakeDialogRunner.ButtonsDialogLineShowed += OnButtonsDialogLineShowed;
		}

		private void OnButtonsDialogLineShowed()
		{
			ButtonsDialogLineShowed?.Invoke();
		}

		private void OnBaseDialogLineShowed()
		{
			BaseDialogLineShowed?.Invoke();
		}

		private void OnDayDialogSelected()
		{
			DayDialogSelected?.Invoke();
		}

		private void OnWereBadBoy()
		{
			WereBadBoy?.Invoke();
		}

		private void OnEnergyConsumed()
		{
			EnergyConsumed?.Invoke();
		}

		private void EndDialog()
		{
			_onDialogEndActions?.Invoke();
			_character.gameObject.SetActive(value: false);
			DisableCrtShader();
			DialogOverlayData currentOverlay = _currentOverlay;
			if (currentOverlay != null && !currentOverlay.ShowEvenIfDialogEnded)
			{
				_overlay.gameObject.SetActive(value: false);
			}
			if (_temporaryDialogAction != null)
			{
				_temporaryDialogAction?.Invoke();
				_temporaryDialogAction = null;
			}
			AnimatedImage[] backgroundCharacters = _backgroundCharacters;
			for (int i = 0; i < backgroundCharacters.Length; i++)
			{
				backgroundCharacters[i].gameObject.SetActive(value: false);
			}
			SetDialogStateDelayed(state: false).Forget();
			SetSubtitleStateDelayed(state: false).Forget();
			_raycastBlocker.SetActive(value: false);
		}

		private async UniTask SetSubtitleStateDelayed(bool state)
		{
			await UniTask.DelayFrame(1);
			_isSubtitleActive = state;
		}

		private async UniTask SetDialogStateDelayed(bool state)
		{
			await UniTask.DelayFrame(1);
			_isActive = state;
		}

		public void SkipLine()
		{
			_fakeDialogRunner.SkipLine();
		}

		public void StartDialog(CharacterSOData character, string nodeName, EDialogOverlayType overlayType = EDialogOverlayType.None, Camera cam = null, DialogViewData viewData = null, bool hideCharacter = false)
		{
			if (viewData == null)
			{
				viewData = new DialogViewData(385f);
			}
			if (cam == null)
			{
				cam = _reserveCamera;
			}
			_parentCanvas.worldCamera = cam;
			if (character.CharacterType == ECharacterType.TV)
			{
				EnableCrtShader();
			}
			_onDialogStarted?.Invoke();
			_fakeDialogRunner.SetDialogXPos(viewData.XPosition);
			_dialogPanelAnswers.position = _dialogPanelBase.position;
			_currentCharacter = character;
			_character.gameObject.SetActive(!hideCharacter);
			_character.SetData(character.GetEmotion(EDialogEmotionState.Base));
			DialogTransformData dialogTransformData = (character.EmotionOverrides.ContainsKey(EDialogEmotionState.Base) ? character.EmotionOverrides[EDialogEmotionState.Base] : new DialogTransformData());
			_character.StartAnimationOrSetFirstFrame();
			_character.transform.localPosition = character.DialogPosition + (Vector2)dialogTransformData.Position;
			_character.transform.localScale = Vector3.one * character.DialogScale + dialogTransformData.Scale;
			_fakeDialogRunner.StartDialogue(nodeName);
			_fakeDialogRunner.UpdateButtonsActivityDelayed().Forget();
			_overlay.gameObject.SetActive(overlayType != EDialogOverlayType.None);
			SetupOverlay(overlayType);
			_raycastBlocker.SetActive(value: true);
			SetDialogStateDelayed(state: true).Forget();
		}

		private void SetupOverlay(EDialogOverlayType overlayType)
		{
			if (overlayType != EDialogOverlayType.None)
			{
				_overlay.gameObject.SetActive(value: true);
				DialogOverlayData dialogOverlayData = _dialogOverlays.First((DialogOverlayData x) => x.OverlayType == overlayType);
				_overlay.sprite = dialogOverlayData.Sprite;
				_overlay.color = dialogOverlayData.Color;
				if (dialogOverlayData.ShowBehindTextsContent)
				{
					_fakeDialogRunner.MoveDialogContentsToTopLayer();
				}
				else
				{
					_overlay.transform.SetAsLastSibling();
				}
				_currentOverlay = dialogOverlayData;
			}
			else
			{
				_overlay.gameObject.SetActive(value: false);
			}
		}

		public void SetCharacterEmotion(EDialogEmotionState emotionState)
		{
			_currentEmotion = emotionState;
			_character.SetData(_currentCharacter.GetEmotion(emotionState));
			_character.StartAnimationOrSetFirstFrame();
			DialogTransformData dialogTransformData = (_currentCharacter.EmotionOverrides.ContainsKey(emotionState) ? _currentCharacter.EmotionOverrides[emotionState] : new DialogTransformData());
			_character.transform.localPosition = _currentCharacter.DialogPosition + (Vector2)dialogTransformData.Position;
			_character.transform.localScale = Vector3.one * _currentCharacter.DialogScale + dialogTransformData.Scale;
		}

		public void ShowSign(CharacterSOData character, ECharacterSign sign)
		{
			_fakeDialogRunner.ShowSign(character, sign);
		}

		public void StopShowingSign()
		{
			_fakeDialogRunner.StopShowingSign();
		}

		public void ShowSubtitle(string dialogName, Camera linkedCamera, EDialogOverlayType overlay)
		{
			SetSubtitleStateDelayed(state: true).Forget();
			_parentCanvas.worldCamera = linkedCamera;
			_fakeDialogRunner.ShowSubtitle();
			_fakeDialogRunner.StartDialogue(dialogName);
			_fakeDialogRunner.UpdateButtonsActivityDelayed().Forget();
			SetupOverlay(overlay);
		}

		public void ShowSubtitlePopup(EInfoMessageType messageType, Camera camera, EDialogOverlayType overlayType)
		{
			_parentCanvas.worldCamera = camera;
			_fakeDialogRunner.ShowSubtitlePopup(messageType);
			SetupOverlay(overlayType);
		}

		public void HideSubtitle()
		{
			_fakeDialogRunner.HideSubtitle();
		}

		public void AddActionForNextDialogEnded(Action temporaryDialogAction)
		{
			_temporaryDialogAction = temporaryDialogAction;
		}

		public void SetBackgroundCharacters(EDialogEmotionState emotion = EDialogEmotionState.Base, params CharacterSOData[] exiledCharacters)
		{
			for (int i = 0; i < _backgroundCharacters.Length; i++)
			{
				if (i < exiledCharacters.Length)
				{
					_backgroundCharacters[i].gameObject.SetActive(value: true);
					_backgroundCharacters[i].transform.localScale = Vector3.one * exiledCharacters[i].DialogScale;
					_backgroundCharacters[i].SetData(exiledCharacters[i].GetEmotion(emotion));
					_backgroundCharacters[i].StartAnimationOrSetFirstFrame();
				}
				else
				{
					_backgroundCharacters[i].gameObject.SetActive(value: false);
				}
			}
		}

		public void EnableCrtShader()
		{
			_character.Image.material.SetFloat("_IsEnabled", 1f);
		}

		public void DisableCrtShader()
		{
			_character.Image.material.SetFloat("_IsEnabled", 0f);
		}

		public bool IsNodeVisited(string nodeName)
		{
			return _fakeDialogRunner.IsNodeVisited(nodeName);
		}

		public void OnLoad(DialogSaveData saveData)
		{
			_fakeDialogRunner.OnLoad(saveData);
		}
	}
}
