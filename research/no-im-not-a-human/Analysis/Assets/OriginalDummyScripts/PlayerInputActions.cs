using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerInputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct WorldActions
	{
		private PlayerInputActions m_Wrapper;

		public InputAction Move => null;

		public InputAction Look => null;

		public InputAction Crouch => null;

		public InputAction Interact => null;

		public InputAction Pause => null;

		public InputAction Run => null;

		public bool enabled => false;

		public WorldActions(PlayerInputActions wrapper)
		{
			m_Wrapper = null;
		}

		public InputActionMap Get()
		{
			return null;
		}

		public void Enable()
		{
		}

		public void Disable()
		{
		}

		public static implicit operator InputActionMap(WorldActions set)
		{
			return null;
		}

		public void AddCallbacks(IWorldActions instance)
		{
		}

		private void UnregisterCallbacks(IWorldActions instance)
		{
		}

		public void RemoveCallbacks(IWorldActions instance)
		{
		}

		public void SetCallbacks(IWorldActions instance)
		{
		}
	}

	public struct UIActions
	{
		private PlayerInputActions m_Wrapper;

		public InputAction DialogSkip => null;

		public InputAction Exit => null;

		public InputAction Point => null;

		public InputAction Cancel => null;

		public InputAction Scroll => null;

		public InputAction PauseExit => null;

		public InputAction Tutor => null;

		public InputAction Navigate => null;

		public InputAction Select => null;

		public InputAction Submit => null;

		public InputAction RadioKnob => null;

		public InputAction RadioLeftHandle => null;

		public InputAction RadioRightHandle => null;

		public InputAction LMB => null;

		public InputAction SkipVideo => null;

		public InputAction ChangeTab => null;

		public InputAction SpeedUp => null;

		public InputAction SpeedUpTrigger => null;

		public bool enabled => false;

		public UIActions(PlayerInputActions wrapper)
		{
			m_Wrapper = null;
		}

		public InputActionMap Get()
		{
			return null;
		}

		public void Enable()
		{
		}

		public void Disable()
		{
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return null;
		}

		public void AddCallbacks(IUIActions instance)
		{
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
		}

		public void RemoveCallbacks(IUIActions instance)
		{
		}

		public void SetCallbacks(IUIActions instance)
		{
		}
	}

	public interface IWorldActions
	{
		void OnMove(InputAction.CallbackContext context);

		void OnLook(InputAction.CallbackContext context);

		void OnCrouch(InputAction.CallbackContext context);

		void OnInteract(InputAction.CallbackContext context);

		void OnPause(InputAction.CallbackContext context);

		void OnRun(InputAction.CallbackContext context);
	}

	public interface IUIActions
	{
		void OnDialogSkip(InputAction.CallbackContext context);

		void OnExit(InputAction.CallbackContext context);

		void OnPoint(InputAction.CallbackContext context);

		void OnCancel(InputAction.CallbackContext context);

		void OnScroll(InputAction.CallbackContext context);

		void OnPauseExit(InputAction.CallbackContext context);

		void OnTutor(InputAction.CallbackContext context);

		void OnNavigate(InputAction.CallbackContext context);

		void OnSelect(InputAction.CallbackContext context);

		void OnSubmit(InputAction.CallbackContext context);

		void OnRadioKnob(InputAction.CallbackContext context);

		void OnRadioLeftHandle(InputAction.CallbackContext context);

		void OnRadioRightHandle(InputAction.CallbackContext context);

		void OnLMB(InputAction.CallbackContext context);

		void OnSkipVideo(InputAction.CallbackContext context);

		void OnChangeTab(InputAction.CallbackContext context);

		void OnSpeedUp(InputAction.CallbackContext context);

		void OnSpeedUpTrigger(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_World;

	private List<IWorldActions> m_WorldActionsCallbackInterfaces;

	private readonly InputAction m_World_Move;

	private readonly InputAction m_World_Look;

	private readonly InputAction m_World_Crouch;

	private readonly InputAction m_World_Interact;

	private readonly InputAction m_World_Pause;

	private readonly InputAction m_World_Run;

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces;

	private readonly InputAction m_UI_DialogSkip;

	private readonly InputAction m_UI_Exit;

	private readonly InputAction m_UI_Point;

	private readonly InputAction m_UI_Cancel;

	private readonly InputAction m_UI_Scroll;

	private readonly InputAction m_UI_PauseExit;

	private readonly InputAction m_UI_Tutor;

	private readonly InputAction m_UI_Navigate;

	private readonly InputAction m_UI_Select;

	private readonly InputAction m_UI_Submit;

	private readonly InputAction m_UI_RadioKnob;

	private readonly InputAction m_UI_RadioLeftHandle;

	private readonly InputAction m_UI_RadioRightHandle;

	private readonly InputAction m_UI_LMB;

	private readonly InputAction m_UI_SkipVideo;

	private readonly InputAction m_UI_ChangeTab;

	private readonly InputAction m_UI_SpeedUp;

	private readonly InputAction m_UI_SpeedUpTrigger;

	private int m_KeyboardAndMouseSchemeIndex;

	private int m_GaypadSchemeIndex;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return null;
		}
		set
		{
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => default;

	public IEnumerable<InputBinding> bindings => null;

	public WorldActions World => default;

	public UIActions UI => default;

	public InputControlScheme KeyboardAndMouseScheme => default;

	public InputControlScheme GaypadScheme => default;

	~PlayerInputActions()
	{
	}

	public void Dispose()
	{
	}

	public bool Contains(InputAction action)
	{
		return false;
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return null;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return null;
	}

	public void Enable()
	{
	}

	public void Disable()
	{
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return null;
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		action = null;
		return 0;
	}
}
