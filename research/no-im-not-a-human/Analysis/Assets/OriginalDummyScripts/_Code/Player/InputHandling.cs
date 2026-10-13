using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using _Code.Infrastructure.Cursor;

namespace _Code.Player
{
	public class InputHandling : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CBackAutoswitchAfterFrame_003Ed__107 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public InputHandling _003C_003E4__this;

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
		private struct _003CForceSetUI_003Ed__103 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public InputHandling _003C_003E4__this;

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
		private struct _003CNeverSwitchSchemeReturnAsync_003Ed__91 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public InputHandling _003C_003E4__this;

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
		private struct _003CResetFocusAfterFrame_003Ed__87 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public InputHandling _003C_003E4__this;

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
		private PlayerInput _playerInput;

		[SerializeField]
		private GamepadCursor _gamepadCursor;

		private PlayerInputActions _playerInputActions;

		private const string WorldScheme = "World";

		private const string UIScheme = "UI";

		private int _inUiCounter;

		private const string GAYPAD_CONTROL_SCHEME = "Gaypad";

		private const string KEYBOARD_MOUSE_CONTROL_SCHEME = "Keyboard And Mouse";

		private readonly string[] _dualShockFingerprints;

		private readonly string[] _xboxFingerprints;

		private readonly string[] _switchFingerprints;

		private int _gamepadCursorActiveState;

		private bool _isBlockingNextClick;

		[CompilerGenerated]
		private Action<EInputDevice> m_InputDeviceChanged;

		public EInputDevice CurrentDevice { get; private set; }

		public Vector2 MoveInput => default;

		public Vector2 LookInput => default;

		public bool CrouchTriggered => false;

		public bool CrouchPressed => false;

		public bool RunTriggered => false;

		public bool RunPerformedThisFrame => false;

		public bool RunReleasedThisFrame => false;

		public bool InteractTriggered => false;

		public bool PauseTriggered => false;

		public bool UIDialogSkipClicked => false;

		public bool UISubmitClicked => false;

		public bool UISubmitDown => false;

		public bool UISubmitUp => false;

		public bool UIExitTriggered => false;

		public bool UIExitReleased => false;

		public bool UIExitPauseTriggered => false;

		public bool UITutorTriggered => false;

		public Vector2 UIRadioKnob => default;

		public bool UIRadioLeftHandle => false;

		public bool UIRadioRightHandle => false;

		public bool UICancelClicked => false;

		public bool UISwitchTab => false;

		public Vector2 UIScroll => default;

		public bool LMBClicked => false;

		public bool SkipVideoPressed => false;

		public bool SkipVideoClicked => false;

		public bool SkipVideoReleased => false;

		public bool SpeedUpPerformedThisFrame => false;

		public bool SpeedUpReleasedThisFrame => false;

		public float SpeedUpValue => 0f;

		public bool AnyKeyClicked => false;

		public event Action<EInputDevice> InputDeviceChanged
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

		private void Awake()
		{
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		private void OnApplicationFocus(bool hasFocus)
		{
		}

		private void Update()
		{
		}

		[AsyncStateMachine(typeof(_003CResetFocusAfterFrame_003Ed__87))]
		private UniTaskVoid ResetFocusAfterFrame()
		{
			return default;
		}

		private void ResetFocus()
		{
		}

		private void OnControlsChanged(PlayerInput input)
		{
		}

		public void SetGamepadCursorActiveState(bool active)
		{
		}

		[AsyncStateMachine(typeof(_003CNeverSwitchSchemeReturnAsync_003Ed__91))]
		private UniTaskVoid NeverSwitchSchemeReturnAsync()
		{
			return default;
		}

		private void UpdateActiveScheme()
		{
		}

		public void SetIsInUIState(bool isInUi)
		{
		}

		public string GetKey(EControl control)
		{
			return null;
		}

		public EGaypadType GetGaypadType()
		{
			return EGaypadType.Unknown;
		}

		public void SetAutoSwitchSchemeState(bool isSwitch)
		{
		}

		public void ForceSetNeverAutoSwitchControlScheme(bool canChange)
		{
		}

		public void ForceNeverSwitchScheme()
		{
		}

		public void BackNeveSwitchScheme()
		{
		}

		public void SetMouseSensitivity(float value, bool isInvertY)
		{
		}

		public void SetGamepadSensitivity(float value, bool isInvertY)
		{
		}

		public void SetGamepadRoomSensitivity(float value)
		{
		}

		[AsyncStateMachine(typeof(_003CForceSetUI_003Ed__103))]
		public UniTask ForceSetUI()
		{
			return default;
		}

		public void Init(ICursorController cursorController)
		{
		}

		private void OnLocked()
		{
		}

		private void OnUnlocked(bool canGoThrough)
		{
		}

		[AsyncStateMachine(typeof(_003CBackAutoswitchAfterFrame_003Ed__107))]
		private UniTaskVoid BackAutoswitchAfterFrame()
		{
			return default;
		}
	}
}
