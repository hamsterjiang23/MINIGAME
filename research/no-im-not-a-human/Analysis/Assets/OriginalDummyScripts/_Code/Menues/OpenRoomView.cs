using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using _Code.Infrastructure.ControlsViewer;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Menues
{
	public sealed class OpenRoomView : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CHide_003Ed__16 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public OpenRoomView _003C_003E4__this;

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
		private struct _003CShow_003Ed__15 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public OpenRoomView _003C_003E4__this;

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
		private Camera _camera;

		[SerializeField]
		private RTLTextMeshPro _subjectText;

		[SerializeField]
		private RTLTextMeshPro _actionText;

		[SerializeField]
		private AnimatedImage _animatedImage;

		[SerializeField]
		private Transform _crosshair;

		[SerializeField]
		private Transform _box;

		[SerializeField]
		private SerializedDictionary<ERaycastHintIcon, AnimationData> _icons;

		[SerializeField]
		private Image _fade;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private ControlView _controlView;

		private LocalizedString _roomNameLocalizationKey;

		private ERaycastHintIcon _icon;

		private Transform _currentTarget;

		private bool _isShowing;

		private void Update()
		{
		}

		[AsyncStateMachine(typeof(_003CShow_003Ed__15))]
		public UniTask Show()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHide_003Ed__16))]
		public UniTask Hide()
		{
			return default;
		}

		public OpenRoomView SetSubjectName(string subject)
		{
			return null;
		}

		public OpenRoomView SetActionName(string action)
		{
			return null;
		}

		public OpenRoomView SetTarget(Transform target)
		{
			return null;
		}

		public OpenRoomView SetIcon(ERaycastHintIcon icon)
		{
			return null;
		}

		public void SetFadedState(bool isFaded)
		{
		}

		public void UpdateIcon(EInputDevice device, EGaypadType gaypadType)
		{
		}
	}
}
