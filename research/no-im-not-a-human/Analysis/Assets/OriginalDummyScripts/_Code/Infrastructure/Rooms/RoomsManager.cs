using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Zenject;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Rooms;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Rooms
{
	public sealed class RoomsManager : ASavableClass<RoomsSaveData>, IRoomsManager, ITickable, IDisposable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnRoomEnteredAsync_003Ed__53 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ARoom room;

			public RoomsManager _003C_003E4__this;

			private UniTask _003CtaskToAwaitResult_003E5__2;

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
		private struct _003COnRoomLeftAsync_003Ed__55 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public RoomsManager _003C_003E4__this;

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

		private RoomsSaveData _saveData;

		private readonly IRoomsViewProvider _roomsViewProvider;

		private readonly IDayNightController _dayNightController;

		private readonly ICharactersManager _charactersManager;

		private readonly IGameEventsManager _gameEventsManager;

		private readonly RoomDisplayer _roomDisplayer;

		private readonly IPlayerService _playerService;

		private readonly IHUDPresenter _hudPresenter;

		private readonly IDataModelService _dataModelService;

		private readonly CharacterSOData[] _charactersList;

		private readonly IGameplayEndingManager _gameplayEndingManager;

		[CompilerGenerated]
		private Func<ECharacterType, bool> m_IsCharacterAlive;

		public ARoom Kitchen { get; }

		public ARoom Office { get; }

		public ARoom Bedroom { get; }

		public ARoom BigRoom { get; }

		public ARoom Bathroom { get; }

		public ARoom Pantry { get; }

		public ARoom Entrance { get; }

		public bool IsInRoom { get; private set; }

		private ARoom[] AllRooms => null;

		public event Func<ECharacterType, bool> IsCharacterAlive
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

		public RoomsManager(IRoomsViewProvider roomsViewProvider, IDayNightController dayNightController, IOtherGameSODataProvider otherGameSODataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICharactersManager charactersManager, ICloseUpsController closeUpsController, ICursorController cursorController, IRoomDisplayerViewProvider roomDisplayerViewProvider, IPlayerService playerService, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, IInputHandlerProvider inputHandlerProvider, IDataModelService dataModelService, ICharactersSODataProvider charactersSODataProvider, IStateObjectController stateObjectController)
		{
		}

		private void OnRoomKilled(ERoom obj)
		{
		}

		private void OnSetFridgeActivity(bool isActive)
		{
		}

		private void Init()
		{
		}

		private void OnBodyEaterAppeared()
		{
		}

		private void OnBodyEaterDisappeared()
		{
		}

		private void OnDialogStarted()
		{
		}

		private void OnDialogFinished()
		{
		}

		private void OnSexyExploded()
		{
		}

		private void OnActivatedBaby(int index)
		{
		}

		private void OnSpecifiedEventCompleted(string eventName)
		{
		}

		private void OnRoomEntered(ARoom room)
		{
		}

		[AsyncStateMachine(typeof(_003COnRoomEnteredAsync_003Ed__53))]
		private UniTask OnRoomEnteredAsync(ARoom room)
		{
			return default;
		}

		private void OnRoomLeft(ARoom room)
		{
		}

		[AsyncStateMachine(typeof(_003COnRoomLeftAsync_003Ed__55))]
		private UniTask OnRoomLeftAsync()
		{
			return default;
		}

		private void OnCharacterCame(CharacterDingDongEvent dingDingEvent)
		{
		}

		private void OnCharacterCameSoon()
		{
		}

		private void OnDayNightChanged(ETimeOfDay currentTimeOfDay)
		{
		}

		public void WatchTV()
		{
		}

		public void ClearCorpses()
		{
		}

		public void ReinitWhispers(ETimeOfDay currentTimeOfDay)
		{
		}

		public ARoom GetRoom(ERoom roomType)
		{
			return null;
		}

		public void LinkActionableToRoom(ERoom linkedRoom, AActionableObjectView actionableObject)
		{
		}

		public void Tick()
		{
		}

		public void ChangeCharacterPose(ECharacterType character, ERoomPeopleState pose)
		{
		}

		private void ChangeCharacterPoseTomorrow(ECharacterType character, ERoomPeopleState pose)
		{
		}

		private void OnDelayedPosesChanged(List<ChangePoseData> poses)
		{
		}

		private void OnSave(bool isReserve)
		{
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}
	}
}
