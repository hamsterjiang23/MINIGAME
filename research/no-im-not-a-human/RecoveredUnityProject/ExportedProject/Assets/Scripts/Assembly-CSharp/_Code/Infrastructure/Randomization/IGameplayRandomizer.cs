using _Code.Characters;

namespace _Code.Infrastructure.Randomization
{
	public interface IGameplayRandomizer
	{
		int CompletedGameTimesCountAtBegin { get; }

		(CharacterSOData character, int order)[] GetRandomCharactersForDay(int day, (int, int) charactersCount);

		int GetFirstEmptyVisitSlot(int day);
	}
}
