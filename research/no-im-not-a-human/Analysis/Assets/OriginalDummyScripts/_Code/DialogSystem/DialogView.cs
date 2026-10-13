using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using _Code.Characters;
using _Code.Characters.DialogSystem;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Settings;
using _Code.Player;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.Sound.Service;

namespace _Code.DialogSystem
{
	public sealed class DialogView : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CSetDialogStateDelayed_003Ed__46 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public DialogView _003C_003E4__this;

			public bool state;

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
		private struct _003CSetSubtitleStateDelayed_003Ed__45 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public DialogView _003C_003E4__this;

			public bool state;

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
		private Canvas _parentCanvas;

		[SerializeField]
		private FakeDialogRunner _fakeDialogRunner;

		[SerializeField]
		private DialogOverlayData[] _dialogOverlays;

		[SerializeField]
		private AnimatedImage _character;

		[SerializeField]
		private AnimatedImage[] _backgroundCharacters;

		[SerializeField]
		private Image _overlay;

		[SerializeField]
		private GameObject _raycastBlocker;

		[SerializeField]
		private Transform _dialogPanelBase;

		[SerializeField]
		private Transform _dialogPanelAnswers;

		[SerializeField]
		private Camera _reserveCamera;

		private DialogOverlayData _currentOverlay;

		private Action _onDialogEndActions;

		private Action _onDialogStarted;

		private bool _isActive;

		private CharacterSOData _currentCharacter;

		private Vector2 _backgroundCharacterPositionRelativeToMainCharacter;

		private Action _temporaryDialogAction;

		private ICursorController _cursorController;

		private bool _isSubtitleActive;

		private EDialogEmotionState _currentEmotion;

		private const float USUAL_DIALOG_X_POSITION = 385f;

		[CompilerGenerated]
		private Action m_EnergyConsumed;

		[CompilerGenerated]
		private Action m_DayDialogSelected;

		[CompilerGenerated]
		private Action m_BaseDialogLineShowed;

		[CompilerGenerated]
		private Action m_ButtonsDialogLineShowed;

		[CompilerGenerated]
		private Action m_WereBadBoy;

		public Camera LastUsedCamera => null;

		public event Action EnergyConsumed
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

		public event Action DayDialogSelected
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

		public event Action BaseDialogLineShowed
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

		public event Action ButtonsDialogLineShowed
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

		public event Action WereBadBoy
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

		public void Init(Action onDialogStarted, Action onDialogEndActions, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, DialogSaveData saveData, ISettingsInstanceProvider settingsInstanceProvider, ICursorController cursorController)
		{
		}

		private void OnButtonsDialogLineShowed()
		{
		}

		private void OnBaseDialogLineShowed()
		{
		}

		private void OnDayDialogSelected()
		{
		}

		private void OnWereBadBoy()
		{
		}

		private void OnEnergyConsumed()
		{
		}

		private void EndDialog()
		{
		}

		[AsyncStateMachine(typeof(_003CSetSubtitleStateDelayed_003Ed__45))]
		private UniTask SetSubtitleStateDelayed(bool state)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CSetDialogStateDelayed_003Ed__46))]
		private UniTask SetDialogStateDelayed(bool state)
		{
			return default;
		}

		public void SkipLine()
		{
		}

		public void StartDialog(CharacterSOData character, string nodeName, EDialogOverlayType overlayType = EDialogOverlayType.None, Camera cam = null, DialogViewData viewData = null, bool hideCharacter = false)
		{
		}

		private void SetupOverlay(EDialogOverlayType overlayType)
		{
		}

		public void SetCharacterEmotion(EDialogEmotionState emotionState)
		{
		}

		public void ShowSign(CharacterSOData character, ECharacterSign sign)
		{
		}

		public void StopShowingSign()
		{
		}

		public void ShowSubtitle(string dialogName, Camera linkedCamera, EDialogOverlayType overlay)
		{
		}

		public void ShowSubtitlePopup(EInfoMessageType messageType, Camera camera, EDialogOverlayType overlayType)
		{
		}

		public void HideSubtitle()
		{
		}

		public void AddActionForNextDialogEnded(Action temporaryDialogAction)
		{
		}

		public void SetBackgroundCharacters(EDialogEmotionState emotion = EDialogEmotionState.Base, params CharacterSOData[] exiledCharacters)
		{
		}

		public void EnableCrtShader()
		{
		}

		public void DisableCrtShader()
		{
		}

		public bool IsNodeVisited(string nodeName)
		{
			return false;
		}

		public void OnLoad(DialogSaveData saveData)
		{
		}
	}
}
