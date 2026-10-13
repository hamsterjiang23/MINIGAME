using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;
using _Code.Characters;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure._NINAH__Endings.View;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Code.Utils.UI;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure._NINAH__MainMenu.Gacha
{
	public sealed class GachaWindow : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInitEndings_003Ed__46 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public GachaWindow _003C_003E4__this;

			public IEndingDataProvider endingDataProvider;

			public ICharactersSODataProvider charactersSODataProvider;

			public INotAHumanSoundService soundService;

			public ICursorController cursorController;

			public IInputHandlerProvider inputHandlerProvider;

			public IDataModelService dataModelService;

			public WatcherManager watcherManager;

			public ICustomYarnReaderProvider customYarnReaderProvider;

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
		private struct _003CResetCharacter_003Ed__60 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public GachaWindow _003C_003E4__this;

			public CharacterSOData character;

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
		private GameObject _endingsPanel;

		[SerializeField]
		private GameObject _charactersPanel;

		[SerializeField]
		private GachaEndingView[] _gachaEndingViews;

		[SerializeField]
		private GachaCharacterView[] _gachaCharacterViews;

		[SerializeField]
		private EndingView _cartoonWatcher;

		[SerializeField]
		private Image _characterSprite;

		[SerializeField]
		private TMP_Text _characterQuote;

		[SerializeField]
		private LocalizedString _baseQuote;

		[SerializeField]
		private Button _nextGachaButton;

		[SerializeField]
		private Image _currentGachaImage;

		[SerializeField]
		private Image _nextGachaImage;

		[SerializeField]
		private Sprite _endingsGachaSprite;

		[SerializeField]
		private Sprite _charactersGachaSprite;

		[SerializeField]
		private Sprite _endingsGachaButtonSprite;

		[SerializeField]
		private Sprite _charactersGachaButtonSprite;

		[SerializeField]
		private Image _progresFill;

		[SerializeField]
		private RTLTextMeshPro _progressText;

		[SerializeField]
		private GameObject _firstEndingGO;

		[SerializeField]
		private GameObject _firstCharacterGO;

		[SerializeField]
		private Button _backButton;

		[SerializeField]
		private Scrollbar _scrollbarEndings;

		[SerializeField]
		private Scrollbar _scrollbarCharacters;

		[SerializeField]
		private ScrollRect _scrollRectEndings;

		[SerializeField]
		private ScrollRect _scrollRectCharacters;

		[SerializeField]
		private GamepadTypeControlView _backGamepadHint;

		[SerializeField]
		private GamepadTypeControlView _switchGamepadHint;

		private UISelectable[] _selectablesEndings;

		private UISelectable[] _selectablesCharacters;

		private EndingViewSOData[] _endingsData;

		private CharacterSOData[] _charactersData;

		private INotAHumanSoundService _soundService;

		private ICursorController _cursorController;

		private InputHandling _inputHandler;

		private bool _isCurrentGachaIsEndings;

		private CharacterSOData _selectedCharacter;

		private IDataModelService _dataModelService;

		[CompilerGenerated]
		private Action m_CartoonEnded;

		private int _endingsTotalCount;

		private int _endingsCurrentCount;

		private int _charactersTotalCount;

		private int _charactersCurrentCount;

		private ECharacterType[] _ignoreCharacters;

		private EEnding[] _ignoreEndings;

		private WatcherManager _watcherManager;

		public event Action CartoonEnded
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

		[AsyncStateMachine(typeof(_003CInitEndings_003Ed__46))]
		public UniTaskVoid InitEndings(IEndingDataProvider endingDataProvider, IDataModelService dataModelService, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, INotAHumanSoundService soundService, ICustomYarnReaderProvider customYarnReaderProvider, ICharactersSODataProvider charactersSODataProvider, WatcherManager watcherManager)
		{
			return default;
		}

		private void OnInputChanged(EInputDevice device)
		{
		}

		private void OnEnable()
		{
		}

		private void Start()
		{
		}

		private void OnItemSelectedEndings(BaseEventData eventData)
		{
		}

		private void OnItemSelectedCharacters(BaseEventData eventData)
		{
		}

		public void ReinitLocalization()
		{
		}

		private void ReinitCharacters(List<ECharacterType> unlockedCharacters)
		{
		}

		private void NextGacha()
		{
		}

		private void UpdateCountForEndings()
		{
		}

		private void UpdateCountForCharacters()
		{
		}

		private void OnCartoonEnded()
		{
		}

		public void ShowEnding(EndingViewSOData ending)
		{
		}

		private void OnCharacterSelected(CharacterSOData character)
		{
		}

		[AsyncStateMachine(typeof(_003CResetCharacter_003Ed__60))]
		private UniTask ResetCharacter(CharacterSOData character)
		{
			return default;
		}

		private void Update()
		{
		}
	}
}
