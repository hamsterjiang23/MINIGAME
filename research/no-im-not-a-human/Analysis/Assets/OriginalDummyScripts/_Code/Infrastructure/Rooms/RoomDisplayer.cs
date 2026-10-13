using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Infrastructure.Cursor;
using _Code.Player;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms
{
	public sealed class RoomDisplayer : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CCloseFade_003Ed__33 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public RoomDisplayer _003C_003E4__this;

			public float fadeDuration;

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
		private struct _003COpenFadeGlobal_003Ed__32 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public RoomDisplayer _003C_003E4__this;

			public float fadeDuration;

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
		private struct _003COpenFadeLocal_003Ed__31 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public RoomDisplayer _003C_003E4__this;

			public float fadeDuration;

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
		private GraphicRaycaster _raycaster;

		[SerializeField]
		private EventSystem _eventSystem;

		[SerializeField]
		private Camera _camera;

		[SerializeField]
		private GameObject _backImage;

		private const float FADE_DURATION = 0.5f;

		public const float ANGLE_TO_USE_GLOBAL_OVERLAY = 40f;

		private bool _wasOpenedWithGlobalOverlay;

		private bool _isOpened;

		private bool _isHoveredObject;

		private ICursorController _cursorController;

		private bool _isBlocked;

		private bool _areClicksLocked;

		private InputHandling _inputHandler;

		private UIButton _selectedButton;

		[field: SerializeField]
		public Canvas Canvas { get; private set; }

		[field: SerializeField]
		public Canvas DialogCanvas { get; private set; }

		[field: SerializeField]
		public Image LocalOverlay { get; private set; }

		[field: SerializeField]
		public Image GlobalOverlay { get; private set; }

		public void InitModules(ICursorController cursorController, IInputHandlerProvider inputHandlerProvider)
		{
		}

		[AsyncStateMachine(typeof(_003COpenFadeLocal_003Ed__31))]
		public UniTask OpenFadeLocal(float fadeDuration = 0.5f)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003COpenFadeGlobal_003Ed__32))]
		public UniTask OpenFadeGlobal(float fadeDuration = 0.5f)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CCloseFade_003Ed__33))]
		public UniTask CloseFade(float fadeDuration = 0.5f)
		{
			return default;
		}

		public void SetData(Camera roomLinkedCamera, float roomCameraDistance, float dialogCanvasDistance)
		{
		}

		private void Update()
		{
		}

		private void Unhover()
		{
		}

		private void Hover()
		{
		}

		public void SetBlockerActive(bool isActive)
		{
		}

		public void LockClicks()
		{
		}

		public void UnlockClicks()
		{
		}
	}
}
