using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.ViewProvider;
using _Code.Menues.HUD.Animations;
using _Code.Player;

namespace _Code.Menues.HUD
{
	public sealed class MockHudPresenter : IHUDPresenter
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CFadeIn_003Ed__17 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public MockHudPresenter _003C_003E4__this;

			public float appearingTime;

			public CancellationToken token;

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
		private struct _003CFadeOut_003Ed__18 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public MockHudPresenter _003C_003E4__this;

			public float disappearingTime;

			public CancellationToken token;

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
		private struct _003CHideHint_003Ed__7 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public MockHudPresenter _003C_003E4__this;

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
		private struct _003CShowHint_003Ed__6 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public MockHudPresenter _003C_003E4__this;

			public string subject;

			public string action;

			public Transform target;

			public ERaycastHintIcon icon;

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

		private IHUDView _view;

		[CompilerGenerated]
		private Func<int> m_GetMaxDayActions;

		[CompilerGenerated]
		private Func<int> m_GetDayActions;

		public event Func<int> GetMaxDayActions
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

		public event Func<int> GetDayActions
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

		public MockHudPresenter(IViewProvider viewProvider)
		{
		}

		public void InitActionsCount()
		{
		}

		public void InitActionsCount(bool isSkipAnim = false)
		{
		}

		public void SetActionsCountActive(bool hasActionsCount)
		{
		}

		public UniTask Death(string cause, bool showEndingAfter = false, Camera camera = null)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CShowHint_003Ed__6))]
		public UniTask ShowHint(string subject, string action, Transform target, ERaycastHintIcon icon)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHideHint_003Ed__7))]
		public UniTask HideHint()
		{
			return default;
		}

		public void AnimateFallAsleep(out float[] randomSleepTimes)
		{
			randomSleepTimes = null;
		}

		public UniTask WakeUp()
		{
			return default;
		}

		public UniTask PlayAnimation(EHUDAnimation animation)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CFadeIn_003Ed__17))]
		public UniTask FadeIn(float appearingTime, CancellationToken token = default(CancellationToken))
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CFadeOut_003Ed__18))]
		public UniTask FadeOut(float disappearingTime, CancellationToken token = default(CancellationToken))
		{
			return default;
		}

		public void GunShow()
		{
		}

		public void GunHide()
		{
		}

		public UniTask ShowGameSaved()
		{
			return default;
		}

		public void HideControlsView()
		{
		}

		public void SetupAndShowControlsView(EControlsList controlsList)
		{
		}

		public void SetControlsAvailability(EControl control, bool isAvailable)
		{
		}

		public void SetHintAvailability(bool isAvailable)
		{
		}

		public void ShowScreamer()
		{
		}

		public void SetHintFadedState(bool isFaded)
		{
		}

		public void ShowItemReceivedHint(EConsumable item, int count)
		{
		}

		public void ShowItemGivenAwayHint(EConsumable item, int count)
		{
		}

		public void EnableDream()
		{
		}

		public void DisableDream()
		{
		}

		public void HideAction()
		{
		}

		public UniTask ShowGameSavedOnClose()
		{
			return default;
		}
	}
}
