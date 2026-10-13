using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class PlayerInputActions : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct WorldActions(PlayerInputActions wrapper)
	{
		private PlayerInputActions m_Wrapper = wrapper;

		public InputAction Move => m_Wrapper.m_World_Move;

		public InputAction Look => m_Wrapper.m_World_Look;

		public InputAction Crouch => m_Wrapper.m_World_Crouch;

		public InputAction Interact => m_Wrapper.m_World_Interact;

		public InputAction Pause => m_Wrapper.m_World_Pause;

		public InputAction Run => m_Wrapper.m_World_Run;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_World;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(WorldActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IWorldActions instance)
		{
			if (instance != null && !m_Wrapper.m_WorldActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_WorldActionsCallbackInterfaces.Add(instance);
				Move.started += instance.OnMove;
				Move.performed += instance.OnMove;
				Move.canceled += instance.OnMove;
				Look.started += instance.OnLook;
				Look.performed += instance.OnLook;
				Look.canceled += instance.OnLook;
				Crouch.started += instance.OnCrouch;
				Crouch.performed += instance.OnCrouch;
				Crouch.canceled += instance.OnCrouch;
				Interact.started += instance.OnInteract;
				Interact.performed += instance.OnInteract;
				Interact.canceled += instance.OnInteract;
				Pause.started += instance.OnPause;
				Pause.performed += instance.OnPause;
				Pause.canceled += instance.OnPause;
				Run.started += instance.OnRun;
				Run.performed += instance.OnRun;
				Run.canceled += instance.OnRun;
			}
		}

		private void UnregisterCallbacks(IWorldActions instance)
		{
			Move.started -= instance.OnMove;
			Move.performed -= instance.OnMove;
			Move.canceled -= instance.OnMove;
			Look.started -= instance.OnLook;
			Look.performed -= instance.OnLook;
			Look.canceled -= instance.OnLook;
			Crouch.started -= instance.OnCrouch;
			Crouch.performed -= instance.OnCrouch;
			Crouch.canceled -= instance.OnCrouch;
			Interact.started -= instance.OnInteract;
			Interact.performed -= instance.OnInteract;
			Interact.canceled -= instance.OnInteract;
			Pause.started -= instance.OnPause;
			Pause.performed -= instance.OnPause;
			Pause.canceled -= instance.OnPause;
			Run.started -= instance.OnRun;
			Run.performed -= instance.OnRun;
			Run.canceled -= instance.OnRun;
		}

		public void RemoveCallbacks(IWorldActions instance)
		{
			if (m_Wrapper.m_WorldActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IWorldActions instance)
		{
			foreach (IWorldActions worldActionsCallbackInterface in m_Wrapper.m_WorldActionsCallbackInterfaces)
			{
				UnregisterCallbacks(worldActionsCallbackInterface);
			}
			m_Wrapper.m_WorldActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
		}
	}

	public struct UIActions(PlayerInputActions wrapper)
	{
		private PlayerInputActions m_Wrapper = wrapper;

		public InputAction DialogSkip => m_Wrapper.m_UI_DialogSkip;

		public InputAction Exit => m_Wrapper.m_UI_Exit;

		public InputAction Point => m_Wrapper.m_UI_Point;

		public InputAction Cancel => m_Wrapper.m_UI_Cancel;

		public InputAction Scroll => m_Wrapper.m_UI_Scroll;

		public InputAction PauseExit => m_Wrapper.m_UI_PauseExit;

		public InputAction Tutor => m_Wrapper.m_UI_Tutor;

		public InputAction Navigate => m_Wrapper.m_UI_Navigate;

		public InputAction Select => m_Wrapper.m_UI_Select;

		public InputAction Submit => m_Wrapper.m_UI_Submit;

		public InputAction RadioKnob => m_Wrapper.m_UI_RadioKnob;

		public InputAction RadioLeftHandle => m_Wrapper.m_UI_RadioLeftHandle;

		public InputAction RadioRightHandle => m_Wrapper.m_UI_RadioRightHandle;

		public InputAction LMB => m_Wrapper.m_UI_LMB;

		public InputAction SkipVideo => m_Wrapper.m_UI_SkipVideo;

		public InputAction ChangeTab => m_Wrapper.m_UI_ChangeTab;

		public InputAction SpeedUp => m_Wrapper.m_UI_SpeedUp;

		public InputAction SpeedUpTrigger => m_Wrapper.m_UI_SpeedUpTrigger;

		public bool enabled => Get().enabled;

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void AddCallbacks(IUIActions instance)
		{
			if (instance != null && !m_Wrapper.m_UIActionsCallbackInterfaces.Contains(instance))
			{
				m_Wrapper.m_UIActionsCallbackInterfaces.Add(instance);
				DialogSkip.started += instance.OnDialogSkip;
				DialogSkip.performed += instance.OnDialogSkip;
				DialogSkip.canceled += instance.OnDialogSkip;
				Exit.started += instance.OnExit;
				Exit.performed += instance.OnExit;
				Exit.canceled += instance.OnExit;
				Point.started += instance.OnPoint;
				Point.performed += instance.OnPoint;
				Point.canceled += instance.OnPoint;
				Cancel.started += instance.OnCancel;
				Cancel.performed += instance.OnCancel;
				Cancel.canceled += instance.OnCancel;
				Scroll.started += instance.OnScroll;
				Scroll.performed += instance.OnScroll;
				Scroll.canceled += instance.OnScroll;
				PauseExit.started += instance.OnPauseExit;
				PauseExit.performed += instance.OnPauseExit;
				PauseExit.canceled += instance.OnPauseExit;
				Tutor.started += instance.OnTutor;
				Tutor.performed += instance.OnTutor;
				Tutor.canceled += instance.OnTutor;
				Navigate.started += instance.OnNavigate;
				Navigate.performed += instance.OnNavigate;
				Navigate.canceled += instance.OnNavigate;
				Select.started += instance.OnSelect;
				Select.performed += instance.OnSelect;
				Select.canceled += instance.OnSelect;
				Submit.started += instance.OnSubmit;
				Submit.performed += instance.OnSubmit;
				Submit.canceled += instance.OnSubmit;
				RadioKnob.started += instance.OnRadioKnob;
				RadioKnob.performed += instance.OnRadioKnob;
				RadioKnob.canceled += instance.OnRadioKnob;
				RadioLeftHandle.started += instance.OnRadioLeftHandle;
				RadioLeftHandle.performed += instance.OnRadioLeftHandle;
				RadioLeftHandle.canceled += instance.OnRadioLeftHandle;
				RadioRightHandle.started += instance.OnRadioRightHandle;
				RadioRightHandle.performed += instance.OnRadioRightHandle;
				RadioRightHandle.canceled += instance.OnRadioRightHandle;
				LMB.started += instance.OnLMB;
				LMB.performed += instance.OnLMB;
				LMB.canceled += instance.OnLMB;
				SkipVideo.started += instance.OnSkipVideo;
				SkipVideo.performed += instance.OnSkipVideo;
				SkipVideo.canceled += instance.OnSkipVideo;
				ChangeTab.started += instance.OnChangeTab;
				ChangeTab.performed += instance.OnChangeTab;
				ChangeTab.canceled += instance.OnChangeTab;
				SpeedUp.started += instance.OnSpeedUp;
				SpeedUp.performed += instance.OnSpeedUp;
				SpeedUp.canceled += instance.OnSpeedUp;
				SpeedUpTrigger.started += instance.OnSpeedUpTrigger;
				SpeedUpTrigger.performed += instance.OnSpeedUpTrigger;
				SpeedUpTrigger.canceled += instance.OnSpeedUpTrigger;
			}
		}

		private void UnregisterCallbacks(IUIActions instance)
		{
			DialogSkip.started -= instance.OnDialogSkip;
			DialogSkip.performed -= instance.OnDialogSkip;
			DialogSkip.canceled -= instance.OnDialogSkip;
			Exit.started -= instance.OnExit;
			Exit.performed -= instance.OnExit;
			Exit.canceled -= instance.OnExit;
			Point.started -= instance.OnPoint;
			Point.performed -= instance.OnPoint;
			Point.canceled -= instance.OnPoint;
			Cancel.started -= instance.OnCancel;
			Cancel.performed -= instance.OnCancel;
			Cancel.canceled -= instance.OnCancel;
			Scroll.started -= instance.OnScroll;
			Scroll.performed -= instance.OnScroll;
			Scroll.canceled -= instance.OnScroll;
			PauseExit.started -= instance.OnPauseExit;
			PauseExit.performed -= instance.OnPauseExit;
			PauseExit.canceled -= instance.OnPauseExit;
			Tutor.started -= instance.OnTutor;
			Tutor.performed -= instance.OnTutor;
			Tutor.canceled -= instance.OnTutor;
			Navigate.started -= instance.OnNavigate;
			Navigate.performed -= instance.OnNavigate;
			Navigate.canceled -= instance.OnNavigate;
			Select.started -= instance.OnSelect;
			Select.performed -= instance.OnSelect;
			Select.canceled -= instance.OnSelect;
			Submit.started -= instance.OnSubmit;
			Submit.performed -= instance.OnSubmit;
			Submit.canceled -= instance.OnSubmit;
			RadioKnob.started -= instance.OnRadioKnob;
			RadioKnob.performed -= instance.OnRadioKnob;
			RadioKnob.canceled -= instance.OnRadioKnob;
			RadioLeftHandle.started -= instance.OnRadioLeftHandle;
			RadioLeftHandle.performed -= instance.OnRadioLeftHandle;
			RadioLeftHandle.canceled -= instance.OnRadioLeftHandle;
			RadioRightHandle.started -= instance.OnRadioRightHandle;
			RadioRightHandle.performed -= instance.OnRadioRightHandle;
			RadioRightHandle.canceled -= instance.OnRadioRightHandle;
			LMB.started -= instance.OnLMB;
			LMB.performed -= instance.OnLMB;
			LMB.canceled -= instance.OnLMB;
			SkipVideo.started -= instance.OnSkipVideo;
			SkipVideo.performed -= instance.OnSkipVideo;
			SkipVideo.canceled -= instance.OnSkipVideo;
			ChangeTab.started -= instance.OnChangeTab;
			ChangeTab.performed -= instance.OnChangeTab;
			ChangeTab.canceled -= instance.OnChangeTab;
			SpeedUp.started -= instance.OnSpeedUp;
			SpeedUp.performed -= instance.OnSpeedUp;
			SpeedUp.canceled -= instance.OnSpeedUp;
			SpeedUpTrigger.started -= instance.OnSpeedUpTrigger;
			SpeedUpTrigger.performed -= instance.OnSpeedUpTrigger;
			SpeedUpTrigger.canceled -= instance.OnSpeedUpTrigger;
		}

		public void RemoveCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterfaces.Remove(instance))
			{
				UnregisterCallbacks(instance);
			}
		}

		public void SetCallbacks(IUIActions instance)
		{
			foreach (IUIActions uIActionsCallbackInterface in m_Wrapper.m_UIActionsCallbackInterfaces)
			{
				UnregisterCallbacks(uIActionsCallbackInterface);
			}
			m_Wrapper.m_UIActionsCallbackInterfaces.Clear();
			AddCallbacks(instance);
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

	private List<IWorldActions> m_WorldActionsCallbackInterfaces = new List<IWorldActions>();

	private readonly InputAction m_World_Move;

	private readonly InputAction m_World_Look;

	private readonly InputAction m_World_Crouch;

	private readonly InputAction m_World_Interact;

	private readonly InputAction m_World_Pause;

	private readonly InputAction m_World_Run;

	private readonly InputActionMap m_UI;

	private List<IUIActions> m_UIActionsCallbackInterfaces = new List<IUIActions>();

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

	private int m_KeyboardAndMouseSchemeIndex = -1;

	private int m_GaypadSchemeIndex = -1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public WorldActions World => new WorldActions(this);

	public UIActions UI => new UIActions(this);

	public InputControlScheme KeyboardAndMouseScheme
	{
		get
		{
			if (m_KeyboardAndMouseSchemeIndex == -1)
			{
				m_KeyboardAndMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard And Mouse");
			}
			return asset.controlSchemes[m_KeyboardAndMouseSchemeIndex];
		}
	}

	public InputControlScheme GaypadScheme
	{
		get
		{
			if (m_GaypadSchemeIndex == -1)
			{
				m_GaypadSchemeIndex = asset.FindControlSchemeIndex("Gaypad");
			}
			return asset.controlSchemes[m_GaypadSchemeIndex];
		}
	}

	public PlayerInputActions()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"version\": 1,\r\n    \"name\": \"PlayerInputActions\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"World\",\r\n            \"id\": \"cb4067f3-a685-4c86-b9ea-46a6eabfada7\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Move\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"548e32fd-77d1-40e5-8197-32ca56b41bc0\",\r\n                    \"expectedControlType\": \"Double\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Look\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"c58af18a-cd57-434c-b9d4-561834d18e44\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Crouch\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"c14bfa58-ac89-47e5-a63b-fe9dcfec834d\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Interact\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"4245cc1f-a47b-4a66-85e9-1491e96ac15e\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Pause\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"a82984a9-c28e-4519-ad33-5412b2021340\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Run\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"4da805b3-2047-45a2-96e2-e261e4b66831\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"WASD\",\r\n                    \"id\": \"99258992-afbc-4513-a4ee-24146566e341\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"d57e0987-ea9f-4b18-9042-239931d4c060\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"75168890-922f-4122-9968-1ecac0f33c28\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"54b99838-0c45-421e-af38-b1f25b3f6927\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"77680fb0-0b9d-4a74-97de-9e3149ad6526\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ff1fe763-d03f-4a43-a0a8-5126b9489d6b\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7527e23e-b005-4c42-b957-f1ee9cc3fcff\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Pause\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c47a24d5-2441-4728-8a0b-b532570177c8\",\r\n                    \"path\": \"<Gamepad>/start\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Pause\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4e613f51-5611-46d1-958c-1bce7a6f0bc5\",\r\n                    \"path\": \"<Keyboard>/leftCtrl\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Crouch\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"2238e12d-1942-4527-b218-af78b41ef6bd\",\r\n                    \"path\": \"<Gamepad>/rightStickPress\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Crouch\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e2fc914a-adcc-4f54-9e65-e5230756f6b7\",\r\n                    \"path\": \"<Mouse>/delta\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"ScaleVector2(x=0.1,y=0.1)\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b5642af9-1660-4958-b299-d2259b2a6817\",\r\n                    \"path\": \"<Gamepad>/rightStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fcfc1e37-f067-4da9-b484-da1d7dc82203\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0e1fe3d8-d8df-402b-80a4-c3a0bf4bf95f\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d3fcf7a8-3798-4fa5-8819-3db482ff4a85\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6022b1cf-4b95-4fb7-8d6d-b73a98c58a4d\",\r\n                    \"path\": \"<Gamepad>/buttonSouth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Interact\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"13751565-8fb9-451a-99a5-956f896e39b5\",\r\n                    \"path\": \"<Keyboard>/shift\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Run\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cce2f704-0f5d-42f3-a961-94963dedd095\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Run\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"UI\",\r\n            \"id\": \"4fcb02e3-6938-4463-87dd-a833e68782aa\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"DialogSkip\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"5aa0bfd4-2f61-4b8d-b4ab-4fb0337358fc\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Exit\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"8834eed7-8d68-41ff-a34d-fa8afe45b7d1\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Point\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"bb00fea7-f6b6-46cd-978f-9d83d49e9b7b\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Cancel\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"1af6fa3d-f14d-43f7-87b5-3d142a49e038\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Scroll\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"200bd790-a32f-483d-b442-a41fe01cd517\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"PauseExit\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"91759b80-1d59-4646-88fe-4bf8a6d0b8ff\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Tutor\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"b881beb3-cba7-43ae-8426-762b14a03f14\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Navigate\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"6681be7b-c070-4f98-b3a6-dc1706ed10a0\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Select\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"f8363c14-3af8-447f-94a6-7a85e49b04c6\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Submit\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"78b1785a-1c6c-440b-9bf0-713114f9a332\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RadioKnob\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"1c8c19d2-8953-4eae-a14b-4dd0e4119520\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"RadioLeftHandle\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"925dab8e-7050-4e95-a29f-183d73f65084\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RadioRightHandle\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"f292ffb6-30b5-4992-a263-d024e49d997e\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"LMB\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"5cdc8d7d-d3b9-4e86-b383-44efd34b0c1e\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SkipVideo\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"582216d4-8495-4821-a1de-de233dd2b1ea\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"ChangeTab\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"83abea6c-8f4d-4668-aedc-5f1fcc6121d5\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SpeedUp\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"769b0df5-aab7-4e67-b3d7-46c55acca7ef\",\r\n                    \"expectedControlType\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"SpeedUpTrigger\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"c2f0a54d-b657-495f-a240-f441437dafb2\",\r\n                    \"expectedControlType\": \"Double\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a48a0be6-008d-4161-a600-d3a311248358\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"DialogSkip\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f20b4ded-062c-4a0a-afe4-945e547632ef\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"DialogSkip\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3c59d769-4bdc-4c9f-b41f-cb6810e320ff\",\r\n                    \"path\": \"<Gamepad>/buttonWest\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"DialogSkip\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"a422fb5b-6ef8-42e4-92cb-6439db823d8f\",\r\n                    \"path\": \"<Keyboard>/q\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Exit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c102ac36-2bba-4f02-83e9-793bfdcbe899\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"Exit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"694f7403-3778-49f5-9952-5979adeede1b\",\r\n                    \"path\": \"<Gamepad>/buttonEast\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Exit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6ad39e42-731e-475b-bf47-9b639dbf0faa\",\r\n                    \"path\": \"<Gamepad>/start\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Exit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ad7cfdd5-1d15-4e43-aa2a-f3356295f39c\",\r\n                    \"path\": \"<Mouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5267e724-3eeb-44ac-82da-9791de3a270c\",\r\n                    \"path\": \"<Gamepad>/rightStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"ScaleVector2\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f4abd8e0-36a0-4934-8933-2a32897e83df\",\r\n                    \"path\": \"<Keyboard>/q\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"613fe4e0-3f84-48bc-a885-044c3fa2b37d\",\r\n                    \"path\": \"<Gamepad>/buttonEast\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ab9cd170-78fb-4037-b81d-fb897d3ca1c1\",\r\n                    \"path\": \"<Gamepad>/rightStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Scroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"46dbb29d-cdef-4cf6-a4de-17b535b9f418\",\r\n                    \"path\": \"<Mouse>/scroll\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"Scroll\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ef1c844d-4631-48d6-849f-d65437c7ab4c\",\r\n                    \"path\": \"<Keyboard>/escape\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"PauseExit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6084abd2-2fe1-4b38-b9ae-0682f2dfd781\",\r\n                    \"path\": \"<Gamepad>/start\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"PauseExit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e55c61b9-bb3c-4195-8f62-fbed2cce5f2a\",\r\n                    \"path\": \"<Keyboard>/f\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Tutor\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"bac61f45-be1a-4248-a796-1572fb9ae206\",\r\n                    \"path\": \"<Gamepad>/buttonNorth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Tutor\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"WASD\",\r\n                    \"id\": \"08fa6ad6-0d7b-45f8-bd2c-bcc7f99b3ed8\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"0068d546-9cbb-4cb8-a657-1feff163ac30\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"a4198258-0c2c-4794-8cae-0602a268f2e5\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"3e4330ee-33e9-4dc0-882b-8c61f2d6efec\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"fc812bc5-0327-4ca0-acaa-cee82d62fd9f\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard And Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"Arrows\",\r\n                    \"id\": \"3855f4bd-e24f-42ca-8f17-2989f3606b24\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"d0ceaf5e-5d3a-4afa-9580-637f86ef5b1f\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"c0911204-be6d-41e1-8e75-dbdacd87308f\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"619089c5-893b-4519-a3f1-44b3ccb9bd75\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"1b870cc9-5fea-460b-bb2b-84fdcec8b713\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"DPad\",\r\n                    \"id\": \"a70773ec-867d-421f-a55a-83ec8a7b319f\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"e7b8bc0c-fa10-4ab0-b31f-8cf42f74e93f\",\r\n                    \"path\": \"<Gamepad>/dpad/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"4bb00cb1-fbd5-4b8e-8dd4-90a4149e4d47\",\r\n                    \"path\": \"<Gamepad>/dpad/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"8018915f-66d0-4795-8f1f-2880d94104d7\",\r\n                    \"path\": \"<Gamepad>/dpad/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"9e7c30ff-d370-46d0-b305-638d593fb484\",\r\n                    \"path\": \"<Gamepad>/dpad/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7aa04d8f-2192-4b0d-8d5a-d6cf92097f37\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d4d38921-6b59-4268-a07a-73b41aec894a\",\r\n                    \"path\": \"<Gamepad>/buttonSouth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Select\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"81db414a-1fac-4d09-ba09-372151a59408\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"Select\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"df35b71a-3eb0-4a25-b12a-9a3be35efeaf\",\r\n                    \"path\": \"<Keyboard>/enter\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c725afc0-f771-4693-a230-cc3fb638138d\",\r\n                    \"path\": \"<Gamepad>/buttonSouth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e366ab24-6ff0-42cf-a071-553cb5c58101\",\r\n                    \"path\": \"<Gamepad>/rightStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"RadioKnob\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"73733642-2730-492d-b286-0c96d4f5f812\",\r\n                    \"path\": \"<Gamepad>/leftShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"RadioLeftHandle\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"51b333d6-7085-45af-bb69-a90ac08b3f20\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"RadioRightHandle\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f01867b6-f853-4995-8861-052d8c616ca7\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"LMB\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"59fd00c2-4152-407e-b9ce-cf53f6d35348\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"SkipVideo\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8e4c9ca0-7ecc-475e-9e39-dd529c983c93\",\r\n                    \"path\": \"<Gamepad>/buttonSouth\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"SkipVideo\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0f49c600-0935-40bd-8492-a61604466c5b\",\r\n                    \"path\": \"<Gamepad>/rightShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"ChangeTab\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"60a9aedf-bec7-4284-854f-345ee35f5f6a\",\r\n                    \"path\": \"<Gamepad>/leftShoulder\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"ChangeTab\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ced0db44-80f5-4c11-9a38-db091ca606f3\",\r\n                    \"path\": \"<Keyboard>/shift\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard And Mouse\",\r\n                    \"action\": \"SpeedUp\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ea045465-58c2-4455-8749-d50e65781d7e\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"SpeedUp\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"22efaeac-4fcc-4354-841f-e7391c14fe67\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gaypad\",\r\n                    \"action\": \"SpeedUpTrigger\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": [\r\n        {\r\n            \"name\": \"Keyboard And Mouse\",\r\n            \"bindingGroup\": \"Keyboard And Mouse\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Keyboard>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                },\r\n                {\r\n                    \"devicePath\": \"<Mouse>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Gaypad\",\r\n            \"bindingGroup\": \"Gaypad\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Gamepad>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}");
		m_World = asset.FindActionMap("World", throwIfNotFound: true);
		m_World_Move = m_World.FindAction("Move", throwIfNotFound: true);
		m_World_Look = m_World.FindAction("Look", throwIfNotFound: true);
		m_World_Crouch = m_World.FindAction("Crouch", throwIfNotFound: true);
		m_World_Interact = m_World.FindAction("Interact", throwIfNotFound: true);
		m_World_Pause = m_World.FindAction("Pause", throwIfNotFound: true);
		m_World_Run = m_World.FindAction("Run", throwIfNotFound: true);
		m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
		m_UI_DialogSkip = m_UI.FindAction("DialogSkip", throwIfNotFound: true);
		m_UI_Exit = m_UI.FindAction("Exit", throwIfNotFound: true);
		m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
		m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
		m_UI_Scroll = m_UI.FindAction("Scroll", throwIfNotFound: true);
		m_UI_PauseExit = m_UI.FindAction("PauseExit", throwIfNotFound: true);
		m_UI_Tutor = m_UI.FindAction("Tutor", throwIfNotFound: true);
		m_UI_Navigate = m_UI.FindAction("Navigate", throwIfNotFound: true);
		m_UI_Select = m_UI.FindAction("Select", throwIfNotFound: true);
		m_UI_Submit = m_UI.FindAction("Submit", throwIfNotFound: true);
		m_UI_RadioKnob = m_UI.FindAction("RadioKnob", throwIfNotFound: true);
		m_UI_RadioLeftHandle = m_UI.FindAction("RadioLeftHandle", throwIfNotFound: true);
		m_UI_RadioRightHandle = m_UI.FindAction("RadioRightHandle", throwIfNotFound: true);
		m_UI_LMB = m_UI.FindAction("LMB", throwIfNotFound: true);
		m_UI_SkipVideo = m_UI.FindAction("SkipVideo", throwIfNotFound: true);
		m_UI_ChangeTab = m_UI.FindAction("ChangeTab", throwIfNotFound: true);
		m_UI_SpeedUp = m_UI.FindAction("SpeedUp", throwIfNotFound: true);
		m_UI_SpeedUpTrigger = m_UI.FindAction("SpeedUpTrigger", throwIfNotFound: true);
	}

	~PlayerInputActions()
	{
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
