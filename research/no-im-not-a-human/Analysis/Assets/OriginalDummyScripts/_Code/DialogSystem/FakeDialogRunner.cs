using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using _Code.Characters;
using _Code.Characters.DialogSystem;
using _Code.Infrastructure.Settings;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.DialogSystem
{
	public sealed class FakeDialogRunner : DialogueRunner
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CUpdateButtonsActivityAsync_003Ed__39 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public FakeDialogRunner _003C_003E4__this;

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
		private struct _003CUpdateButtonsActivityDelayed_003Ed__38 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public FakeDialogRunner _003C_003E4__this;

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
		private DialogSignsView _dialogSignsView;

		[SerializeField]
		private SubtitlesView _subtitlesView;

		[SerializeField]
		private Image _usualLineView;

		[SerializeField]
		private RTLTextMeshPro _usualLineViewText;

		[SerializeField]
		private RTLTextMeshPro _optionsLineViewText;

		[SerializeField]
		private Transform[] _lineViewContents;

		private const float POPUP_TIME = 2.5f;

		private bool _isSubtitleShown;

		private int _initedButtonsCount;

		private INotAHumanSoundService _soundService;

		private InputHandling _inputHandler;

		private SettingsInstance _settingsInstance;

		private HoverableButton[] _buttons;

		[CompilerGenerated]
		private Action m_EnergyConsumed;

		[CompilerGenerated]
		private Action m_DayDialogSelected;

		[CompilerGenerated]
		private Action m_WereBadBoy;

		[CompilerGenerated]
		private Action m_BaseDialogLineShowed;

		[CompilerGenerated]
		private Action m_ButtonsDialogLineShowed;

		public Func<bool> _isDialogOrSubtitleShown;

		private LineView lineView => null;

		private OptionsListView optionsView => null;

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

		public void Init(INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, DialogSaveData saveData, ISettingsInstanceProvider settingsInstanceProvider, Func<bool> isDialogOrSubtitleShown)
		{
		}

		private void OnInputChanged(EInputDevice device)
		{
		}

		private void OnSettingsChanged()
		{
		}

		private void Update()
		{
		}

		public void UpdateButtonsActivity()
		{
		}

		[AsyncStateMachine(typeof(_003CUpdateButtonsActivityDelayed_003Ed__38))]
		public UniTaskVoid UpdateButtonsActivityDelayed()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CUpdateButtonsActivityAsync_003Ed__39))]
		private UniTaskVoid UpdateButtonsActivityAsync()
		{
			return default;
		}

		public void SkipLine()
		{
		}

		public void SetDialogXPos(float xPos)
		{
		}

		public void MoveDialogContentsToTopLayer()
		{
		}

		public void ShowSign(CharacterSOData character, ECharacterSign sign)
		{
		}

		public void StopShowingSign()
		{
		}

		public void OnLineViewTextAppeared()
		{
		}

		public void ShowSubtitle()
		{
		}

		public void HideSubtitle()
		{
		}

		public void ShowSubtitlePopup(EInfoMessageType messageType)
		{
		}

		public void OnEnergyConsumed()
		{
		}

		public void OnDayDialogSelected()
		{
		}

		public void OnWereBadBoy()
		{
		}

		public void ReinitButtons()
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
