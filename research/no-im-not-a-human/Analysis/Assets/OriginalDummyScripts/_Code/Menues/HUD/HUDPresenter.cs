using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.ViewProvider;
using _Code.Menues.HUD.Animations;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Menues.HUD
{
	public sealed class HUDPresenter : IHUDPresenter
	{
		[CompilerGenerated]
		private sealed class _003C_003Ec__DisplayClass31_0
		{
			public HUDPresenter _003C_003E4__this;

			public bool showEndingAfter;

			internal void _003CDeath_003Eb__0(bool dialog, bool subtitle)
			{
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CDeath_003Ed__31 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDPresenter _003C_003E4__this;

			public bool showEndingAfter;

			public Camera camera;

			private _003C_003Ec__DisplayClass31_0 _003C_003E8__1;

			public string cause;

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
		private struct _003CFadeIn_003Ed__27 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDPresenter _003C_003E4__this;

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
		private struct _003CFadeOut_003Ed__28 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDPresenter _003C_003E4__this;

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
		private struct _003CHideHint_003Ed__34 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDPresenter _003C_003E4__this;

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
		private struct _003CPlayAnimation_003Ed__26 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDPresenter _003C_003E4__this;

			public EHUDAnimation animation;

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
		private struct _003CShowGameSaved_003Ed__40 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDPresenter _003C_003E4__this;

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
		private struct _003CShowGameSavedOnClose_003Ed__41 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDPresenter _003C_003E4__this;

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
		private struct _003CShowHint_003Ed__33 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDPresenter _003C_003E4__this;

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

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CWakeUp_003Ed__37 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDPresenter _003C_003E4__this;

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

		private IDialogManager _dialogManager;

		private readonly ICharactersManager _charactersManager;

		private EControlsList _lastControlsList;

		private readonly IConsumablesController _consumablesController;

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

		public HUDPresenter(IViewProvider view, IDialogManager dialogManager, IInputHandlerProvider inputHandlerProvider, INotAHumanSoundService soundService, ICharactersManager charactersManager, IConsumablesController consumablesController, CommonEnumEventus enumEventus)
		{
		}

		private void SubscribeEvents()
		{
		}

		private void OnFakeShot()
		{
		}

		private void OnPlayedAnimation(EHUDAnimation animation)
		{
		}

		private void OnArmpitsWashed()
		{
		}

		private void OnButtonsDialogLineShowed()
		{
		}

		private void OnBaseDialogLineShowed()
		{
		}

		private void OnDialogStarted()
		{
		}

		private void OnDialogEnded(bool isEndedDialog, bool isEndedSubtitle)
		{
		}

		private void OnFadeIn(float duration)
		{
		}

		private void OnFadeOut(float duration)
		{
		}

		private void OnFedCat()
		{
		}

		private void OnGivenPovistka()
		{
		}

		private void OnShowedAura()
		{
		}

		private void OnDeadBySuper(string cause, Camera camera)
		{
		}

		[AsyncStateMachine(typeof(_003CPlayAnimation_003Ed__26))]
		public UniTask PlayAnimation(EHUDAnimation animation)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CFadeIn_003Ed__27))]
		public UniTask FadeIn(float appearingTime, CancellationToken token = default(CancellationToken))
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CFadeOut_003Ed__28))]
		public UniTask FadeOut(float appearingTime, CancellationToken token = default(CancellationToken))
		{
			return default;
		}

		public void InitActionsCount(bool isSkipAnim = false)
		{
		}

		public void SetActionsCountActive(bool hasActionsCount)
		{
		}

		[AsyncStateMachine(typeof(_003CDeath_003Ed__31))]
		public UniTask Death(string cause, bool showEndingAfter = false, Camera camera = null)
		{
			return default;
		}

		private void DeathDialogEnded(bool showEndingAfter)
		{
		}

		[AsyncStateMachine(typeof(_003CShowHint_003Ed__33))]
		public UniTask ShowHint(string subject, string action, Transform target, ERaycastHintIcon icon)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHideHint_003Ed__34))]
		public UniTask HideHint()
		{
			return default;
		}

		public void SetHintAvailability(bool isAvailable)
		{
		}

		public void AnimateFallAsleep(out float[] randomSleepTimes)
		{
			randomSleepTimes = null;
		}

		[AsyncStateMachine(typeof(_003CWakeUp_003Ed__37))]
		public UniTask WakeUp()
		{
			return default;
		}

		public void GunShow()
		{
		}

		public void GunHide()
		{
		}

		[AsyncStateMachine(typeof(_003CShowGameSaved_003Ed__40))]
		public UniTask ShowGameSaved()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CShowGameSavedOnClose_003Ed__41))]
		public UniTask ShowGameSavedOnClose()
		{
			return default;
		}

		private void GunShot()
		{
		}

		private void OnShotAnimationCompleted()
		{
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
	}
}
