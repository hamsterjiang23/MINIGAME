using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Menues.HUD;

namespace _Code.Infrastructure
{
	public sealed class EndingLaunchInteractable : AInteractableObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInteractAsync_003Ed__11 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public EndingLaunchInteractable _003C_003E4__this;

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
		private EEnding _ending;

		private IHUDPresenter _hudPresenter;

		private IPlayerService _playerService;

		private IGameplayEndingManager _gameplayEndingManager;

		private bool _hasAlreadyInteracted;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IPlayerService playerService, IGameplayEndingManager gameplayEndingManager)
		{
		}

		public override void Interact()
		{
		}

		[AsyncStateMachine(typeof(_003CInteractAsync_003Ed__11))]
		private UniTask InteractAsync()
		{
			return default;
		}

		public void SetEnding(EEnding ending)
		{
		}
	}
}
