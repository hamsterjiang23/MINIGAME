using UnityEngine;
using _Code.Infrastructure._NINAH__InteractableObjects.Objects;

namespace _Code.Infrastructure
{
	public sealed class InteractablesViewProvider : MonoBehaviour, IInteractablesViewProvider
	{
		[field: SerializeField]
		public HatchInteractable HatchHouse { get; private set; }

		[field: SerializeField]
		public HatchInteractable HatchBasement { get; private set; }

		[field: SerializeField]
		public PhoneInteractable Phone { get; private set; }

		[field: SerializeField]
		public RadioInteractable Radio { get; private set; }

		[field: SerializeField]
		public CigaretteInteractable Cigarettes { get; private set; }

		[field: SerializeField]
		public WindowBoardsInteractable[] WindowBoards { get; private set; }

		[field: SerializeField]
		public EndingLaunchInteractable PeepholeEnding { get; private set; }

		[field: SerializeField]
		public SaveInteractable SaveInteractable { get; private set; }

		[field: SerializeField]
		public MushroomInteractable Mushroom { get; private set; }

		[field: SerializeField]
		public TheHoleInteractable TheHole { get; private set; }

		[field: SerializeField]
		public CatInteractable Cat { get; private set; }

		[field: SerializeField]
		public DialogInteractable[] Dialogs { get; private set; }

		[field: SerializeField]
		public EndingLaunchInteractable DeathEndingInteractable { get; private set; }

		[field: SerializeField]
		public ZoomInteractable[] ZoomInteractables { get; private set; }

		[field: SerializeField]
		public CalendarInteractable CalendarInteractable { get; private set; }
	}
}
