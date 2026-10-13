using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.StateObjects;
using _Code.Menues.HUD;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class TheHoleInteractable : AInteractableObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInteractAsync_003Ed__18 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public TheHoleInteractable _003C_003E4__this;

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
		[SearchableEnum]
		private ESound[] _diggingSounds;

		private IStateObjectController _stateObjectController;

		private IHUDPresenter _hudPresenter;

		private IPlayerService _playerService;

		private IDayNightController _dayNightController;

		private INotAHumanSoundService _soundService;

		private bool _isInteracting;

		[CompilerGenerated]
		private Action m_HoleReady;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		protected override int EnergyCost => 0;

		public event Action HoleReady
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

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IStateObjectController stateObjectController, IPlayerService playerService, IDayNightController dayNightController, INotAHumanSoundService soundService)
		{
		}

		public override void Interact()
		{
		}

		[AsyncStateMachine(typeof(_003CInteractAsync_003Ed__18))]
		private UniTask InteractAsync()
		{
			return default;
		}
	}
}
