using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.DialogSystem;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Sound;
using _Code.Menues.HUD;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class WindowBoardsInteractable : AInteractableObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInteractAsync_003Ed__16 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public WindowBoardsInteractable _003C_003E4__this;

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
		private GameObject _boards;

		[SerializeField]
		private GameObject[] _disableObjects;

		[SerializeField]
		private Camera _subtitleCamera;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _boardingSounds;

		private IHUDPresenter _hudPresenter;

		private INotAHumanSoundService _soundService;

		private IPlayerService _playerService;

		private IGameplayEndingManager _gameplayEndingManager;

		private IDialogManager _dialogManager;

		private bool _hasAlreadyInteracted;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, INotAHumanSoundService soundService, IPlayerService playerService, IGameplayEndingManager gameplayEndingManager, IDialogManager dialogManager)
		{
		}

		public override void Interact()
		{
		}

		[AsyncStateMachine(typeof(_003CInteractAsync_003Ed__16))]
		private UniTask InteractAsync()
		{
			return default;
		}
	}
}
