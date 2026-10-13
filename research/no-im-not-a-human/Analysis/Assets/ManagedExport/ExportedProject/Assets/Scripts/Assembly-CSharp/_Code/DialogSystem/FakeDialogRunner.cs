using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RTLTMPro;
using SABI;
using UnityEngine;
using UnityEngine.UI;
using Yarn;
using Yarn.Unity;
using _Code.Characters;
using _Code.Characters.DialogSystem;
using _Code.Infrastructure.Settings;
using _Code.Player;
using _Code.Player.Markers;
using _Scripts.Services.Sound.Service;

namespace _Code.DialogSystem
{
	public sealed class FakeDialogRunner : DialogueRunner
	{
		[SerializeField]
		private DialogSignsView _dialogSignsView;

		[SerializeField]
		private SubtitlesView _subtitlesView;

		[SerializeField]
		private Image _usualLineView;

		[SerializeField]
		private RTLTextMeshPro _usualLineViewText;

		[SerializeField]
		private RTLTextMeshPro _optionsLineViewText;

		[SerializeField]
		private Transform[] _lineViewContents;

		private const float POPUP_TIME = 2.5f;

		private bool _isSubtitleShown;

		private int _initedButtonsCount;

		private INotAHumanSoundService _soundService;

		private InputHandling _inputHandler;

		private SettingsInstance _settingsInstance;

		private HoverableButton[] _buttons;

		public Func<bool> _isDialogOrSubtitleShown;

		private LineView lineView => (LineView)dialogueViews[0];

		private OptionsListView optionsView => (OptionsListView)dialogueViews[1];

		public event Action EnergyConsumed;

		public event Action DayDialogSelected;

		public event Action WereBadBoy;

		public event Action BaseDialogLineShowed;

		public event Action ButtonsDialogLineShowed;

		public void Init(INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, DialogSaveData saveData, ISettingsInstanceProvider settingsInstanceProvider, Func<bool> isDialogOrSubtitleShown)
		{
			_inputHandler = inputHandlerProvider.InputHandler;
			_soundService = soundService;
			_settingsInstance = settingsInstanceProvider.SettingsInstance;
			((FakeVariableStorage)base.VariableStorage).Init(saveData);
			optionsView.ShowedOptions -= ReinitButtons;
			optionsView.ShowedOptions += ReinitButtons;
			optionsView.AnyOptionSelected -= UpdateButtonsActivity;
			optionsView.AnyOptionSelected += UpdateButtonsActivity;
			_inputHandler.InputDeviceChanged -= OnInputChanged;
			_inputHandler.InputDeviceChanged += OnInputChanged;
			_settingsInstance.Changed -= OnSettingsChanged;
			_settingsInstance.Changed += OnSettingsChanged;
			lineView.LineRunned -= OnLineViewTextAppeared;
			lineView.LineRunned += OnLineViewTextAppeared;
			_dialogSignsView.InitModules(soundService);
			OnSettingsChanged();
			_isDialogOrSubtitleShown = isDialogOrSubtitleShown;
		}

		private void OnInputChanged(EInputDevice device)
		{
			if (_isDialogOrSubtitleShown())
			{
				UpdateButtonsActivity();
			}
		}

		private void OnSettingsChanged()
		{
			lineView.useTypewriterEffect = _settingsInstance.TextSettingsData.IsUseTypewriter;
		}

		private void Update()
		{
			if (_inputHandler.UIDialogSkipClicked && _isDialogOrSubtitleShown())
			{
				lineView.OnContinueClicked();
				UpdateButtonsActivity();
			}
		}

		public void UpdateButtonsActivity()
		{
			UpdateButtonsActivityAsync().Forget();
		}

		public async UniTaskVoid UpdateButtonsActivityDelayed()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(0.20000000298023224));
			UpdateButtonsActivityAsync().Forget();
		}

		private async UniTaskVoid UpdateButtonsActivityAsync()
		{
			await UniTask.DelayFrame(1);
			if (optionsView.GetComponentsInChildren<HoverableButton>().Count((HoverableButton x) => x.gameObject.activeInHierarchy) > 0)
			{
				ButtonsDialogLineShowed?.Invoke();
			}
			else
			{
				BaseDialogLineShowed?.Invoke();
			}
		}

		public void SkipLine()
		{
			lineView.OnContinueClicked();
			UpdateButtonsActivity();
		}

		public void SetDialogXPos(float xPos)
		{
			Transform[] lineViewContents = _lineViewContents;
			foreach (Transform transform in lineViewContents)
			{
				transform.transform.localPosition = new Vector3(xPos, transform.transform.localPosition.y, 0f);
			}
		}

		public void MoveDialogContentsToTopLayer()
		{
			DialogueViewBase[] array = dialogueViews;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].transform.SetAsLastSibling();
			}
		}

		public void ShowSign(CharacterSOData character, ECharacterSign sign)
		{
			_dialogSignsView.ShowSign(character, sign);
			if (character.CharacterType == ECharacterType.Player)
			{
				SteamAchievements.Unlock("FIRST_CHECK");
			}
		}

		public void StopShowingSign()
		{
			_dialogSignsView.StopShowingSign();
		}

		public void OnLineViewTextAppeared()
		{
			if (!_isSubtitleShown)
			{
				_usualLineViewText.text = _usualLineViewText.text;
				return;
			}
			_subtitlesView.UpdateText(_usualLineViewText.text);
			_usualLineViewText.text = _usualLineViewText.text;
		}

		public void ShowSubtitle()
		{
			_usualLineView.color = Color.clear;
			_usualLineViewText.color = Color.clear;
			_subtitlesView.Show();
			_isSubtitleShown = true;
		}

		public void HideSubtitle()
		{
			_usualLineView.color = Color.black;
			_usualLineViewText.color = Color.white;
			_subtitlesView.Hide();
			_isSubtitleShown = false;
		}

		public void ShowSubtitlePopup(EInfoMessageType messageType)
		{
			_subtitlesView.ShowDialogForTime(messageType, TimeSpan.FromSeconds(2.5)).Forget();
		}

		public void OnEnergyConsumed()
		{
			EnergyConsumed?.Invoke();
		}

		public void OnDayDialogSelected()
		{
			DayDialogSelected?.Invoke();
		}

		public void OnWereBadBoy()
		{
			WereBadBoy?.Invoke();
		}

		public void ReinitButtons()
		{
			_optionsLineViewText.text = _optionsLineViewText.text;
			if (_inputHandler.CurrentDevice == EInputDevice.Gaypad)
			{
				HoverableButton[] buttons = _buttons;
				if (buttons != null)
				{
					buttons[0].OnSelect(null);
				}
			}
			if (optionsView.OptionsCount != 0 && _initedButtonsCount < optionsView.OptionsCount)
			{
				_buttons = GetComponentsInChildren<HoverableButton>();
				if (!_buttons[0].TryGetComponent<FirstMarker>(out var _))
				{
					_buttons[0].AddComponent<FirstMarker>();
				}
				HoverableButton[] buttons2 = _buttons;
				for (int i = 0; i < buttons2.Length; i++)
				{
					buttons2[i].Init(_soundService);
				}
				_initedButtonsCount = optionsView.OptionsCount;
			}
		}

		public bool IsNodeVisited(string nodeName)
		{
			string variableName = Library.GenerateUniqueVisitedVariableForNode(nodeName);
			if (base.VariableStorage.TryGetValue<float>(variableName, out var result))
			{
				return result >= 1f;
			}
			return false;
		}

		public void OnLoad(DialogSaveData saveData)
		{
			((FakeVariableStorage)base.VariableStorage).Init(saveData);
		}
	}
}
