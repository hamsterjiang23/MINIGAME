using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.ControlsViewer;
using _Code.Menues.HUD.Animations;
using _Code.Player;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.Sound.Service;

namespace _Code.Menues.HUD
{
	public sealed class HUDView : MonoBehaviour, IHUDView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAnimateFallAsleep_003Ed__42 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDView _003C_003E4__this;

			public float[] randomSleepAlphas;

			public float[] randomSleepTimes;

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

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CFadeOverlay_003Ed__38 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDView _003C_003E4__this;

			public Color color;

			public float time;

			public Ease ease;

			public CancellationToken cancellationToken;

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
		private struct _003CHideControlsView_003Ed__48 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDView _003C_003E4__this;

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
		private struct _003CHideHint_003Ed__41 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDView _003C_003E4__this;

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
		private struct _003CPlayAnimation_003Ed__44 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public EHUDAnimation animationType;

			public HUDView _003C_003E4__this;

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
		private struct _003CSetupAndShowControlsView_003Ed__49 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public HUDView _003C_003E4__this;

			public EControlsList controlsList;

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
		private struct _003CShowGameSaved_003Ed__46 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDView _003C_003E4__this;

			private DOTweenAsyncExtensions.TweenAwaiter _003C_003Eu__1;

			private UniTask.Awaiter _003C_003Eu__2;

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
		private struct _003CShowGameSavedOnClose_003Ed__47 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDView _003C_003E4__this;

			private DOTweenAsyncExtensions.TweenAwaiter _003C_003Eu__1;

			private UniTask.Awaiter _003C_003Eu__2;

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
		private struct _003CShowHint_003Ed__40 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDView _003C_003E4__this;

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
		private struct _003CWakeUp_003Ed__43 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HUDView _003C_003E4__this;

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

		[SerializeField]
		private HUDAnimationData[] _animationsData;

		[SerializeField]
		private ActionsView _actions;

		[SerializeField]
		private OpenRoomView _openRoomView;

		[SerializeField]
		private ScreenResolutionBoxesAdjuster _screenResolutionBoxesAdjuster;

		[SerializeField]
		private AnimatedImage _animationLayer;

		[SerializeField]
		private CanvasGroup _gameSaved;

		[SerializeField]
		private ControlsListView _controlsList;

		[SerializeField]
		private ScreamersView _screamersView;

		[SerializeField]
		private ItemHintView _itemHintView;

		[SerializeField]
		private RawImage _dreamImage;

		private bool _isBreakingAnimation;

		private bool _isHintAvailable;

		private InputHandling _inputHandler;

		private EControlsList _lastControlsList;

		[CompilerGenerated]
		private Action m_OnShootAnimationCompleted;

		[field: SerializeField]
		public Image ColorOverlay { get; private set; }

		[field: SerializeField]
		public Gun Gun { get; private set; }

		[field: SerializeField]
		public Ending Ending { get; private set; }

		[field: SerializeField]
		public Camera PlayerCamera { get; private set; }

		public event Action OnShootAnimationCompleted
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

		public void Init(IInputHandlerProvider inputHandlerProvider, INotAHumanSoundService soundService)
		{
		}

		private void OnInputChanged(EInputDevice input)
		{
		}

		public void SetActionsCount(int count, bool isSkipAnim)
		{
		}

		public void SetFilledActionsCount(int count)
		{
		}

		public void SetActionsActiveState(bool isActive)
		{
		}

		[AsyncStateMachine(typeof(_003CFadeOverlay_003Ed__38))]
		public UniTask FadeOverlay(Color color, float time, Ease ease, CancellationToken cancellationToken)
		{
			return default;
		}

		public void SetHintData(string subject, string action, Transform target, ERaycastHintIcon icon)
		{
		}

		[AsyncStateMachine(typeof(_003CShowHint_003Ed__40))]
		public UniTask ShowHint()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHideHint_003Ed__41))]
		public UniTask HideHint()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CAnimateFallAsleep_003Ed__42))]
		public UniTask AnimateFallAsleep(float[] randomSleepAlphas, float[] randomSleepTimes)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CWakeUp_003Ed__43))]
		public UniTask WakeUp()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CPlayAnimation_003Ed__44))]
		public UniTask PlayAnimation(EHUDAnimation animationType)
		{
			return default;
		}

		public void AnimateShootOverlay()
		{
		}

		[AsyncStateMachine(typeof(_003CShowGameSaved_003Ed__46))]
		public UniTask ShowGameSaved()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CShowGameSavedOnClose_003Ed__47))]
		public UniTask ShowGameSavedOnClose()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHideControlsView_003Ed__48))]
		public UniTask HideControlsView()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CSetupAndShowControlsView_003Ed__49))]
		public UniTaskVoid SetupAndShowControlsView(EControlsList controlsList)
		{
			return default;
		}

		public void UpdateControlsList()
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

		public EControlsList GetLastControlsList()
		{
			return EControlsList.None;
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
	}
}
