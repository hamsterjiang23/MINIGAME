using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Zenject;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Menues.HUD;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Rooms
{
	public sealed class EntranceRoom : ARoom, ITickable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnLoad_003Ed__24 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public EntranceRoom _003C_003E4__this;

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

		private readonly IDialogManager _dialogManager;

		private bool _isSomeoneKnockingNow;

		private ESound _knockSound;

		private float _lastKnockTime;

		private float _knockDelay;

		private KnockTimeBalanceEnvironment _environment;

		private bool _hasWatched;

		private IDataModelService _dataModelService;

		private ECharacterType _currentCharacter;

		private ICharactersManager _charactersManager;

		private float _noiseLevel;

		public override ERoom RoomType => ERoom.Kitchen;

		public EntranceRoom(IDayNightController dayNightController, IOtherGameSODataProvider otherGameSoDataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, ICharactersManager charactersManager, IDataModelService dataModelService, IStateObjectController stateObjectController)
			: base(null, null, null, null, null, null, null, null, null, null, null, null)
		{
		}

		private void OnDayChanged(int day)
		{
		}

		public void UpdatePictureForDay(int day)
		{
		}

		public void InitModules(IDataModelService dataModelService, ICharactersManager charactersManager)
		{
		}

		private new string GetDialogName(CharacterSOData character)
		{
			return null;
		}

		public void ShowFullSizeCharacter(CharacterSOData character, string conditionName)
		{
		}

		public void CharacerWillCumSoon()
		{
		}

		private void OnDialogStarted()
		{
		}

		private void OnDialogEnded(bool endedDialog, bool endedSubtitle)
		{
		}

		private void OnDialogEnded2(bool endedDialog, bool endedSubtitle)
		{
		}

		public void Tick()
		{
		}

		[AsyncStateMachine(typeof(_003COnLoad_003Ed__24))]
		public UniTaskVoid OnLoad()
		{
			return default;
		}

		protected override void EnterInner()
		{
		}
	}
}
