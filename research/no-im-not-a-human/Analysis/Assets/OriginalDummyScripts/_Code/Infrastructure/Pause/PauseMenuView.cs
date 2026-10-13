using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Localization;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Settings;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.UI.Popup;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.Pause
{
	public sealed class PauseMenuView : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CExitGameAsync_003Ed__49 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public PauseMenuView _003C_003E4__this;

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
		private struct _003CGoToMenuAsync_003Ed__50 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public PauseMenuView _003C_003E4__this;

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
		private struct _003CHide_003Ed__42 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public PauseMenuView _003C_003E4__this;

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
		private struct _003CShow_003Ed__41 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public PauseMenuView _003C_003E4__this;

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
		private EventSystem _eventSystem;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private Material _distortMaterial;

		[SerializeField]
		private GameObject _pauseContent;

		[SerializeField]
		private SettingsInstance _settingsContent;

		[SerializeField]
		private Button _continueButton;

		[SerializeField]
		private Button _settingsButton;

		[SerializeField]
		private Button _mainMenuButton;

		[SerializeField]
		private Button _exitButton;

		[SerializeField]
		private Button _closeSettingsButton;

		[SerializeField]
		private PopupWindow _popupWindow;

		[SerializeField]
		private CanvasGroup _pauseFade;

		private IPlayerService _playerService;

		private ICursorController _cursorController;

		private ILocalizationManager _localizationManager;

		private IHUDPresenter _hudPresenter;

		private InputHandling _inputHandler;

		private IDataModelService _dataModelService;

		private IDayNightController _dayNightController;

		private ILocationsManager _locationsManager;

		private WatcherManager _watcherManager;

		private static readonly int Show1;

		private static readonly int Hide1;

		[CompilerGenerated]
		private Action m_Switched;

		public bool IsChanging { get; private set; }

		public bool IsPaused { get; private set; }

		public bool IsSaveAnQuitting { get; private set; }

		public event Action Switched
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

		public void Init(IPlayerService playerService, ICursorController cursorController, ILocalizationManager localizationManager, IInputHandlerProvider inputHandlerProvider, IHUDPresenter hudPresenter, IDataModelService dataModelService, IDayNightController dayNightController, ILocationsManager locationsManager, WatcherManager watcherManager)
		{
		}

		private void OpenSettings()
		{
		}

		private void CloseSettings()
		{
		}

		[AsyncStateMachine(typeof(_003CShow_003Ed__41))]
		private UniTaskVoid Show()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CHide_003Ed__42))]
		private UniTaskVoid Hide()
		{
			return default;
		}

		private void Update()
		{
		}

		public void OnHiddenAnimationEvent()
		{
		}

		public void Switch(bool state)
		{
		}

		private void SwitchState()
		{
		}

		public void GoToMenu()
		{
		}

		public void ExitGame()
		{
		}

		[AsyncStateMachine(typeof(_003CExitGameAsync_003Ed__49))]
		private UniTaskVoid ExitGameAsync()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CGoToMenuAsync_003Ed__50))]
		private UniTaskVoid GoToMenuAsync()
		{
			return default;
		}
	}
}
