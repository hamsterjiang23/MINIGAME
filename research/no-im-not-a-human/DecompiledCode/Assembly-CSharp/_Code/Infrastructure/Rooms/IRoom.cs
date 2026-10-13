using System;
using _Code.Characters;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms;

public interface IRoom
{
	bool HasDialogNow { get; }

	float CameraDistance { get; }

	bool SuspendedExit { get; }

	event Action<ARoom> Entered;

	event Action<ARoom> Left;

	event Action DialogStarted;

	event Action DialogFinished;

	void AddCharacter(ECharacterType character, bool isAddSilently = false);

	void KillCharacter(ECharacterType character);

	void ExileCharacter(ECharacterType character);

	void StopWhispering(float time);

	void StartWhispering(float time);

	void Enter();

	void Leave();

	void ChangeCharacterPose(ECharacterType character, ERoomPeopleState pose);

	void EnableObjects();

	void DisableObjects();

	void StartLeavingRoom();

	void StopLeavingRoom();
}
