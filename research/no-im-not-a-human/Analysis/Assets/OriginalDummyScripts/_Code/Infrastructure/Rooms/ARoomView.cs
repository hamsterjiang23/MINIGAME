using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Menues.HUD;
using _Code.Rooms;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Rooms
{
	public abstract class ARoomView : MonoBehaviour, IRoomView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CBlink_003Ed__68 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public ARoomView _003C_003E4__this;

			public Action action;

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
		private struct _003CEnableObjectsAsync_003Ed__77 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public ARoomView _003C_003E4__this;

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
		private struct _003COnDialogHiddenAsync_003Ed__65 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public ARoomView _003C_003E4__this;

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
		private struct _003COnNarrativeObjectEndedAsync_003Ed__54 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public ARoomView _003C_003E4__this;

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
		private struct _003CStopWhispering_003Ed__57 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public ARoomView _003C_003E4__this;

			public float time;

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
		protected UIButton[] _uiButtons;

		[Header("Door")]
		[SerializeField]
		private Transform _door;

		[SerializeField]
		private Transform _brokenDoor;

		[SerializeField]
		private DoorTrigger _doorTrigger;

		private Action ActionOnOpen;

		protected CharacterRoomObjectView _dialogCompanion;

		protected IDialogManager DialogManager;

		protected IDayNightController DayNightController;

		protected ICloseUpsController CloseUpsController;

		protected INotAHumanSoundService SoundService;

		protected IHUDPresenter HudPresenter;

		private CancellationTokenSource _cancellationTokenSource;

		private bool _isLeaving;

		[field: SerializeField]
		public float CameraDistance { get; private set; }

		[field: SerializeField]
		public float DialogCameraDistance { get; private set; }

		[field: SerializeField]
		public CharacterRoomObjectView[] CharacterViews { get; private set; }

		[field: SerializeField]
		public ObjectRoomObjectView[] ObjectsViews { get; private set; }

		[field: SerializeField]
		public NarrativeRoomObject[] NarrativeObjects { get; private set; }

		[field: SerializeField]
		public AudioSource WhispersAudioSource { get; private set; }

		[field: SerializeField]
		public Camera LinkedCamera { get; private set; }

		public bool HasDialogNow { get; protected set; }

		public bool IsDead { get; private set; }

		public Vector3 LookDirection => default;

		public virtual void Init(IDialogManager dialogManager, IDayNightController dayNightController, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IHUDPresenter hudPresenter, IDataModelService dataModelService)
		{
		}

		private void OnNarrativeObjectStarted()
		{
		}

		private void OnNarrativeObjectEnded()
		{
		}

		[AsyncStateMachine(typeof(_003COnNarrativeObjectEndedAsync_003Ed__54))]
		private UniTaskVoid OnNarrativeObjectEndedAsync()
		{
			return default;
		}

		public void SetDialogCompanion(CharacterRoomObjectView character)
		{
		}

		public void SetOnOpenAction(Action action)
		{
		}

		[AsyncStateMachine(typeof(_003CStopWhispering_003Ed__57))]
		public UniTaskVoid StopWhispering(float time)
		{
			return default;
		}

		public void StartWhispering(AudioClip sound, float time)
		{
		}

		protected virtual void OnEnable()
		{
		}

		protected virtual void OnDisable()
		{
		}

		private void OnDead(string cause, Camera cam)
		{
		}

		private void OnSubtitleShowed()
		{
		}

		protected virtual void OnDialogShowed()
		{
		}

		protected virtual void OnDialogHidden(bool isEndedDialog, bool isEndedSubtitle)
		{
		}

		[AsyncStateMachine(typeof(_003COnDialogHiddenAsync_003Ed__65))]
		private UniTaskVoid OnDialogHiddenAsync()
		{
			return default;
		}

		public virtual void Enter()
		{
		}

		public virtual void Leave()
		{
		}

		[AsyncStateMachine(typeof(_003CBlink_003Ed__68))]
		public UniTaskVoid Blink(Action action)
		{
			return default;
		}

		public void StartLeavingRoom()
		{
		}

		public void StopLeavingRoom()
		{
		}

		private void OnDestroy()
		{
		}

		public void ChangeCharacterPose(ECharacterType character, ERoomPeopleState pose)
		{
		}

		public void EnableObjects()
		{
		}

		public void DisableObjects()
		{
		}

		public void BreakDoor()
		{
		}

		public void RepairDoor()
		{
		}

		[AsyncStateMachine(typeof(_003CEnableObjectsAsync_003Ed__77))]
		private UniTaskVoid EnableObjectsAsync()
		{
			return default;
		}

		public Dictionary<string, int> GetUIBUttonsStates()
		{
			return null;
		}

		public void SetUIButtonState(Dictionary<string, int> buttonStates)
		{
		}
	}
}
