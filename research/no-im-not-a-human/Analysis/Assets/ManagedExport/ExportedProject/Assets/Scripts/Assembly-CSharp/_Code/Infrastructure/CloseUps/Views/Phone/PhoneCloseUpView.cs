using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Infrastructure._NINAH__CloseUps.Views.Phone.Pins;
using _Code.Player;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;

namespace _Code.Infrastructure.CloseUps.Views.Phone
{
	public sealed class PhoneCloseUpView : ACloseUpView
	{
		[SerializeField]
		private PhoneButtonView[] _phoneButtonViews;

		[SerializeField]
		private TMP_Text _screenText;

		[SerializeField]
		private CharacterSOData _phoneCharacter;

		[SerializeField]
		private PhonePinController _phonePinController;

		[SerializeField]
		private GameObject _camera;

		private string _screenContent = "";

		private EPhoneState _phoneState;

		private readonly PhoneNumbersController _phoneNumbersController = new PhoneNumbersController();

		private IDialogManager _dialogManager;

		private IDayNightController _dayNightController;

		private ICursorController _cursorController;

		private CloseUpSaveData _saveData;

		private OtherGameSOData _otherGameSOData;

		private const float MaxRealCallDelay = 5f;

		private const float MinRealCallDelay = 2.5f;

		private bool _canLeave = true;

		private ICharactersManager _charactersManager;

		private string _lastActualNumber;

		private InputHandling _inputHandling;

		private bool _isActive;

		private PhoneButtonView _selectedButton;

		private IDataModelService _dataModelService;

		private WatcherManager _watcherManager;

		public override IUpdateable[] Updateables => new IUpdateable[1] { this };

		public bool IsAnimating { get; private set; }

		public override void Init()
		{
			base.Init();
			PhoneButtonView[] phoneButtonViews = _phoneButtonViews;
			foreach (PhoneButtonView obj in phoneButtonViews)
			{
				obj.Pressed -= OnPhoneButtonPressed;
				obj.Pressed += OnPhoneButtonPressed;
				obj.PressedUp -= OnPhoneButtonPressedUp;
				obj.PressedUp += OnPhoneButtonPressedUp;
				obj.Selected -= OnSelected;
				obj.Selected += OnSelected;
			}
			_phoneNumbersController.Init(_saveData, _otherGameSOData);
			_phoneNumbersController.GenerateNumbers();
			_phonePinController.Init(_saveData);
		}

		public void InitModules(IDialogManager dialogManager, IDayNightController dayNightController, CloseUpSaveData saveData, OtherGameSOData otherGameSOData, ICursorController cursorController, ICharactersManager charactersManager, InputHandling inputHandling, IDataModelService dataModelService, WatcherManager watcherManager)
		{
			_dialogManager = dialogManager;
			_dayNightController = dayNightController;
			_saveData = saveData;
			_otherGameSOData = otherGameSOData;
			_cursorController = cursorController;
			_charactersManager = charactersManager;
			_inputHandling = inputHandling;
			_dataModelService = dataModelService;
			_watcherManager = watcherManager;
			dayNightController.DayChanged -= OnDayChanged;
			dayNightController.DayChanged += OnDayChanged;
		}

		private void OnDayChanged(int day)
		{
			_saveData.TodayCalls.Clear();
		}

