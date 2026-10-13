using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using _Code.Characters;
using _Code.Characters.DialogSystem;
using _Code.DialogSystem.Commands;
using _Code.Infrastructure;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DataModel.Models.Meta;
using _Code.Infrastructure.Endings.Data;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Settings;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure._NINAH__Cat;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Menues.HUD.Animations;
using _Code.Player;
using _Code.Rooms;
using _Code.Utils.CustomYarnReading;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.DialogSystem
{
	public sealed class DialogManager : ASavableClass<DialogSaveData>, IDialogManager, IInitializable, IDisposable
	{
		private DialogSaveData _saveData = new DialogSaveData();

		private readonly CharacterSOData[] _characters;

		private IDialogUI _currentUI;

		private EDialogEmotionState _characterEmotion = EDialogEmotionState.Base;

		private List<Action> _onDialogEndedActions = new List<Action>();

		private readonly ICharactersManager _charactersManager;

		private readonly IGameEventsManager _gameEventsManager;

		private readonly IDataModelService _dataModelService;

		private readonly ICutscenesManager _cutscenesManager;

		private readonly DialogView _dialogView;

		private readonly SubtitlesView _overlaySubtitlesView;

		private readonly DialogCommandsInstance _commandsInstance;

		private readonly IConsumablesController _consumablesController;

		private readonly IStateObjectController _stateObjectController;

		private readonly ICatController _catController;

		private readonly CustomYarnReader _customYarnReader;

		private readonly IEndingSODataProvider _endingSoDataProvider;

		private readonly InputHandling _inputHandler;

		private readonly ICursorController _cursorController;

		private Func<int> _getDay;

		private CharacterSOData _currentDialogCharacter;

		private readonly INotAHumanSoundService _soundService;

		private readonly WatcherManager _watcherManager;

		private readonly IEffectsController _effectsController;

		private const float POPUP_TIME = 2.5f;

		public bool IsOpened { get; private set; }

		public bool IsOpenedSubtitle { get; private set; }

		public bool EverRudeToFema => _saveData.EverRudeToFema;

		private Func<int, bool> CheckProphetCondition { get; set; }

		private Func<int, bool> CheckMushroomeaterCondition { get; set; }

		private Func<int, bool> CheckPriestCondition { get; set; }

		public event Action DialogStarted;

		public event Action SubtitleStarted;

		public event Action<bool, bool> DialogEnded;

		public event Action Acted;

		public event Action GunShowed;

		public event Action GunHidden;

		public event Action GunShot;

		public event Action FakedShot;

		public event Action<string, Camera> Dead;

		public event Action EnergyConsumed;

		public event Action EndingTriggered;

		public event Action CutsceneTriggered;

		public event Action ShowedAura;

		public event Action GivenPovistka;

		public event Action FedCat;

		public event Action<float> FadedIn;

		public event Action<float> FadedOut;

		public event Action<ELocation> WentToLocation;

		public event Action CultistsBegun;

		public event Action CatPet;

		public event Action CatTaken;

		public event Action ProphetDontCheckConditionMet;

		public event Action CultistsSaved;

		public event Action UnlockedDeathEnding;

		public event Action<bool> SetFridgeActivity;

		public event Action<EConsumable, int> OrderedCourier;

		public event Action<EPhoneSubscriber> UnlockedPhoneSubscriber;

		public event Action<ESound> StartedWindowNoise;

		public event Action<ECharacterType, ERoomPeopleState> PoseChanged;

		public event Action<ECharacterType, ERoomPeopleState> PoseChangedTomorrow;

		public event Action BaseDialogLineShowed;

		public event Action ButtonsDialogLineShowed;

		public event Action ArmpitsWashed;

		public event Action PlayerRevealedByVigilante;

		public event Action UnlockedMushroomEnding;

		public event Action UnlockedKillerEnding;

		public event Action<EHUDAnimation> PlayedAnimation;

		public event Action IntroSkipped;

		public event Action<ERoom> RoomKilled;

		public event Func<bool> HasCompletedMushroomCheck;

		public event Func<EPhoneSubscriber, string> GotPhoneNumber;

		public event Func<bool> CouldOrderCourier;

		public event Func<int> GotEnergy;

		public event Func<ECharacterSign, bool> PlayerSignShowed;

		public event Func<EDream, bool> HadSeenDream;

		public DialogManager(ICharactersManager charactersManager, IGameEventsManager gameEventsManager, ICharactersSODataProvider charactersSoDataProvider, IDialogViewProvider dialogViewProvider, IDataModelService dataModelService, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, ICutscenesManager cutscenesManager, IConsumablesController consumablesController, IStateObjectController stateObjectController, ICatController catController, ICustomYarnReaderProvider customYarnReaderProvider, ISettingsInstanceProvider settingsInstanceProvider, IEndingSODataProvider endingSoDataProvider, ICursorController cursorController, WatcherManager watcherManager, IEffectsController effectsController)
		{
			_charactersManager = charactersManager;
			_gameEventsManager = gameEventsManager;
			_dataModelService = dataModelService;
			_cutscenesManager = cutscenesManager;
			_dialogView = dialogViewProvider.DialogView;
			_overlaySubtitlesView = dialogViewProvider.OverlaySubtitlesView;
			_commandsInstance = dialogViewProvider.CommandsInstance;
			_characters = charactersSoDataProvider.CharactersData;
			_consumablesController = consumablesController;
			_stateObjectController = stateObjectController;
			_catController = catController;
			_customYarnReader = customYarnReaderProvider.CustomYarnReader;
			_endingSoDataProvider = endingSoDataProvider;
			_inputHandler = inputHandlerProvider.InputHandler;
			_cursorController = cursorController;
			_soundService = soundService;
			_watcherManager = watcherManager;
			_effectsController = effectsController;
			_dialogView.Init(OnDialogStarted, OnDialogEnded, soundService, inputHandlerProvider, _saveData, settingsInstanceProvider, cursorController);
			_commandsInstance.Init(charactersManager, charactersSoDataProvider, gameEventsManager);
			SubscribeEvents();
			InitMaxTalksCount();
			LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
		}

		public void Initialize()
		{
			InitDevThings();
		}

		private void InitDevThings()
		{
		}

		public void InitializeGetDay(Func<int> func)
		{
			_getDay = func;
		}

		public bool IsNodeVisited(string nodeName)
		{
			return _dialogView.IsNodeVisited(nodeName);
		}

		private void SubscribeEvents()
		{
			_commandsInstance.EmotionChanged -= OnEmotionChanged;
			_commandsInstance.CharacterExiled -= OnCharacterExiled;
			_commandsInstance.SignShowed -= OnSignShowed;
			_commandsInstance.PlayerSignShowed -= OnPlayerSignShowed;
			_commandsInstance.StoppedShowingSign -= OnStoppedShowingSign;
			_commandsInstance.GunSetUp -= OnGunSetUp;
			_commandsInstance.GunShot -= OnGunShot;
			_commandsInstance.FakedShot -= OnFakeShot;
			_commandsInstance.Dead -= OnDead;
			_commandsInstance.EndingTriggered -= OnEndingTriggered;
			_commandsInstance.CutsceneTriggered -= OnCutsceneTriggered;
			_commandsInstance.GotItem -= OnGotItem;
			_commandsInstance.TryGiveItem -= OnTryGiveItem;
			_commandsInstance.StateChanged -= OnStateChanged;
			_commandsInstance.GotItemCount -= OnGotItemCount;
			_commandsInstance.FadedIn -= OnFadedIn;
			_commandsInstance.FadedOut -= OnFadedOut;
			_commandsInstance.GotCat -= OnGotCat;
			_commandsInstance.HasCompletedMushroom -= OnHasCompletedMushroomCheck;
			_commandsInstance.WentToLocation -= OnWentToLocation;
			_commandsInstance.CultistsBegun -= OnCultistsBegun;
			_commandsInstance.CatPet -= OnCatPet;
			_commandsInstance.CatTaken -= OnCatTaken;
			_commandsInstance.CharacterExiledByFEMA -= OnExiledByFEMA;
			_commandsInstance.UnlockedPhoneNumber -= OnUnlockedPhoneSubscriber;
			_commandsInstance.GotPhoneNumber -= OnGotPhoneNumber;
			_commandsInstance.CouldOrderCourier -= OnCouldOrderCourier;
			_commandsInstance.OrderedCourier -= OnOrderedCourier;
			_commandsInstance.GotCourierOrder -= OnGotCourierOrder;
			_commandsInstance.StartedWindowNoise -= OnStartedWindowNoise;
			_commandsInstance.CalledFema -= OnCalledFEMA;
			_commandsInstance.PoseChanged -= OnPoseChanged;
			_commandsInstance.PoseChangedTomorrow -= OnPoseChangedTomorrow;
			_commandsInstance.GotEnergy -= OnGotEnergy;
			_commandsInstance.SetFridgeActivity -= OnSetFridgeActivity;
			_commandsInstance.ArmpitsWashed -= OnArmpitsWashed;
			_commandsInstance.NeededToGetDeficitItem -= OnNeedToGetDeficitItem;
			_commandsInstance.PeekedDeficitItem -= OnPeekedDeficitItem;
			_commandsInstance.DeficitDayUpdated -= OnDeficitDayUpdated;
			_commandsInstance.PlayerRevealedByVigilante -= OnPlayerRevealedByVigilante;
			_commandsInstance.CultistsSaved -= OnCultistsSaved;
			_commandsInstance.Sounded -= OnSounded;
			_commandsInstance.UnlockedDeathEnding -= OnUnlockedDeathEnding;
			_commandsInstance.PlayedAnimation -= OnPlayedAnimation;
			_commandsInstance.UnlockedMushroomEnding -= OnUnlockedMushroomEnding;
			_commandsInstance.UnlockedKillerEnding -= OnUnlockedKillerEnding;
			_commandsInstance.IntroSkipped -= OnIntroSkipped;
			_commandsInstance.WasEverCompletedGame -= OnWasEverCompletedGame;
			_commandsInstance.HadSeenDream -= OnHadSeenDream;
			_commandsInstance.SmokeEnabled -= OnSmokeEnabled;
			_commandsInstance.RoomKilled -= OnRoomKilled;
			_dialogView.EnergyConsumed -= OnEnergyConsumed;
			_dialogView.DayDialogSelected -= OnDayDialogSelected;
			_dialogView.BaseDialogLineShowed -= OnBaseDialogLineShowed;
			_dialogView.ButtonsDialogLineShowed -= OnButtonsDialogLineShowed;
			_dialogView.WereBadBoy -= OnWereBadBoy;
			_charactersManager.CharacterAdded -= OnCharacterAdded;
			_commandsInstance.EmotionChanged += OnEmotionChanged;
			_commandsInstance.CharacterExiled += OnCharacterExiled;
			_commandsInstance.SignShowed += OnSignShowed;
			_commandsInstance.PlayerSignShowed += OnPlayerSignShowed;
			_commandsInstance.StoppedShowingSign += OnStoppedShowingSign;
			_commandsInstance.GunSetUp += OnGunSetUp;
			_commandsInstance.GunShot += OnGunShot;
			_commandsInstance.FakedShot += OnFakeShot;
			_commandsInstance.Dead += OnDead;
			_commandsInstance.EndingTriggered += OnEndingTriggered;
			_commandsInstance.CutsceneTriggered += OnCutsceneTriggered;
			_commandsInstance.GotItem += OnGotItem;
			_commandsInstance.TryGiveItem += OnTryGiveItem;
			_commandsInstance.StateChanged += OnStateChanged;
			_commandsInstance.GotItemCount += OnGotItemCount;
			_commandsInstance.FadedIn += OnFadedIn;
			_commandsInstance.FadedOut += OnFadedOut;
			_commandsInstance.GotCat += OnGotCat;
			_commandsInstance.HasCompletedMushroom += OnHasCompletedMushroomCheck;
			_commandsInstance.WentToLocation += OnWentToLocation;
			_commandsInstance.CultistsBegun += OnCultistsBegun;
			_commandsInstance.CatPet += OnCatPet;
			_commandsInstance.CatTaken += OnCatTaken;
			_commandsInstance.CharacterExiledByFEMA += OnExiledByFEMA;
			_commandsInstance.UnlockedPhoneNumber += OnUnlockedPhoneSubscriber;
			_commandsInstance.GotPhoneNumber += OnGotPhoneNumber;
			_commandsInstance.CouldOrderCourier += OnCouldOrderCourier;
			_commandsInstance.OrderedCourier += OnOrderedCourier;
			_commandsInstance.GotCourierOrder += OnGotCourierOrder;
			_commandsInstance.StartedWindowNoise += OnStartedWindowNoise;
			_commandsInstance.CalledFema += OnCalledFEMA;
			_commandsInstance.PoseChanged += OnPoseChanged;
			_commandsInstance.PoseChangedTomorrow += OnPoseChangedTomorrow;
			_commandsInstance.GotEnergy += OnGotEnergy;
			_commandsInstance.SetFridgeActivity += OnSetFridgeActivity;
			_commandsInstance.ArmpitsWashed += OnArmpitsWashed;
			_commandsInstance.NeededToGetDeficitItem += OnNeedToGetDeficitItem;
			_commandsInstance.PeekedDeficitItem += OnPeekedDeficitItem;
			_commandsInstance.DeficitDayUpdated += OnDeficitDayUpdated;
			_commandsInstance.PlayerRevealedByVigilante += OnPlayerRevealedByVigilante;
			_commandsInstance.CultistsSaved += OnCultistsSaved;
			_commandsInstance.Sounded += OnSounded;
			_commandsInstance.UnlockedDeathEnding += OnUnlockedDeathEnding;
			_commandsInstance.PlayedAnimation += OnPlayedAnimation;
			_commandsInstance.UnlockedMushroomEnding += OnUnlockedMushroomEnding;
			_commandsInstance.UnlockedKillerEnding += OnUnlockedKillerEnding;
			_commandsInstance.IntroSkipped += OnIntroSkipped;
			_commandsInstance.WasEverCompletedGame += OnWasEverCompletedGame;
			_commandsInstance.HadSeenDream += OnHadSeenDream;
			_commandsInstance.SmokeEnabled += OnSmokeEnabled;
			_commandsInstance.RoomKilled += OnRoomKilled;
			_dialogView.DayDialogSelected += OnDayDialogSelected;
			_dialogView.EnergyConsumed += OnEnergyConsumed;
			_dialogView.BaseDialogLineShowed += OnBaseDialogLineShowed;
			_dialogView.ButtonsDialogLineShowed += OnButtonsDialogLineShowed;
			_dialogView.WereBadBoy += OnWereBadBoy;
			_charactersManager.CharacterAdded -= OnCharacterAdded;
		}

		private void OnSmokeEnabled()
		{
			_effectsController.EnableSmoke3D();
		}

		private bool OnHadSeenDream(EDream dream)
		{
			return HadSeenDream?.Invoke(dream) ?? false;
		}

		private void OnRoomKilled(ECharacterType obj)
		{
			ERoom characterRoom = _charactersManager.GetCharacterRoom(obj);
			RoomKilled?.Invoke(characterRoom);
		}

		private bool OnWasEverCompletedGame()
		{
			return _dataModelService.MetaPrefsDataHandler.GetCompletedGameTimesCount(EGameCompleteType.ExcludeEarlyEndings) > 0;
		}

		private void OnIntroSkipped()
		{
			IntroSkipped?.Invoke();
			_consumablesController.TryRemove(EConsumable.Bobeer);
		}

		private void OnUnlockedKillerEnding()
		{
			UnlockedKillerEnding?.Invoke();
		}

		private void OnUnlockedMushroomEnding()
		{
			UnlockedMushroomEnding?.Invoke();
		}

		private void OnCharacterAdded(ECharacterType character, bool isFromSave)
		{
			if (character == ECharacterType.Ballerina && !isFromSave)
			{
				_saveData.DidntCheckSignsDays = 0;
			}
		}

		private void OnPlayedAnimation(EHUDAnimation animation)
		{
			PlayedAnimation?.Invoke(animation);
		}

		private void OnUnlockedDeathEnding()
		{
			UnlockedDeathEnding?.Invoke();
		}

		private void OnSounded(ESound sound)
		{
			_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, sound);
		}

		private void OnCultistsSaved()
		{
			CultistsSaved?.Invoke();
		}

		private void OnPlayerRevealedByVigilante()
		{
			PlayerRevealedByVigilante?.Invoke();
		}

		private void OnWereBadBoy()
		{
			if (_currentDialogCharacter.CharacterType == ECharacterType.Gasmask)
			{
				_saveData.EverRudeToFema = true;
			}
		}

		private void OnDayDialogSelected()
		{
			_charactersManager.OnDayDialogTalked(_saveData.HasAlreadyTalkedToday(_currentDialogCharacter.CharacterType));
		}

		private void OnDeficitDayUpdated()
		{
			_consumablesController.UpdateDeficitDay();
		}

		private string OnPeekedDeficitItem()
		{
			return _consumablesController.GetDeficitItem().ToString();
		}

		private bool OnNeedToGetDeficitItem()
		{
			return _consumablesController.IsNeedToGetDeficitItem();
		}

		private void OnArmpitsWashed()
		{
			ArmpitsWashed?.Invoke();
		}

		private void OnBaseDialogLineShowed()
		{
			BaseDialogLineShowed?.Invoke();
		}

		private void OnButtonsDialogLineShowed()
		{
			ButtonsDialogLineShowed?.Invoke();
		}

		private void OnSetFridgeActivity(bool isActive)
		{
			SetFridgeActivity?.Invoke(isActive);
		}

		private int OnGotEnergy()
		{
			return GotEnergy?.Invoke() ?? 0;
		}

		private void OnPoseChangedTomorrow(ECharacterType character, ERoomPeopleState pose)
		{
			PoseChangedTomorrow?.Invoke(character, pose);
		}

		private void OnPoseChanged(ECharacterType character, ERoomPeopleState pose)
		{
			PoseChanged?.Invoke(character, pose);
		}

		private void OnCalledFEMA()
		{
			_gameEventsManager.CallFema();
		}

		public void RefreshSignChecks()
		{
			if (_saveData.HasCheckedSignsToday)
			{
				_saveData.DidntCheckSignsDays = 0;
			}
			else
			{
				_saveData.DidntCheckSignsDays++;
			}
			_saveData.HasCheckedSignsToday = false;
			if (_saveData.DidntCheckSignsDays >= _endingSoDataProvider.EndingSOData.ProphetDontCheckSignsDaysNeeded)
			{
				ProphetDontCheckConditionMet?.Invoke();
			}
		}

		public void InitializeProphetCondition(Func<int, bool> condition)
		{
			CheckProphetCondition = condition;
			_gameEventsManager.InitializeProphetCondition(CheckProphetCondition);
		}

		public void InitializeMushroomeaterCondition(Func<int, bool> condition)
		{
			CheckMushroomeaterCondition = condition;
			_gameEventsManager.InitializeMushroomeaterCondition(CheckMushroomeaterCondition);
		}

		public void InitializePriestCondition(Func<int, bool> condition)
		{
			CheckPriestCondition = condition;
			_gameEventsManager.InitializePriestCondition(CheckPriestCondition);
		}

		private void OnStartedWindowNoise(ESound sound)
		{
			StartedWindowNoise?.Invoke(sound);
		}

		private DialogCourierOrderData OnGotCourierOrder()
		{
			DialogCourierOrderData dialogCourierOrderData = new DialogCourierOrderData(_saveData.CourierOrderData.Consumable, _saveData.CourierOrderData.Count);
			_saveData.CourierOrderData = null;
			OnGotItem(dialogCourierOrderData.Consumable, dialogCourierOrderData.Count);
			return dialogCourierOrderData;
		}

		private void OnOrderedCourier(EConsumable consumable, int count)
		{
			OrderedCourier?.Invoke(consumable, count);
			_saveData.CourierOrderData = new DialogCourierOrderData(consumable, count);
		}

		private bool OnCouldOrderCourier()
		{
			return CouldOrderCourier?.Invoke() ?? false;
		}

		private string OnGotPhoneNumber(EPhoneSubscriber subscriber)
		{
			return GotPhoneNumber?.Invoke(subscriber);
		}

		private void OnUnlockedPhoneSubscriber(EPhoneSubscriber subscriber)
		{
			UnlockedPhoneSubscriber?.Invoke(subscriber);
		}

		private void OnExiledByFEMA(int count)
		{
			CharacterSOData[] array = _charactersManager.ExilyByFEMA(count);
			if (array != null && array.Length != 0)
			{
				_dialogView.SetBackgroundCharacters(EDialogEmotionState.Sad, array);
			}
		}

		private void OnCatTaken()
		{
			CatTaken?.Invoke();
		}

		private void OnCatPet()
		{
			CatPet?.Invoke();
		}

		private void InitMaxTalksCount()
		{
			CharacterSOData[] characters = _characters;
			foreach (CharacterSOData characterSOData in characters)
			{
				_saveData.AddMaxTalksCount(characterSOData.CharacterType, _customYarnReader.GetNodesCountContainsString("talk_" + characterSOData.CharacterType.ToString().ToLower()));
			}
		}

		private void OnCultistsBegun()
		{
			CultistsBegun?.Invoke();
		}

		private void OnWentToLocation(ELocation location)
		{
			WentToLocation?.Invoke(location);
		}

		private bool OnHasCompletedMushroomCheck()
		{
			return HasCompletedMushroomCheck?.Invoke() ?? false;
		}

		private void OnGotCat()
		{
			_catController.ActivateCat();
		}

		private void OnFadedIn(float duration)
		{
			FadedIn?.Invoke(duration);
		}

		private void OnFadedOut(float duration)
		{
			FadedOut?.Invoke(duration);
		}

		private int OnGotItemCount(EConsumable item)
		{
			return _consumablesController.Count(item);
		}

		private void OnStateChanged(EStateObjectType stateObject, int stateIndex)
		{
			_stateObjectController.SetState(stateObject, stateIndex);
		}

		private bool OnTryGiveItem(EConsumable item, int count, ECharacterType character)
		{
			bool flag = _consumablesController.TryRemove(item, count, isShowHint: true);
			if ((item == EConsumable.Povistka) & flag)
			{
				_charactersManager.GivePovistka(character);
				GivenPovistka?.Invoke();
			}
			if ((item == EConsumable.CatFood) & flag)
			{
				FedCat?.Invoke();
			}
			return flag;
		}

		private void OnGotItem(EConsumable item, int count)
		{
			_consumablesController.Add(item, count, isShowHint: true);
		}

		private void OnCutsceneTriggered(ECutscene cutscene)
		{
			OnCutsceneTriggeredAsync(cutscene).Forget();
		}

		private async UniTaskVoid OnCutsceneTriggeredAsync(ECutscene cutscene)
		{
			_dialogView.SkipLine();
			await UniTask.Delay(TimeSpan.FromSeconds(0.15000000596046448));
			CutsceneTriggered?.Invoke();
			await UniTask.Delay(TimeSpan.FromSeconds(1.0));
			_cutscenesManager.ShowCutscene(cutscene);
		}

		private void OnEndingTriggered()
		{
			EndingTriggered?.Invoke();
		}

		private void OnEnergyConsumed()
		{
			EnergyConsumed?.Invoke();
		}

		public void RunDialog(CharacterSOData character, string nodeName, EDialogOverlayType overlayType = EDialogOverlayType.None, Camera camera = null, DialogViewData viewData = null, bool hideCharacter = false)
		{
			_charactersManager.OnDialogStarted(character.CharacterType);
			_dataModelService.MetaPrefsDataHandler.MeetCharacter(character.CharacterType);
			_currentDialogCharacter = character;
			_dialogView.StartDialog(character, nodeName, overlayType, camera, viewData, hideCharacter);
			_onDialogEndedActions.Add(() =>
			{
				if (!nodeName.Contains("entrance"))
				{
					AddTalk(character.CharacterType);
				}
			});
		}

		public void AddTalk(ECharacterType characterType)
		{
			_saveData.AddCharacterWithWhoTalkedToday(characterType);
		}

		public void SetToLastTalk(ECharacterType character)
		{
			_saveData.SetToLastTalk(character);
		}

		public int GetCurrentDialogIndexForCharacter(CharacterSOData character)
		{
			return _saveData.GetTalksCount(character.CharacterType);
		}

		public void ShowSubtitle(string dialogName, Camera camera = null, EDialogOverlayType overlay = EDialogOverlayType.None, bool autoskip = false)
		{
			if ((object)camera == null)
			{
				camera = Camera.main;
			}
			_dialogView.ShowSubtitle(dialogName, camera, overlay);
			SubtitleStarted?.Invoke();
			IsOpenedSubtitle = true;
			if (autoskip)
			{
				RunSubtitleSkip().Forget();
			}
		}

		private async UniTaskVoid RunSubtitleSkip()
		{
			while (IsOpenedSubtitle)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(4.0));
				_dialogView.SkipLine();
			}
		}

		public void ShowSubtitlePopup(EInfoMessageType messageType, float time = 2.5f)
		{
			_overlaySubtitlesView.ShowDialogForTime(messageType, TimeSpan.FromSeconds(time)).Forget();
		}

		public void ShowSubtitlePopup(string message)
		{
			_overlaySubtitlesView.ShowDialogForTime(message, TimeSpan.FromSeconds(2.5)).Forget();
		}

		public void HideSubtitle()
		{
			_dialogView.HideSubtitle();
			IsOpenedSubtitle = false;
		}

		public void AddActionForNextDialogEnded(Action temporaryDialogAction)
		{
			_dialogView.AddActionForNextDialogEnded(temporaryDialogAction);
		}

		private void OnDead()
		{
			OnDeadAsync().Forget();
		}

		private async UniTaskVoid OnDeadAsync()
		{
			_dialogView.SkipLine();
			await UniTask.WaitUntil(() => !IsOpened);
			Dead?.Invoke("Death_SuperImposter", _dialogView.LastUsedCamera);
		}

		private void OnEmotionChanged(EDialogEmotionState emotionState)
		{
			_dialogView.SetCharacterEmotion(emotionState);
		}

		private void OnCharacterExiled(CharacterSOData character)
		{
			_dialogView.SetBackgroundCharacters(EDialogEmotionState.Base, character);
		}

		private void OnPlayerSignShowed(ECharacterSign sign)
		{
			bool isImposter = PlayerSignShowed?.Invoke(sign) ?? false;
			CharacterSOData characterSOData = _characters.FirstOrDefault((CharacterSOData x) => x.CharacterType == ECharacterType.Player);
			characterSOData.IsImposter = isImposter;
			_dialogView.ShowSign(characterSOData, sign);
		}

		private void OnSignShowed(CharacterSOData character, ECharacterSign sign)
		{
			if (sign == ECharacterSign.AuraPhoto)
			{
				ShowedAura?.Invoke();
			}
			_dialogView.ShowSign(character, sign);
			_saveData.HasCheckedSignsToday = true;
		}

		private void OnStoppedShowingSign()
		{
			_dialogView.StopShowingSign();
		}

		private void OnGunSetUp(bool isShown)
		{
			if (isShown)
			{
				GunShowed?.Invoke();
			}
			else
			{
				GunHidden?.Invoke();
			}
		}

		private void OnGunShot()
		{
			GunShot?.Invoke();
		}

		private void OnFakeShot()
		{
			FakedShot?.Invoke();
		}

		public void CompleteShotAnimation()
		{
			_dialogView.SkipLine();
		}

		public void UpdateTalksCount()
		{
			_saveData.ApplyCharacterTalks();
		}

		private void OnDialogStarted()
		{
			_inputHandler.SetGamepadCursorActiveState(active: false);
			_watcherManager.EnterState(EWatcherState.Dialog);
			IsOpened = true;
			DialogStarted?.Invoke();
			_characterEmotion = EDialogEmotionState.Base;
		}

		private void OnDialogEnded()
		{
			bool isOpenedSubtitle = IsOpenedSubtitle;
			bool isOpened = IsOpened;
			if (IsOpened)
			{
				_inputHandler.SetGamepadCursorActiveState(active: true);
				_watcherManager.LeaveState(EWatcherState.Dialog);
				IsOpened = false;
			}
			if (IsOpenedSubtitle)
			{
				HideSubtitle();
			}
			DialogEnded?.Invoke(isOpened, isOpenedSubtitle);
			foreach (Action onDialogEndedAction in _onDialogEndedActions)
			{
				onDialogEndedAction?.Invoke();
			}
			_onDialogEndedActions = new List<Action>();
		}

		public void AddOnDialogEndedAction(Action action)
		{
			_onDialogEndedActions.Add(action);
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
			_saveData = saver.LoadKey(this);
			_dialogView.OnLoad(_saveData);
			LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
		}
	}
}
