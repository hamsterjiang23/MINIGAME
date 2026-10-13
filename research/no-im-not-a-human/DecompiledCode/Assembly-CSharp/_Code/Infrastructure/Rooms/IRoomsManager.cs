using _Code.Characters;
using _Code.Events;
using _Code.Infrastructure.ActionableObjects;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms;

public interface IRoomsManager
{
	ARoom Kitchen { get; }

	ARoom Office { get; }

	ARoom Bedroom { get; }

	ARoom BigRoom { get; }

	ARoom Bathroom { get; }

	ARoom Pantry { get; }

	ARoom Entrance { get; }

	bool IsInRoom { get; }

	void WatchTV();

	void ClearCorpses();

	void ReinitWhispers(ETimeOfDay currentTimeOfDay);

	ARoom GetRoom(ERoom roomType);

	void LinkActionableToRoom(ERoom linkedRoom, AActionableObjectView actionableObject);

	void ChangeCharacterPose(ECharacterType character, ERoomPeopleState pose);
}
