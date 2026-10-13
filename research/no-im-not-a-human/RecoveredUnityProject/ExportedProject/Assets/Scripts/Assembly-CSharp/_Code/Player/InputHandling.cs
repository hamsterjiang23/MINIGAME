using System;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;
using Steamworks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using _Code.Infrastructure.Cursor;

namespace _Code.Player
{
	public class InputHandling : MonoBehaviour
	{
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

		private readonly string[] _dualShockFingerprints = new string[4] { "dualshockgamepad", "dualshock", "playstation", "dualsense" };

		private readonly string[] _xboxFingerprints = new string[3] { "xinputcontroller", "xbox", "microsoft" };

		private readonly string[] _switchFingerprints = new string[2] { "switchprocontrollerhid", "nintendo" };

		private int _gamepadCursorActiveState;

		private bool _isBlockingNextClick;

		public EInputDevice CurrentDevice { get; private set; } = EInputDevice.MouseAndKeyboard;

		public Vector2 MoveInput => _playerInputActions.World.Move.ReadValue<Vector2>();

		public Vector2 LookInput => _playerInputActions.World.Look.ReadValue<Vector2>();

		public bool CrouchTriggered => _playerInputActions.World.Crouch.IsPressed();

		public bool CrouchPressed => _playerInputActions.World.Crouch.WasPerformedThisFrame();

		public bool RunTriggered => _playerInputActions.World.Run.IsPressed();

		public bool RunPerformedThisFrame => _playerInputActions.World.Run.WasPerformedThisFrame();

		public bool RunReleasedThisFrame => _playerInputActions.World.Run.WasReleasedThisFrame();

		public bool InteractTriggered => _playerInputActions.World.Interact.WasPerformedThisFrame();

		public bool PauseTriggered => _playerInputActions.World.Pause.WasPerformedThisFrame();

		public bool UIDialogSkipClicked => _playerInputActions.UI.DialogSkip.WasPerformedThisFrame();

		public bool UISubmitClicked => _playerInputActions.UI.Submit.WasPerformedThisFrame();

		public bool UISubmitDown => _playerInputActions.UI.Submit.WasPressedThisFrame();

		public bool UISubmitUp => _playerInputActions.UI.Submit.WasReleasedThisFrame();

		public bool UIExitTriggered => _playerInputActions.UI.Exit.WasPerformedThisFrame();

		public bool UIExitReleased => _playerInputActions.UI.Exit.WasReleasedThisFrame();

		public bool UIExitPauseTriggered => _playerInputActions.UI.PauseExit.WasPerformedThisFrame();

		public bool UITutorTriggered => _playerInputActions.UI.Tutor.WasPerformedThisFrame();

		public Vector2 UIRadioKnob => _playerInputActions.UI.RadioKnob.ReadValue<Vector2>();

		public bool UIRadioLeftHandle => _playerInputActions.UI.RadioLeftHandle.WasPerformedThisFrame();

		public bool UIRadioRightHandle => _playerInputActions.UI.RadioRightHandle.WasPerformedThisFrame();

		public bool UICancelClicked => _playerInputActions.UI.Cancel.WasPerformedThisFrame();

		public bool UISwitchTab => _playerInputActions.UI.ChangeTab.WasPerformedThisFrame();

		public Vector2 UIScroll => _playerInputActions.UI.Scroll.ReadValue<Vector2>();

		public bool LMBClicked => _playerInputActions.UI.LMB.WasPerformedThisFrame();

		public bool SkipVideoPressed => _playerInputActions.UI.SkipVideo.IsPressed();

		public bool SkipVideoClicked => _playerInputActions.UI.SkipVideo.WasPerformedThisFrame();

		public bool SkipVideoReleased => _playerInputActions.UI.SkipVideo.WasReleasedThisFrame();

		public bool SpeedUpPerformedThisFrame => _playerInputActions.UI.SpeedUp.WasPerformedThisFrame();

		public bool SpeedUpReleasedThisFrame => _playerInputActions.UI.SpeedUp.WasReleasedThisFrame();

		public float SpeedUpValue => _playerInputActions.UI.SpeedUpTrigger.ReadValue<float>();

		public bool AnyKeyClicked
		{
			get
			{
				if (CurrentDevice != EInputDevice.MouseAndKeyboard)
				{
					if (!Gamepad.current.buttonSouth.wasPressedThisFrame && !Gamepad.current.buttonWest.wasPressedThisFrame && !Gamepad.current.buttonEast.wasPressedThisFrame && !Gamepad.current.buttonNorth.wasPressedThisFrame && !Gamepad.current.leftShoulder.wasPressedThisFrame && !Gamepad.current.rightShoulder.wasPressedThisFrame && !Gamepad.current.leftTrigger.wasPressedThisFrame && !Gamepad.current.rightTrigger.wasPressedThisFrame && !Gamepad.current.leftStickButton.wasPressedThisFrame && !Gamepad.current.rightStickButton.wasPressedThisFrame && !Gamepad.current.startButton.wasPressedThisFrame)
					{
						return Gamepad.current.selectButton.wasPressedThisFrame;
					}
					return true;
				}
				return Keyboard.current.anyKey.wasPressedThisFrame;
			}
		}

