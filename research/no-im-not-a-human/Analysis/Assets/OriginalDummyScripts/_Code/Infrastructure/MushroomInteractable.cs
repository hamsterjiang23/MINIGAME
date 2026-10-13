using UnityEngine;
using _Code.DialogSystem;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.StateObjects;
using _Code.Menues.HUD;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class MushroomInteractable : AInteractableObject
	{
		[SerializeField]
		private Camera _linkedCamera;

		private IConsumablesController _consumablesController;

		private IDialogManager _dialogManager;

		private INotAHumanSoundService _soundService;

		private IStateObjectController _stateObjectController;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public void Init(IConsumablesController consumablesController, IDialogManager dialogManager, IHUDPresenter hudPresenter, IPauseController pauseController, INotAHumanSoundService soundService, IStateObjectController stateObjectController)
		{
		}

		public override void Interact()
		{
		}
	}
}
