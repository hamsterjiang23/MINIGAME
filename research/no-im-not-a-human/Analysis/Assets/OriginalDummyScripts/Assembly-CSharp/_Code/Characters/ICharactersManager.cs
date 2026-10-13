using System;
using System.Collections.Generic;
using _Code.Events;
using _Code.Infrastructure.Rooms;
using _Code.Rooms;

namespace _Code.Characters;

public interface ICharactersManager
{
	int ImpostersCount { get; }

	int InnocentsCount { get; }

	int Count { get; }

	int InnocentsKilledCount { get; }

	int EverImposterInHouse { get; }

	IReadOnlyList<ECharacterType> CharactersInside { get; }

	int KilledByImpostersCount { get; }

	event Func<ERoom, ARoom> GetRoom;

	event Func<(int Day, ETimeOfDay CurrentTimeOfDay, float LastChange)> GetBaseDayNightData;

	event Action ScreamerShowed;

	event Action<ECharacterType> CharacterRemoved;

	event Action<ECharacterType, bool> CharacterAdded;

	void LetIn(CharacterSOData character, bool exileOldCharacterFromThisPlace = false);

	void Refuse(CharacterSOData character);

	void Kill(CharacterSOData character, bool isByPlayer = false);

	void Kill(ECharacterType character);

	CharacterSOData GetRandomCharacterToExile();

	void KillRandom();

	void ClearPovistkas();

	void GivePovistka(ECharacterType character);

	void KillRoom(CharacterSOData characterOfRoom);

	void ExileCharacter(ECharacterType character);

	void KillTomorrow(CharacterSOData character);

	void RunMorningOfSuicides();

	CharacterSOData[] ExilyByFEMA(int count);

	ERoom GetCharacterRoom(ECharacterType character);

	bool IsImposter(ECharacterType character);

	ECharacterPlace GetPlace(ECharacterType character);

	void BeginNewDay(int day);

	void OnGunStateSelected(bool b);

	void OnDialogStarted(ECharacterType characterCharacterType);

	void OnDayDialogTalked(bool hasAlreadyTalkedToday);

	void KillEveryone();

	int GetPossibleCharactersToKillCount();

	bool IsCharacterAliveInside(ECharacterType characterType);

	CharacterSOData GetCharacter(ECharacterType characterType);

	void ExileAfterTomorrow(CharacterSOData character);

	bool EverRefused();
}