		public event Action<EInputDevice> InputDeviceChanged;

		private void Awake()
		{
			_playerInputActions = new PlayerInputActions();
			_inUiCounter = ((_playerInput.defaultActionMap == "UI") ? 1 : 0);
		}

		private void OnEnable()
		{
			UpdateActiveScheme();
			_playerInput.onControlsChanged += OnControlsChanged;
		}

		private void OnDisable()
		{
			_playerInputActions.World.Disable();
			_playerInputActions.UI.Disable();
			_playerInput.onControlsChanged -= OnControlsChanged;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus)
			{
				_isBlockingNextClick = true;
				_playerInputActions.UI.LMB.Disable();
				_playerInputActions.UI.Submit.Disable();
				_playerInputActions.World.Interact.Disable();
				ResetFocusAfterFrame().Forget();
			}
		}

		private void Update()
		{
			if (_isBlockingNextClick && Input.GetMouseButton(0))
			{
				ResetFocus();
			}
		}

		private async UniTaskVoid ResetFocusAfterFrame()
		{
			await UniTask.DelayFrame(1);
			ResetFocus();
			_isBlockingNextClick = false;
		}

		private void ResetFocus()
		{
			_playerInputActions.UI.LMB.Enable();
			_playerInputActions.UI.Submit.Enable();
			_playerInputActions.World.Interact.Enable();
		}

		private void OnControlsChanged(PlayerInput input)
		{
			Debug.Log("OnControlsChanged " + input.currentControlScheme);
			if (input.currentControlScheme == "Gaypad")
			{
				CurrentDevice = EInputDevice.Gaypad;
				_gamepadCursor?.Enable();
			}
			else if (input.currentControlScheme == "Keyboard And Mouse")
			{
				CurrentDevice = EInputDevice.MouseAndKeyboard;
				_gamepadCursor?.Disable();
			}
			InputDeviceChanged?.Invoke(CurrentDevice);
		}

		public void SetGamepadCursorActiveState(bool active)
		{
			_gamepadCursorActiveState += (active ? 1 : (-1));
			_gamepadCursor?.SetActiveState(_gamepadCursorActiveState > 0);
			if (_gamepadCursorActiveState > 0)
			{
				_playerInput.neverAutoSwitchControlSchemes = true;
			}
			else
			{
				NeverSwitchSchemeReturnAsync().Forget();
			}
		}

		private async UniTaskVoid NeverSwitchSchemeReturnAsync()
		{
			await UniTask.DelayFrame(1);
			_playerInput.neverAutoSwitchControlSchemes = false;
		}

		private void UpdateActiveScheme()
		{
			if (_inUiCounter > 0)
			{
				_playerInputActions.UI.Enable();
				_playerInputActions.World.Disable();
			}
			else
			{
				_playerInputActions.World.Enable();
				_playerInputActions.UI.Disable();
			}
		}

		public void SetIsInUIState(bool isInUi)
		{
			if (isInUi)
			{
				_inUiCounter++;
			}
			else
			{
				_inUiCounter--;
			}
			_playerInput.SwitchCurrentActionMap((_inUiCounter > 0) ? "UI" : "World");
			UpdateActiveScheme();
		}

