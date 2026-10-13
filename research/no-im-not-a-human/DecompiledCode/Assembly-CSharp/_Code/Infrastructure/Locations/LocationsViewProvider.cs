using UnityEngine;

namespace _Code.Infrastructure.Locations;

public sealed class LocationsViewProvider : MonoBehaviour, ILocationsViewProvider
{
	[field: SerializeField]
	public Location[] Locations { get; private set; }
}
