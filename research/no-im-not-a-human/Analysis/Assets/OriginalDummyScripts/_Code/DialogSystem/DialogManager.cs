using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
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
using _Scripts.Services.Sound.Service;

namespace _Code.DialogSystem
{
	public sealed class DialogManager : ASavableClass<DialogSaveData>, IDialogManager, IInitializable, IDisposable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnCutsceneTriggeredAsync_003Ed__239 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public DialogManager _003C_003E4__this;

			public ECutscene cutscene;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnDeadAsync_003Ed__253 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public DialogManager _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CRunSubtitleSkip_003Ed__247 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public DialogManager _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		private DialogSaveData _saveData;

		private readonly CharacterSOData[] _characters;

		private IDialogUI _currentUI;

		private EDialogEmotionState _characterEmotion;

		private List<Action> _onDialogEndedActions;

		[CompilerGenerated]
		private Action m_DialogStarted;

		[CompilerGenerated]
		private Action m_SubtitleStarted;

		[CompilerGenerated]
		private Action<bool, bool> m_DialogEnded;

		[CompilerGenerated]
		private Action m_Acted;

		[CompilerGenerated]
		private Action m_GunShowed;

		[CompilerGenerated]
		private Action m_GunHidden;

		[CompilerGenerated]
		private Action m_GunShot;

		[CompilerGenerated]
		private Action m_FakedShot;

		[CompilerGenerated]
		private Action<string, Camera> m_Dead;

		[CompilerGenerated]
		private Action m_EnergyConsumed;

		[CompilerGenerated]
		private Action m_EndingTriggered;

		[CompilerGenerated]
		private Action m_CutsceneTriggered;

		[CompilerGenerated]
		private Action m_ShowedAura;

		[CompilerGenerated]
		private Action m_GivenPovistka;

		[CompilerGenerated]
		private Action m_FedCat;

		[CompilerGenerated]
		private Action<float> m_FadedIn;

		[CompilerGenerated]
		private Action<float> m_FadedOut;

		[CompilerGenerated]
		private Action<ELocation> m_WentToLocation;

		[CompilerGenerated]
		private Action m_CultistsBegun;

		[CompilerGenerated]
		private Action m_CatPet;

		[CompilerGenerated]
		private Action m_CatTaken;

		[CompilerGenerated]
		private Action m_ProphetDontCheckConditionMet;

		[CompilerGenerated]
		private Action m_CultistsSaved;

		[CompilerGenerated]
		private Action m_UnlockedDeathEnding;

		[CompilerGenerated]
		private Action<bool> m_SetFridgeActivity;

		[CompilerGenerated]
		private Action<EConsumable, int> m_OrderedCourier;

		[CompilerGenerated]
		private Action<EPhoneSubscriber> m_UnlockedPhoneSubscriber;

		[CompilerGenerated]
		private Action<ESound> m_StartedWindowNoise;

		[CompilerGenerated]
		private Action<ECharacterType, ERoomPeopleState> m_PoseChanged;

		[CompilerGenerated]
		private Action<ECharacterType, ERoomPeopleState> m_PoseChangedTomorrow;

		[CompilerGenerated]
		private Action m_BaseDialogLineShowed;

		[CompilerGenerated]
		private Action m_ButtonsDialogLineShowed;

		[CompilerGenerated]
		private Action m_ArmpitsWashed;

		[CompilerGenerated]
		private Action m_PlayerRevealedByVigilante;

		[CompilerGenerated]
		private Action m_UnlockedMushroomEnding;

		[CompilerGenerated]
		private Action m_UnlockedKillerEnding;

		[CompilerGenerated]
		private Action<EHUDAnimation> m_PlayedAnimation;

		[CompilerGenerated]
		private Action m_IntroSkipped;

		[CompilerGenerated]
		private Action<ERoom> m_RoomKilled;

		[CompilerGenerated]
		private Func<bool> m_HasCompletedMushroomCheck;

		[CompilerGenerated]
		private Func<EPhoneSubscriber, string> m_GotPhoneNumber;

		[CompilerGenerated]
		private Func<bool> m_CouldOrderCourier;

		[CompilerGenerated]
		private Func<int> m_GotEnergy;

		[CompilerGenerated]
		private Func<ECharacterSign, bool> m_PlayerSignShowed;

		[CompilerGenerated]
		private Func<EDream, bool> m_HadSeenDream;

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