		public string GetKey(EControl control)
		{
			InputBinding inputBinding = (control switch
			{
				EControl.ExitRoom => _playerInputActions.UI.Exit, 
				EControl.CloseFridge => _playerInputActions.UI.Exit, 
				EControl.PutRadio => _playerInputActions.UI.Exit, 
				EControl.HangPhone => _playerInputActions.UI.Exit, 
				EControl.Leave => _playerInputActions.UI.Exit, 
				EControl.Run => _playerInputActions.World.Run, 
				EControl.ShowTutor => _playerInputActions.UI.Tutor, 
				EControl.Enter => _playerInputActions.World.Interact, 
				EControl.UseFridgeItem => _playerInputActions.UI.Submit, 
				EControl.SwitchToAM => _playerInputActions.UI.RadioLeftHandle, 
				EControl.SwitchToFM => _playerInputActions.UI.RadioRightHandle, 
				EControl.Listen => _playerInputActions.UI.DialogSkip, 
				EControl.Answer => _playerInputActions.UI.Submit, 
				EControl.SkipVideo => _playerInputActions.UI.SkipVideo, 
				EControl.RotateHandle => _playerInputActions.UI.RadioKnob, 
				EControl.SpeedUp => _playerInputActions.UI.SpeedUp, 
			}).bindings.FirstOrDefault((InputBinding x) => !x.isPartOfComposite && x.groups.Contains(_playerInput.currentControlScheme));
			if (!string.IsNullOrEmpty(inputBinding.effectivePath))
			{
				return InputControlPath.ToHumanReadableString(inputBinding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
			}
			return "?";
		}

		public EGaypadType GetGaypadType()
		{
			if (CurrentDevice == EInputDevice.MouseAndKeyboard)
			{
				return EGaypadType.Unknown;
			}
			string devicePath = Gamepad.current.path.ToLower();
			if (SteamManager.Initialized && SteamUtils.IsSteamRunningOnSteamDeck())
			{
				return EGaypadType.Steamdeck;
			}
			if (_xboxFingerprints.Any((string x) => devicePath.Contains(x)))
			{
				return EGaypadType.Xbox;
			}
			if (_dualShockFingerprints.Any((string x) => devicePath.Contains(x)))
			{
				return EGaypadType.PlayStation;
			}
			if (_switchFingerprints.Any((string x) => devicePath.Contains(x)))
			{
				return EGaypadType.Switch;
			}
			return EGaypadType.Unknown;
		}

		public void SetAutoSwitchSchemeState(bool isSwitch)
		{
			SetGamepadCursorActiveState(!isSwitch);
		}

		public void ForceSetNeverAutoSwitchControlScheme(bool canChange)
		{
			_playerInput.neverAutoSwitchControlSchemes = canChange;
		}

		public void ForceNeverSwitchScheme()
		{
			_playerInput.neverAutoSwitchControlSchemes = true;
		}

		public void BackNeveSwitchScheme()
		{
			if (_gamepadCursorActiveState > 0)
			{
				_playerInput.neverAutoSwitchControlSchemes = true;
			}
			else
			{
				NeverSwitchSchemeReturnAsync().Forget();
			}
		}

		public void SetMouseSensitivity(float value, bool isInvertY)
		{
			ReadOnlyArray<InputBinding> bindings = _playerInputActions.World.Look.bindings;
			InputBinding binding = bindings.FirstOrDefault((InputBinding x) => x.effectivePath.Contains("<Mouse>"));
			int index = -1;
			for (int num = 0; num < bindings.Count; num++)
			{
				if (bindings[num].effectivePath.Contains("<Mouse>"))
				{
					index = num;
					break;
				}
			}
			float num2 = value * 0.1f;
			float num3 = (isInvertY ? (-1f) : 1f);
			binding.processors = "scaleVector2(x=" + num2.ToString(CultureInfo.InvariantCulture) + ",y=" + (num2 * num3).ToString(CultureInfo.InvariantCulture) + ")";
			_playerInputActions.World.Look.ChangeBinding(index).To(binding);
		}

		public void SetGamepadSensitivity(float value, bool isInvertY)
		{
			ReadOnlyArray<InputBinding> bindings = _playerInputActions.World.Look.bindings;
			InputBinding binding = _playerInputActions.World.Look.bindings.FirstOrDefault((InputBinding x) => x.effectivePath.Contains("<Gamepad>"));
			int index = -1;
			for (int num = 0; num < bindings.Count; num++)
			{
				if (bindings[num].effectivePath.Contains("<Gamepad>"))
				{
					index = num;
					break;
				}
			}
			float num2 = (isInvertY ? (-1f) : 1f);
			binding.processors = "scaleVector2(x=" + value.ToString(CultureInfo.InvariantCulture) + ",y=" + (value * num2).ToString(CultureInfo.InvariantCulture) + ")";
			_playerInputActions.World.Look.ChangeBinding(index).To(binding);
		}

		public void SetGamepadRoomSensitivity(float value)
		{
			if (_gamepadCursor != null)
			{
				_gamepadCursor.ExtraSensitivity = value;
			}
		}

		public async UniTask ForceSetUI()
		{
			await UniTask.WaitUntil(() => _playerInputActions != null);
			_playerInputActions.UI.Enable();
		}

		public void Init(ICursorController cursorController)
		{
			cursorController.Locked -= OnLocked;
			cursorController.Locked += OnLocked;
			cursorController.Unlocked -= OnUnlocked;
			cursorController.Unlocked += OnUnlocked;
		}

		private void OnLocked()
		{
			ForceNeverSwitchScheme();
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			BackAutoswitchAfterFrame().Forget();
		}

		private void OnUnlocked(bool canGoThrough)
		{
			ForceNeverSwitchScheme();
			Cursor.lockState = ((!canGoThrough) ? CursorLockMode.Confined : CursorLockMode.None);
			Cursor.visible = true;
			BackAutoswitchAfterFrame().Forget();
		}

		private async UniTaskVoid BackAutoswitchAfterFrame()
		{
			await UniTask.DelayFrame(1);
			BackNeveSwitchScheme();
		}
	}
}
