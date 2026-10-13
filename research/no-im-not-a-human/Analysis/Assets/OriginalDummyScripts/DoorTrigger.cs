using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Rooms;
using _Scripts.Services.Sound.Service;

public sealed class DoorTrigger : AActionableObjectView
{
	[StructLayout((LayoutKind)3)]
	[CompilerGenerated]
	private struct _003CSlightlyOpenDoorAsync_003Ed__32 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

		public DoorTrigger _003C_003E4__this;

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

	[SerializeField]
	private Transform _doorPivot;

	[SerializeField]
	private float _openAngle;

	[SerializeField]
	private float _slightOpenAngle;

	[SerializeField]
	private float _duration;

	[SerializeField]
	private ERoom _linkedRoom;

	[SerializeField]
	private Collider _doorCollider;

	[SerializeField]
	[Range(0.1f, 5f)]
	private float _closeSpeedModifier;

	private ESound[] _openSounds;

	private ESound[] _closeSounds;

	private float volumeMultiplier;

	private IRoom _room;

	private Vector3 _startRotation;

	private bool _isStarted;

	private WatcherManager _watcherManager;

	private IRoom Room => null;

	protected override bool CanLeave => false;

	protected override Func<UniTask> ExtraActionIn => null;

	protected override Func<UniTask> ExtraActionOut => null;

	protected override Func<UniTask> ExtraActionInE => null;

	protected override Func<UniTask> ExtraActionOutE => null;

	private void Awake()
	{
	}

	public override void Init(IHUDPresenter hudPresenter, IDayNightController dayNightController, IRoomsManager roomsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
	{
	}

	protected override void ExtraActionBeforeOut()
	{
	}

	protected override void ExtraActionBeforeIn()
	{
	}

	private void Start()
	{
	}

	public void SlightlyOpenDoor()
	{
	}

	[AsyncStateMachine(typeof(_003CSlightlyOpenDoorAsync_003Ed__32))]
	private UniTaskVoid SlightlyOpenDoorAsync()
	{
		return default;
	}
}
