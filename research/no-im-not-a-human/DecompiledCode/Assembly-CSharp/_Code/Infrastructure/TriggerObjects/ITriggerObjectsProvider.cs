using _Code.Infrastructure.TriggerObjects.Objects;
using _Code.Infrastructure._NINAH__TriggerObjects.Objects;

namespace _Code.Infrastructure.TriggerObjects;

public interface ITriggerObjectsProvider
{
	TriggerObjectGoToLocation PreDeathPortal { get; }

	TriggerObjectRunZone[] RunZones { get; }

	TriggerObjectFollowLight FollowLight { get; }

	TriggerObjectCrouchZone[] CrouchZones { get; }
}
