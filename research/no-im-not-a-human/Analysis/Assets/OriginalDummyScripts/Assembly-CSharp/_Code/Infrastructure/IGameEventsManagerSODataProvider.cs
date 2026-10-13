using _Code.Infrastructure.GameEvents;

namespace _Code.Infrastructure;

public interface IGameEventsManagerSODataProvider
{
	GameEventsManagerSOData GameEventsManagerData { get; }
}
