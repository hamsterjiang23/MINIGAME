using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Menues.HUD;

namespace _Code.Infrastructure
{
	public sealed class HatchInteractable : AInteractableObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInteractAsync_003Ed__17 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public HatchInteractable _003C_003E4__this;

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

		[SerializeField]
		private bool _isInsideBasement;

		[SerializeField]
		private Collider _collider;

		private readonly Vector3 _startRotation;

		private readonly Vector3 _endRotation;

		private readonly float _rotationDuration;

		private bool _isOpened;

		private ILocationsManager _locationsManager;

		private IDayNightController _dayNightController;

		private bool _isInteracting;

		private IPlayerService _playerService;

		private IHUDPresenter _hudPresenter;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public void Init(ILocationsManager locationsManager, IHUDPresenter hudPresenter, IPauseController pauseController, IDayNightController dayNightController, IPlayerService playerService)
		{
		}

		public override void Interact()
		{
		}

		[AsyncStateMachine(typeof(_003CInteractAsync_003Ed__17))]
		private UniTask InteractAsync()
		{
			return default;
		}
	}
}