		public bool EverRudeToFema => false;

		private Func<int, bool> CheckProphetCondition { get; set; }

		private Func<int, bool> CheckMushroomeaterCondition { get; set; }

		private Func<int, bool> CheckPriestCondition { get; set; }

		public event Action DialogStarted
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action SubtitleStarted
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<bool, bool> DialogEnded
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action Acted
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action GunShowed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action GunHidden
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action GunShot
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action FakedShot
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<string, Camera> Dead
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action EnergyConsumed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action EndingTriggered
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CutsceneTriggered
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action ShowedAura
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action GivenPovistka
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action FedCat
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<float> FadedIn
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<float> FadedOut
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ELocation> WentToLocation
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CultistsBegun
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CatPet
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CatTaken
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action ProphetDontCheckConditionMet
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CultistsSaved
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action UnlockedDeathEnding
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<bool> SetFridgeActivity
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EConsumable, int> OrderedCourier
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EPhoneSubscriber> UnlockedPhoneSubscriber
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ESound> StartedWindowNoise
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ECharacterType, ERoomPeopleState> PoseChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ECharacterType, ERoomPeopleState> PoseChangedTomorrow
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action BaseDialogLineShowed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action ButtonsDialogLineShowed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action ArmpitsWashed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action PlayerRevealedByVigilante
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action UnlockedMushroomEnding
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action UnlockedKillerEnding
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EHUDAnimation> PlayedAnimation
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action IntroSkipped
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ERoom> RoomKilled
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<bool> HasCompletedMushroomCheck
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<EPhoneSubscriber, string> GotPhoneNumber
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<bool> CouldOrderCourier
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<int> GotEnergy
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<ECharacterSign, bool> PlayerSignShowed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<EDream, bool> HadSeenDream
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public DialogManager(ICharactersManager charactersManager, IGameEventsManager gameEventsManager, ICharactersSODataProvider charactersSoDataProvider, IDialogViewProvider dialogViewProvider, IDataModelService dataModelService, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, ICutscenesManager cutscenesManager, IConsumablesController consumablesController, IStateObjectController stateObjectController, ICatController catController, ICustomYarnReaderProvider customYarnReaderProvider, ISettingsInstanceProvider settingsInstanceProvider, IEndingSODataProvider endingSoDataProvider, ICursorController cursorController, WatcherManager watcherManager, IEffectsController effectsController)
		{
		}

		public void Initialize()
		{
		}

		private void InitDevThings()
		{
		}

		public void InitializeGetDay(Func<int> func)
		{
		}

		public bool IsNodeVisited(string nodeName)
		{
			return false;
		}

		private void SubscribeEvents()
		{
		}

		private void OnSmokeEnabled()
		{
		}

		private bool OnHadSeenDream(EDream dream)
		{
			return false;
		}

		private void OnRoomKilled(ECharacterType obj)
		{
		}

		private bool OnWasEverCompletedGame()
		{
			return false;
		}

		private void OnIntroSkipped()
		{
		}

		private void OnUnlockedKillerEnding()
		{
		}

		private void OnUnlockedMushroomEnding()
		{
		}

		private void OnCharacterAdded(ECharacterType character, bool isFromSave)
		{
		}

		private void OnPlayedAnimation(EHUDAnimation animation)
		{
		}

		private void OnUnlockedDeathEnding()
		{
		}

		private void OnSounded(ESound sound)
		{
		}

		private void OnCultistsSaved()
		{
		}

		private void OnPlayerRevealedByVigilante()
		{
		}

		private void OnWereBadBoy()
		{
		}

		private void OnDayDialogSelected()
		{
		}

		private void OnDeficitDayUpdated()
		{
		}

		private string OnPeekedDeficitItem()
		{
			return null;
		}

		private bool OnNeedToGetDeficitItem()
		{
			return false;
		}

		private void OnArmpitsWashed()
		{
		}

		private void OnBaseDialogLineShowed()
		{
		}

		private void OnButtonsDialogLineShowed()
		{
		}

		private void OnSetFridgeActivity(bool isActive)
		{
		}

		private int OnGotEnergy()
		{
			return 0;
		}

		private void OnPoseChangedTomorrow(ECharacterType character, ERoomPeopleState pose)
		{
		}

		private void OnPoseChanged(ECharacterType character, ERoomPeopleState pose)
		{
		}

