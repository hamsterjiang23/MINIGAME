using UnityEngine;
using _Code.Infrastructure.TriggerObjects.Objects;

namespace _Code.Infrastructure.TriggerObjects
{
	public sealed class BodyEaterTriggerObjectsViewProvider : MonoBehaviour, IBodyEaterTriggerObjectsViewProvider
	{
		[field: SerializeField]
		public TriggerObjectOpenDoor[] OpenDoorTrigger { get; private set; }

		[field: SerializeField]
		public TriggerObjectCloseScene CloseSceneTrigger { get; private set; }
	}
}
