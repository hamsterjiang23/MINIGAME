using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Menues.HUD;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class CigaretteInteractable : AInteractableObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CEnableAfterPause_003Ed__23 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public float smokeDelay;

			public CigaretteInteractable _003C_003E4__this;

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
		private GameObject _cinemachineCam;

		[SerializeField]
		private GameObject _cigarettes;

		[SerializeField]
		private LocalizedString _localizedStringCigarettes;

		private INotAHumanSoundService _soundService;

		private IHUDPresenter _hudPresenter;

		private IConsumablesController _consumablesController;

		private IDayNightController _dayNightController;

		private ICloseUpsController _closeUpsController;

		private IPlayerService _playerService;

		private bool _isStarted;

		private bool _isInteracting;

		private const float SMOKE_DELAY = 2.5f;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IConsumablesController consumablesController, IDayNightController dayNightController, ICloseUpsController closeUpsController, IPlayerService playerService, INotAHumanSoundService soundService)
		{
		}

		private void UpdateString()
		{
		}

		private void OnDestroy()
		{
		}

		private void OnUpdatedConsumables(EConsumable consumable, int count)
		{
		}

		public override void Interact()
		{
		}

		private void OnClose()
		{
		}

		private void OnUse()
		{
		}

		[AsyncStateMachine(typeof(_003CEnableAfterPause_003Ed__23))]
		private UniTaskVoid EnableAfterPause(float smokeDelay)
		{
			return default;
		}

		public override void OnLoad()
		{
		}
	}
}
