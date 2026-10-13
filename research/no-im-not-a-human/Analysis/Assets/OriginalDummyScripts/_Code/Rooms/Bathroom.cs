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
using _Code.Menues.HUD;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Rooms
{
	public sealed class Bathroom : ARoom
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAwaitViewInitThenInit_003Ed__8 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public Bathroom _003C_003E4__this;

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

		private readonly IGameplayEndingManager _gameplayEndingManager;

		private readonly IDialogManager _dialogManager;

		private readonly IDayNightController _dayNightController;

		public override ERoom RoomType => ERoom.Kitchen;

		private BathroomView BathroomView => null;

		public Bathroom(IDayNightController dayNightController, IOtherGameSODataProvider otherGameSoDataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, ICharactersManager charactersManager, IDataModelService dataModelService, IStateObjectController stateObjectController)
			: base(null, null, null, null, null, null, null, null, null, null, null, null)
		{
		}

		[AsyncStateMachine(typeof(_003CAwaitViewInitThenInit_003Ed__8))]
		private UniTask AwaitViewInitThenInit()
		{
			return default;
		}

		public void ActivateBaby()
		{
		}
	}
}