		public override async UniTask Show()
		{
			IsAnimating = true;
			_camera.SetActive(value: true);
			SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Sfx_phone_pickup);
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceDay, 0.5f, 1f);
			PlayerService.MakeImmovable();
			_watcherManager.EnterState(EWatcherState.Phone);
			HUDPresenter.SetupAndShowControlsView(EControlsList.InPhone);
			_cursorController.SetType(ECursorType.Hover);
			await base.Show();
			IsAnimating = false;
			if (_inputHandling.CurrentDevice == EInputDevice.Gaypad)
			{
				EventSystem.current.SetSelectedGameObject(_phoneButtonViews[2].gameObject);
			}
			_isActive = true;
		}

		public override async UniTask Hide()
		{
			if (_canLeave && !IsAnimating)
			{
				IsAnimating = true;
				_camera.SetActive(value: false);
				SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Sfx_phone_hangDown);
				SoundService.FadeVolume(ESoundSource.s1_GameScene_2DMusicSourceDay, 1f, 1f);
				PlayerService.MakeMovable();
				_watcherManager.LeaveState(EWatcherState.Phone);
				HUDPresenter.HideControlsView();
				_cursorController.SetType(ECursorType.Normal);
				await base.Hide();
				IsAnimating = false;
				_isActive = false;
			}
		}

		private void Update()
		{
			if (_inputHandling.CurrentDevice == EInputDevice.Gaypad && _inputHandling.UISubmitDown)
			{
				_selectedButton?.OnPointerDown(null);
			}
			if (_inputHandling.CurrentDevice == EInputDevice.Gaypad && _inputHandling.UISubmitUp)
			{
				_selectedButton?.OnPointerUp(null);
			}
		}

		private void OnSelected(PhoneButtonView button)
		{
			_selectedButton = button;
		}

		private void OnPhoneButtonPressedUp(EPhoneKey key)
		{
			if (_inputHandling.CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
		}

		private void OnPhoneButtonPressed(EPhoneKey key)
		{
			if (!_canLeave)
			{
				return;
			}
			switch (key)
			{
			case EPhoneKey.D0:
			case EPhoneKey.D1:
			case EPhoneKey.D2:
			case EPhoneKey.D3:
			case EPhoneKey.D4:
			case EPhoneKey.D5:
			case EPhoneKey.D6:
			case EPhoneKey.D7:
			case EPhoneKey.D8:
			case EPhoneKey.D9:
			{
				ESound clip = Enum.Parse<ESound>("Sfx_phone_dial_" + key.ToString()[1]);
				SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, clip);
				if (_screenContent.Length < 6)
				{
					_screenContent += key.ToString()[1];
				}
				break;
			}
			case EPhoneKey.Clear:
				_screenContent = string.Empty;
				break;
			case EPhoneKey.Call:
				TryCall().Forget();
				break;
			case EPhoneKey.Star:
				SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Sfx_phone_dial_star);
				break;
			case EPhoneKey.Hash:
				SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Sfx_phone_dial_sharp);
				break;
			}
			UpdateScreen();
		}

		private async UniTaskVoid TryCall()
		{
			if (_screenContent.Length >= 6)
			{
				if (_inputHandling.CurrentDevice == EInputDevice.Gaypad)
				{
					_selectedButton.OnPointerUp(null);
					_selectedButton = null;
					EventSystem.current.SetSelectedGameObject(null);
				}
				PhoneButtonView[] phoneButtonViews = _phoneButtonViews;
				for (int i = 0; i < phoneButtonViews.Length; i++)
				{
					phoneButtonViews[i].Disable();
				}
				_lastActualNumber = _screenText.text;
				if (_phoneNumbersController.TryGetSubscriberByPhone(_screenContent, out var subscriber))
				{
					_canLeave = false;
					await Call();
					await CallTo(subscriber);
					ResetPhone();
				}
				else
				{
					_canLeave = false;
					await Call();
					await EmptyCall();
					ResetPhone();
					_canLeave = true;
				}
				phoneButtonViews = _phoneButtonViews;
				for (int i = 0; i < phoneButtonViews.Length; i++)
				{
					phoneButtonViews[i].Enable();
				}
				_screenContent = string.Empty;
				UpdateScreen();
			}
		}

		private async UniTask Call()
		{
			_phoneState = EPhoneState.Calling;
			SoundService.Play(ESoundSource.s1_GameScene_2DSFXSource, ESound.Phone_call_beep, new SoundSettingCycling());
			await UniTask.Delay(TimeSpan.FromSeconds(UnityEngine.Random.Range(2.5f, 5f)));
			SoundService.Stop(ESoundSource.s1_GameScene_2DSFXSource);
		}

		private async UniTask CallTo(EPhoneSubscriber phoneSubscriber)
		{
			_phoneState = EPhoneState.Talking;
			UpdateScreen();
			await TalkTo(phoneSubscriber);
			if (phoneSubscriber == EPhoneSubscriber.FEMA)
			{
				_saveData.FemaCalls++;
			}
		}

		private async UniTask EmptyCall()
		{
			SoundService.Play(ESoundSource.s1_GameScene_2DSFXSource, ESound.Phone_call_canceled);
			_phoneState = EPhoneState.Error;
			UpdateScreen();
			await UniTask.Delay(TimeSpan.FromSeconds(4.0));
		}

		private void ResetPhone()
		{
			_phoneState = EPhoneState.EnteringNumber;
			UpdateScreen();
		}

		private async UniTask TalkTo(EPhoneSubscriber phoneSubscriber)
		{
			switch (phoneSubscriber)
			{
			case EPhoneSubscriber.Neighbour:
			{
				int day = _dayNightController.Day;
				if (day < 2 || day > 4 || _saveData.TodayCalls.Contains(phoneSubscriber))
				{
					await EmptyCall();
					_canLeave = true;
					return;
				}
				break;
			}
			case EPhoneSubscriber.Psychics:
			case EPhoneSubscriber.Extrasens:
			case EPhoneSubscriber.PhoneRoulette:
				if (_saveData.CallsCount.ContainsKey(phoneSubscriber) && _saveData.CallsCount[phoneSubscriber] > 0)
				{
					await EmptyCall();
					_canLeave = true;
					return;
				}
				break;
			case EPhoneSubscriber.BestSonFamily:
				if ((_saveData.CallsCount.ContainsKey(phoneSubscriber) && _saveData.CallsCount[phoneSubscriber] > 0) || !_charactersManager.IsCharacterAliveInside(ECharacterType.Bestson))
				{
					await EmptyCall();
					_canLeave = true;
					return;
				}
				break;
			case EPhoneSubscriber.MothersHusband:
				if ((_saveData.CallsCount.ContainsKey(phoneSubscriber) && _saveData.CallsCount[phoneSubscriber] > 0) || !_charactersManager.IsCharacterAliveInside(ECharacterType.Mother))
				{
					await EmptyCall();
					_canLeave = true;
					return;
				}
				break;
			case EPhoneSubscriber.AngersWife:
				if ((_saveData.CallsCount.ContainsKey(phoneSubscriber) && _saveData.CallsCount[phoneSubscriber] > 0) || !_charactersManager.IsCharacterAliveInside(ECharacterType.Anger))
				{
					await EmptyCall();
					_canLeave = true;
					return;
				}
				break;
			case EPhoneSubscriber.FEMARecruiter:
				if ((_saveData.CallsCount.ContainsKey(phoneSubscriber) && _saveData.CallsCount[phoneSubscriber] > 0) || !_charactersManager.IsCharacterAliveInside(ECharacterType.Wifefema))
				{
					await EmptyCall();
					_canLeave = true;
					return;
				}
				break;
			case EPhoneSubscriber.ForeignEmbassy:
				if ((_saveData.CallsCount.ContainsKey(phoneSubscriber) && _saveData.CallsCount[phoneSubscriber] > 0) || !_charactersManager.IsCharacterAliveInside(ECharacterType.Foreigner))
				{
					await EmptyCall();
					_canLeave = true;
					return;
				}
				break;
			case EPhoneSubscriber.DaughtersFriend:
				if ((_saveData.CallsCount.ContainsKey(phoneSubscriber) && _saveData.CallsCount[phoneSubscriber] > 0) || !_charactersManager.IsCharacterAliveInside(ECharacterType.Daughter))
				{
					await EmptyCall();
					_canLeave = true;
					return;
				}
				break;
			case EPhoneSubscriber.AlkonostsHusband:
				if ((_saveData.CallsCount.ContainsKey(phoneSubscriber) && _saveData.CallsCount[phoneSubscriber] > 0) || !_charactersManager.IsCharacterAliveInside(ECharacterType.Alkonost) || _dialogManager.IsNodeVisited("Alkonost_Asking_4"))
				{
					await EmptyCall();
					_canLeave = true;
					return;
				}
				break;
			}
			string text = phoneSubscriber.ToString().ToLower();
			string text2 = ((phoneSubscriber != EPhoneSubscriber.Neighbour) ? "" : $"_day{_dayNightController.Day}");
			string nodeName = "phone_" + text + text2;
			_canLeave = false;
			_dataModelService.MetaPrefsDataHandler.CallSubscriber(phoneSubscriber);
			_dialogManager.RunDialog(_phoneCharacter, nodeName);
			if (!_saveData.CallsCount.TryAdd(phoneSubscriber, 1))
			{
				_saveData.CallsCount[phoneSubscriber]++;
			}
			_saveData.TodayCalls.Add(phoneSubscriber);
			_dialogManager.DialogEnded += OnDialogEnded;
			await UniTask.WaitUntil(() => !_dialogManager.IsOpened);
		}

		private void OnDialogEnded(bool dialogState, bool subtitleState)
		{
			_dialogManager.DialogEnded -= OnDialogEnded;
			_canLeave = true;
		}

		private void UpdateScreen()
		{
			switch (_phoneState)
			{
			case EPhoneState.EnteringNumber:
			{
				string text = "";
				for (int i = 0; i < 6; i++)
				{
					text = ((_screenContent.Length <= i) ? (text + "_") : (text + _screenContent[i]));
					if (i == 2)
					{
						text += "-";
					}
				}
				_screenText.text = text;
				break;
			}
			case EPhoneState.Calling:
				_screenText.text = "CALLING";
				break;
			case EPhoneState.Talking:
				_screenText.text = _lastActualNumber;
				break;
			case EPhoneState.Error:
				_screenText.text = "INVALID";
				break;
			}
		}

		public void UnlockPhoneSubscriber(EPhoneSubscriber phoneSubscriber)
		{
			_saveData.UnlockedPhoneSubscribers.Add(phoneSubscriber);
			_saveData.DistinctNumbers();
			_phonePinController.Create(phoneSubscriber, _phoneNumbersController.GetNumberBySubscriber(phoneSubscriber));
		}

		public string GetPhoneNumber(EPhoneSubscriber subscriber)
		{
			return _phoneNumbersController.GetNumberBySubscriber(subscriber);
		}

		public void RestorePhoneNumbers(List<EPhoneSubscriber> phoneSubscribers)
		{
			for (int i = 0; i < phoneSubscribers.Count; i++)
			{
				EPhoneSubscriber ePhoneSubscriber = phoneSubscribers[i];
				Debug.Log(ePhoneSubscriber);
				_phonePinController.CreateFromOld(ePhoneSubscriber, _phoneNumbersController.GetNumberBySubscriber(ePhoneSubscriber), i);
			}
		}

		public void ReinitSaveData(CloseUpSaveData saveData)
		{
			_saveData = saveData;
			_phonePinController.ReinitSaveData(saveData);
			_phoneNumbersController.ReinitSaveData(saveData);
		}
	}
}
