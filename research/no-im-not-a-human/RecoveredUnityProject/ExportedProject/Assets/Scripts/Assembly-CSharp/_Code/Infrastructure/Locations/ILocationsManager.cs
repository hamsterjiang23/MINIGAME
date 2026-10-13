using System;
using Cysharp.Threading.Tasks;

namespace _Code.Infrastructure.Locations
{
	public interface ILocationsManager
	{
		ELocation CurrentLocation { get; }

		bool IsGoingThroughLocations { get; }

		event Action<ELocation> LocationChanged;

		UniTask GoToLocation(ELocation location);
	}
}
