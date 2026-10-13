using _Code.Infrastructure._NINAH__InteractableObjects.Objects;

namespace _Code.Infrastructure
{
	public interface IInteractablesViewProvider
	{
		HatchInteractable HatchHouse { get; }

		HatchInteractable HatchBasement { get; }

		PhoneInteractable Phone { get; }

		RadioInteractable Radio { get; }

		CigaretteInteractable Cigarettes { get; }

		WindowBoardsInteractable[] WindowBoards { get; }

		EndingLaunchInteractable PeepholeEnding { get; }

		SaveInteractable SaveInteractable { get; }

		MushroomInteractable Mushroom { get; }

		TheHoleInteractable TheHole { get; }

		CatInteractable Cat { get; }

		DialogInteractable[] Dialogs { get; }

		EndingLaunchInteractable DeathEndingInteractable { get; }

		ZoomInteractable[] ZoomInteractables { get; }

		CalendarInteractable CalendarInteractable { get; }
	}
}
