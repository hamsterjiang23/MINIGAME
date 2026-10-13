using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure.Settings;
using _Code.Infrastructure._NINAH__MainMenu.Gacha;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuView : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInitAsync_003Ed__32 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public MainMenuView _003C_003E4__this;

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
		private struct _003CStartGameAsync_003Ed__42 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public MainMenuView _003C_003E4__this;

			public bool isSkipFade;

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
		private struct _003CStartNewGameAsync_003Ed__38 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public MainMenuView _003C_003E4__this;

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
		private struct _003CUpdate_003Ed__50 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncVoidMethodBuilder _003C_003Et__builder;

			public MainMenuView _003C_003E4__this;

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
		private GameObject _newGameButtonGO;

		[SerializeField]
		private GameObject _cloudLoadingGO;

		[SerializeField]
		private MainMenuSignLine[] _signLines;

		[SerializeField]
		private Image _fade;

		[SerializeField]
		private AudioMixer _audioMixer;

		[SerializeField]
		private MainMenuTitleView _title;

		[SerializeField]
		private MainMenuFaceController _faceController;

		[SerializeField]
		private SettingsInstance _settings;

		[SerializeField]
		private GachaWindow _gachaWindow;

		[SerializeField]
		private Button _newGameButton;

		[SerializeField]
		private Button _continueGameButton;

		[SerializeField]
		private Button _settingsButton;

		[SerializeField]
		private Button _collectionButton;

		[SerializeField]
		private Button _quitButton;

		[SerializeField]
		private Button _crButton;

		[SerializeField]
		private Button _trioskazButton;

		private Button[] _buttons;

		private Vector3 _backgroundStartPos;

		private Vector3 _notBackgroundStartPos;

		private bool _areSettingsOpened;

		private bool _areSettingsAnimating;

		private IDataModelService _dataModelService;

		private InputHandling _inputHandler;

		private ICursorController _cursorController;

		private INotAHumanSoundService _soundService;

		private IEndingDataProvider _endingDataProvider;

		private bool _isAlreadyStarted;

		private WatcherManager _watcherManager;

		public void Init(IDataModelService dataModelService, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController, IEndingDataProvider endingDataProvider, INotAHumanSoundService soundService, ICustomYarnReaderProvider customYarnReaderProvider, ICharactersSODataProvider charactersSoDataProvider, WatcherManager watcherManager)
		{
		}

		private void OnCloudLoadingFailed(string error)
		{
		}

		private void OnCloudLoadingChanged(bool isLoading)
		{
		}

		[AsyncStateMachine(typeof(_003CInitAsync_003Ed__32))]
		private UniTaskVoid InitAsync()
		{
			return default;
		}

		private void SetButtonsInteractable(bool isInteractable)
		{
		}

		private void OnInputDeviceChanged(EInputDevice device)
		{
		}

		private void OnMovedEyes(Sprite sprite)
		{
		}

		private void OnMovedTeeth(Sprite sprite)
		{
		}

		public void StartNewGame()
		{
		}

		[AsyncStateMachine(typeof(_003CStartNewGameAsync_003Ed__38))]
		private UniTask StartNewGameAsync()
		{
			return default;
		}

		private void OnIntroCartoonEnded()
		{
		}

		public void LoadGame()
		{
		}

		public void CloseGame()
		{
		}

		[AsyncStateMachine(typeof(_003CStartGameAsync_003Ed__42))]
		private UniTaskVoid StartGameAsync(bool isSkipFade = false)
		{
			return default;
		}

		public void ShowSettings()
		{
		}

		public void CloseSettings()
		{
		}

		public void ShowGacha()
		{
		}

		public void CloseGacha()
		{
		}

		public void ShowTitles()
		{
		}

		public void VisitTrioskaz()
		{
		}

		public void VisitCR()
		{
		}

		[AsyncStateMachine(typeof(_003CUpdate_003Ed__50))]
		private void Update()
		{
		}
	}
}
