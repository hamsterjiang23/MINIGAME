using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.DialogSystem;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Updatable;
using _Code.Player;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Player
{
	public sealed class PlayerServiceMorning : ASavableClass<PlayerServiceSaveData>, IPlayerService, IUpdateable, IDisposable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInit_003Ed__47 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

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
		private struct _003CLookAtWithZoom_003Ed__50 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public PlayerServiceMorning _003C_003E4__this;

			public Transform lookAtPos;

			public float duration;

			public float fov;

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
		private struct _003CMoveXZ_003Ed__51 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public PlayerServiceMorning _003C_003E4__this;

			public Vector3 position;

			public float duration;

			private DOTweenAsyncExtensions.TweenAwaiter _003C_003Eu__1;

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

		private PlayerServiceSaveData _saveData;

		private StartPoint _afterSaveStartPoint;

		private readonly PlayerInstance _playerInstance;

		private readonly ICursorController _cursorController;

		private readonly IDayNightController _dayNightController;

		private readonly IDialogManager _dialogManager;

		private float _lookAtTime;

		private float _lookAtDuration;

		private Vector3 _lookPoint;

		private Quaternion _lookAtStartRotation;

		private bool _isLookingAt;

		private int _immovableCounter;

		private float? _baseFov;

		private readonly CommonEnumEventus _commonEnumEventus;

		private readonly IDataModelService _dataModelService;

		public PlayerSigns Signs { get; private set; }

		public bool CanLookAround { get; private set; }

		public bool CanMove { get; private set; }

		public bool IsInRoom { get; private set; }

		public Vector3 Position => default;

		public bool IsCrouched
		{
			set
			{
			}
		}

		public Vector3 LookDirection => default;

		public IUpdateable Updateable => null;

		public bool IsMouseEnabled { get; set; }

		public PlayerServiceMorning(IPlayerViewProvider playerViewProvider, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, IDataModelService dataModelService, INotAHumanSoundService soundService, IDayNightController dayNightController, IDialogManager dialogManager, CommonEnumEventus commonEnumEventus)
		{
		}

		public void TeleportTo(StartPoint newLocationStartPoint)
		{
		}

		public void SetRunAvailability(bool isAvailable)
		{
		}

		public void ResetFov(float duration)
		{
		}

		[AsyncStateMachine(typeof(_003CInit_003Ed__47))]
		private UniTask Init()
		{
			return default;
		}

		public void MakeImmovable()
		{
		}

		public void MakeMovable()
		{
		}

		[AsyncStateMachine(typeof(_003CLookAtWithZoom_003Ed__50))]
		public UniTask LookAtWithZoom(Transform lookAtPos, float fov, float duration)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CMoveXZ_003Ed__51))]
		public UniTask MoveXZ(Vector3 position, float duration)
		{
			return default;
		}

		public void OnUpdateAction()
		{
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}
	}
}
