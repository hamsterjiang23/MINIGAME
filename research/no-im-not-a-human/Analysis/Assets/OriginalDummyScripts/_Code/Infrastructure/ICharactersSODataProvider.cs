using _Code.Characters;

namespace _Code.Infrastructure
{
	public interface ICharactersSODataProvider
	{
		CharacterSOData[] CharactersData { get; }
	}
}
