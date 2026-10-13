using _Code.Infrastructure.Locations;

namespace _Code.Infrastructure.Player;

public interface IPlayerViewProvider
{
	PlayerInstance PlayerInstance { get; }

	StartPoint AfterSaveStartPoint { get; }
}
