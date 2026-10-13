using UnityEngine;
using _Code.Infrastructure.Pause;
using _Code.Menues.HUD;
using _Code.Player;

namespace _Code.Infrastructure
{
	public class ZoomInteractable : AInteractableObject
	{
		[SerializeField]
		private GameObject _camera;

		private bool _isOpened;

		protected IHUDPresenter _hudPresenter;

		protected InputHandling _inputHandler;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IInputHandlerProvider inputHandlerProvider)
		{
		}

		public override void Interact()
		{
		}

		private void Update()
		{
		}

		private void Close()
		{
		}
	}
}
