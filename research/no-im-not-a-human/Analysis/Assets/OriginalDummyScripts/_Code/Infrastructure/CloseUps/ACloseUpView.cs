using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Updatable;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.CloseUps
{
	public abstract class ACloseUpView : MonoBehaviour, ICloseUpView, IUpdateable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CHide_003Ed__44 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ACloseUpView _003C_003E4__this;

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
		private struct _003CShow_003Ed__43 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ACloseUpView _003C_003E4__this;

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

		private const float APPEARING_TIME = 0.5f;

		protected IHUDPresenter HUDPresenter;

		protected ICursorController CursorController;

		protected IPlayerService PlayerService;

		protected INotAHumanSoundService SoundService;

		protected InputHandling InputHandler;

		protected bool IsHoldToClose;

		protected bool HasTutor;

		private bool _isHolding;

		private bool _isChangingState;

		private float _holdProgress;

		[CompilerGenerated]
		private Action m_Entered;

		[CompilerGenerated]
		private Action m_Left;

		[CompilerGenerated]
		private Action<float> m_HoldProgressChanged;

		[CompilerGenerated]
		private Action m_TutorSwitched;

		[CompilerGenerated]
		private Action m_TutorLeft;

		protected virtual bool IsUseFade => false;

		public bool IsEntered { get; private set; }

		protected virtual float HoldProgressTarget { get; }

		protected virtual bool CanHold { get; set; }

		protected bool CanLeave { get; set; }

		public abstract IUpdateable[] Updateables { get; }

		public event Action Entered
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

		public event Action Left
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

		protected event Action<float> HoldProgressChanged
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

		protected event Action TutorSwitched
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

		protected event Action TutorLeft
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

		[AsyncStateMachine(typeof(_003CShow_003Ed__43))]
		public virtual UniTask Show()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHide_003Ed__44))]
		public virtual UniTask Hide()
		{
			return default;
		}

		protected virtual void OnStartShow()
		{
		}

		protected virtual void OnShown()
		{
		}

		protected virtual void OnStartHide()
		{
		}

		protected virtual void OnHidden()
		{
		}

		public virtual void Init()
		{
		}

		public void InitModules(IHUDPresenter hudPresenter, IPlayerService playerService, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, INotAHumanSoundService soundService)
		{
		}

		public virtual void OnUpdateAction()
		{
		}
	}
}
