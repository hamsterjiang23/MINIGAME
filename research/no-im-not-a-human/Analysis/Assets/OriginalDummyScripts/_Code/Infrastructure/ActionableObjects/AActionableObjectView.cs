using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.Localization;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure.Windows;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.EditorWindows;
using _Scripts.Raycast;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.ActionableObjects
{
	[TabsNames(new string[] { "GameObjects", "Float settings", "Other settings", "Drift", "Wrong open" })]
	public abstract class AActionableObjectView : MonoBehaviour, IActionableObjectView, IUpdateable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CStartLooking_003Ed__55 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public AActionableObjectView _003C_003E4__this;

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
		private struct _003CStopLooking_003Ed__56 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public AActionableObjectView _003C_003E4__this;

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

		[TabIndex(0)]
		[SerializeField]
		protected Transform _lookAtPos;

		[TabIndex(0)]
		[SerializeField]
		private Transform _standingPos;

		[TabIndex(0)]
		[SerializeField]
		private ARaycastTarget _raycastTarget;

		[TabIndex(0)]
		[SerializeField]
		private float _awaitedPlayerLookAngle;

		[TabIndex(1)]
		[SerializeField]
		protected float _transitionTime;

		[TabIndex(1)]
		[SerializeField]
		private float _fov;

		[TabIndex(1)]
		[SerializeField]
		[Range(0.1f, 5f)]
		private float _fovChangeSlowerCoefIn;

		[TabIndex(1)]
		[SerializeField]
		[Range(0.1f, 5f)]
		private float _fovChangeSlowerCoefOut;

		[TabIndex(1)]
		[SerializeField]
		private float _moveToStandingPosSpeed;

		[TabIndex(2)]
		[SerializeField]
		private bool _isWaitingActionBeforeChangingFOV;

		[TabIndex(2)]
		[SerializeField]
		private ETimeOfDay[] ProhibitedToUseDaytime;

		[TabIndex(2)]
		[SerializeField]
		private int[] ProhibitedDays;

		[TabIndex(2)]
		[SerializeField]
		private bool _isAwaitingMoveToStandingPos;

		[TabIndex(3)]
		[SerializeField]
		private bool _canBeOpenedByEKey;

		[TabIndex(3)]
		[SerializeField]
		[Range(0f, 1f)]
		private float _eKeyChance;

		[TabIndex(4)]
		[SerializeField]
		[SearchableEnum]
		protected ESoundSource _soundSource;

		[TabIndex(4)]
		[SerializeField]
		[SearchableEnum]
		private ESound _cantOpenSound;

		[TabIndex(4)]
		[SerializeField]
		private LocalizedString _cantOpenCauseOfDay;

		[TabIndex(4)]
		[SerializeField]
		private LocalizedString _cantOpenCauseOfTimeOfDay;

		private bool _canLeave;

		protected IHUDPresenter HUDPresenter;

		protected IDayNightController DayNightController;

		protected IRoomsManager RoomsManager;

		protected IGameEventsManager GameEventsManager;

		protected ICloseUpsController CloseUpsController;

		protected IPlayerService PlayerService;

		protected ICursorController CursorController;

		protected IWindowsManager WindowsManager;

		protected IPauseController PauseController;

		protected INotAHumanSoundService SoundService;

		protected InputHandling InputHandler;

		protected bool IsLooking;

		private bool _isAnimating;

		private bool _isEKeyActivated;

		private bool _isLocked;

		protected IDialogManager _dialogManager;

		private ESound[] _tryOpenSounds;

		protected abstract Func<UniTask> ExtraActionIn { get; }

		protected abstract Func<UniTask> ExtraActionOut { get; }

		protected abstract Func<UniTask> ExtraActionInE { get; }

		protected abstract Func<UniTask> ExtraActionOutE { get; }

		protected abstract bool CanLeave { get; }

		public bool CanShowHint => false;

		public float AwaitedPlayerLookAngle => 0f;

		public void InitModules(IDialogManager dialogManager)
		{
		}

		public virtual void Init(IHUDPresenter hudPresenter, IDayNightController dayNightController, IRoomsManager roomsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
		{
		}

		public void TryLeave()
		{
		}

		public void SetLockedState(bool isLocked)
		{
		}

		[AsyncStateMachine(typeof(_003CStartLooking_003Ed__55))]
		private UniTaskVoid StartLooking()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CStopLooking_003Ed__56))]
		private UniTaskVoid StopLooking()
		{
			return default;
		}

		protected virtual void ExtraActionBeforeIn()
		{
		}

		protected virtual void ExtraActionBeforeOut()
		{
		}

		public void OnUpdateAction()
		{
		}

		public void Update()
		{
		}

		private void KeyAvailabilityCheck()
		{
		}

		private void Act()
		{
		}

		private void TryToOpenInWrongTimeOfDay()
		{
		}

		private void TryToOpenInWrongDay()
		{
		}

		private void OnDrawGizmos()
		{
		}

		public void Disable()
		{
		}

		public void Enable()
		{
		}
	}
}
