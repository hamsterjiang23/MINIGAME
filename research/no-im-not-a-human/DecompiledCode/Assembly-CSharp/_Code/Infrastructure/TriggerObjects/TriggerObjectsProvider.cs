using UnityEngine;
using _Code.Infrastructure.TriggerObjects.Objects;
using _Code.Infrastructure._NINAH__TriggerObjects.Objects;

namespace _Code.Infrastructure.TriggerObjects;

public sealed class TriggerObjectsProvider : MonoBehaviour, ITriggerObjectsProvider
{
	[field: SerializeField]
	public TriggerObjectGoToLocation PreDeathPortal { get; private set; }

	[field: SerializeField]
	public TriggerObjectRunZone[] RunZones { get; private set; }

	[field: SerializeField]
	public TriggerObjectCrouchZone[] CrouchZones { get; private set; }

	[field: SerializeField]
	public TriggerObjectFollowLight FollowLight { get; private set; }
}