		private void OnCalledFEMA()
		{
		}

		public void RefreshSignChecks()
		{
		}

		public void InitializeProphetCondition(Func<int, bool> condition)
		{
		}

		public void InitializeMushroomeaterCondition(Func<int, bool> condition)
		{
		}

		public void InitializePriestCondition(Func<int, bool> condition)
		{
		}

		private void OnStartedWindowNoise(ESound sound)
		{
		}

		private DialogCourierOrderData OnGotCourierOrder()
		{
			return null;
		}

		private void OnOrderedCourier(EConsumable consumable, int count)
		{
		}

		private bool OnCouldOrderCourier()
		{
			return false;
		}

		private string OnGotPhoneNumber(EPhoneSubscriber subscriber)
		{
			return null;
		}

		private void OnUnlockedPhoneSubscriber(EPhoneSubscriber subscriber)
		{
		}

		private void OnExiledByFEMA(int count)
		{
		}

		private void OnCatTaken()
		{
		}

		private void OnCatPet()
		{
		}

		private void InitMaxTalksCount()
		{
		}

		private void OnCultistsBegun()
		{
		}

		private void OnWentToLocation(ELocation location)
		{
		}

		private bool OnHasCompletedMushroomCheck()
		{
			return false;
		}

		private void OnGotCat()
		{
		}

		private void OnFadedIn(float duration)
		{
		}

		private void OnFadedOut(float duration)
		{
		}

		private int OnGotItemCount(EConsumable item)
		{
			return 0;
		}

		private void OnStateChanged(EStateObjectType stateObject, int stateIndex)
		{
		}

		private bool OnTryGiveItem(EConsumable item, int count, ECharacterType character)
		{
			return false;
		}

		private void OnGotItem(EConsumable item, int count)
		{
		}

		private void OnCutsceneTriggered(ECutscene cutscene)
		{
		}

		[AsyncStateMachine(typeof(_003COnCutsceneTriggeredAsync_003Ed__239))]
		private UniTaskVoid OnCutsceneTriggeredAsync(ECutscene cutscene)
		{
			return default;
		}

		private void OnEndingTriggered()
		{
		}

		private void OnEnergyConsumed()
		{
		}

		public void RunDialog(CharacterSOData character, string nodeName, EDialogOverlayType overlayType = EDialogOverlayType.None, Camera camera = null, DialogViewData viewData = null, bool hideCharacter = false)
		{
		}

		public void AddTalk(ECharacterType characterType)
		{
		}

		public void SetToLastTalk(ECharacterType character)
		{
		}

		public int GetCurrentDialogIndexForCharacter(CharacterSOData character)
		{
			return 0;
		}

		public void ShowSubtitle(string dialogName, Camera camera = null, EDialogOverlayType overlay = EDialogOverlayType.None, bool autoskip = false)
		{
		}

		[AsyncStateMachine(typeof(_003CRunSubtitleSkip_003Ed__247))]
		private UniTaskVoid RunSubtitleSkip()
		{
			return default;
		}

		public void ShowSubtitlePopup(EInfoMessageType messageType, float time = 2.5f)
		{
		}

		public void ShowSubtitlePopup(string message)
		{
		}

		public void HideSubtitle()
		{
		}

		public void AddActionForNextDialogEnded(Action temporaryDialogAction)
		{
		}

		private void OnDead()
		{
		}

		[AsyncStateMachine(typeof(_003COnDeadAsync_003Ed__253))]
		private UniTaskVoid OnDeadAsync()
		{
			return default;
		}

		private void OnEmotionChanged(EDialogEmotionState emotionState)
		{
		}

		private void OnCharacterExiled(CharacterSOData character)
		{
		}

		private void OnPlayerSignShowed(ECharacterSign sign)
		{
		}

		private void OnSignShowed(CharacterSOData character, ECharacterSign sign)
		{
		}

		private void OnStoppedShowingSign()
		{
		}

		private void OnGunSetUp(bool isShown)
		{
		}

		private void OnGunShot()
		{
		}

		private void OnFakeShot()
		{
		}

		public void CompleteShotAnimation()
		{
		}

		public void UpdateTalksCount()
		{
		}

		private void OnDialogStarted()
		{
		}

		private void OnDialogEnded()
		{
		}

		public void AddOnDialogEndedAction(Action action)
		{
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}
	}
}
