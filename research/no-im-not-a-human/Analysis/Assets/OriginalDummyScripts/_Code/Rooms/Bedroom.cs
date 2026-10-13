using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Menues.HUD;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Rooms
{
	public sealed class Bedroom : ARoom
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAwaitViewInitThenInit_003Ed__20 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public Bedroom _003C_003E4__this;

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
		private struct _003COnCharacterAddedFromLoadAsync_003Ed__22 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public Bedroom _003C_003E4__this;

			public ECharacterType character;

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
		private struct _003COnLoad_003Ed__31 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public Bedroom _003C_003E4__this;

			public RoomsSaveData saveData;

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

		private bool _isMorningTV;

		private IGameplayEndingManager _gameplayEndingManager;

		private IDataModelService _dataModelService;

		private ICharactersManager _charactersManager;

		private readonly IDayNightController _dayNightController;

		private readonly IDialogManager _dialogManager;

		private RoomsSaveData _saveData;

		private float _lastTVDialogEndedTime;

		[CompilerGenerated]
		private Action m_BabyActivated;

		private const float X_DIALOG_POSITION = -575f;

		public override ERoom RoomType => ERoom.Kitchen;

		private BedroomView BedroomView => null;

		public bool CanWatchTV => false;

		public event Action BabyActivated
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

		public Bedroom(IDayNightController dayNightController, IOtherGameSODataProvider otherGameSoDataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, ICharactersManager charactersManager, IDataModelService dataModelService, IStateObjectController stateObjectController)
			: base(null, null, null, null, null, null, null, null, null, null, null, null)
		{
		}

		public void InitModules(IDataModelService dataModelService, ICharactersManager charactersManager, RoomsSaveData saveData)
		{
		}

		[AsyncStateMachine(typeof(_003CAwaitViewInitThenInit_003Ed__20))]
		private UniTask AwaitViewInitThenInit()
		{
			return default;
		}

		private void OnCharacterAdded2(ECharacterType character, bool isFromSave)
		{
		}

		[AsyncStateMachine(typeof(_003COnCharacterAddedFromLoadAsync_003Ed__22))]
		private UniTaskVoid OnCharacterAddedFromLoadAsync(ECharacterType character)
		{
			return default;
		}

		private void OnCharacterAdded(ECharacterType character, bool isFromSave)
		{
		}

		private void OnBabyActivated()
		{
		}

		private void StartTVDialog(CharacterRoomObjectView character, bool ignoreIsActiveCondition = false)
		{
		}

		private new string GetDialogName(CharacterSOData character)
		{
			return null;
		}

		public void WatchMorningTV()
		{
		}

		public void DisableTvIfExtra()
		{
		}

		public void GrowUpBelly()
		{
		}

		public void SetTVActive(bool isActive)
		{
		}

		[AsyncStateMachine(typeof(_003COnLoad_003Ed__31))]
		public UniTask OnLoad(RoomsSaveData saveData)
		{
			return default;
		}
	}
}
