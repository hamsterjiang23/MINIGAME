using _Code.Infrastructure.TriggerObjects.Objects;

namespace _Code.Infrastructure.TriggerObjects
{
	public interface IBodyEaterTriggerObjectsViewProvider
	{
		TriggerObjectOpenDoor[] OpenDoorTrigger { get; }

		TriggerObjectCloseScene CloseSceneTrigger { get; }
	}
}
