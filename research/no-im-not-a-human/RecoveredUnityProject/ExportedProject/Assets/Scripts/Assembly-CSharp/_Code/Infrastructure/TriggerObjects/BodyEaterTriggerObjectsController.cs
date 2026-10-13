using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.TriggerObjects.Objects;

namespace _Code.Infrastructure.TriggerObjects
{
	public sealed class BodyEaterTriggerObjectsController : ITriggerObjectsController
	{
		private readonly TriggerObjectOpenDoor[] _doorObjects;

		public BodyEaterTriggerObjectsController(IBodyEaterTriggerObjectsViewProvider bodyEaterTriggerObjectsViewProvider, CommonEnumEventus commonEnumEventus)
		{
			bodyEaterTriggerObjectsViewProvider.CloseSceneTrigger.Init(commonEnumEventus);
			_doorObjects = bodyEaterTriggerObjectsViewProvider.OpenDoorTrigger;
			TriggerObjectOpenDoor[] doorObjects = _doorObjects;
			for (int i = 0; i < doorObjects.Length; i++)
			{
				doorObjects[i].Init(commonEnumEventus);
			}
		}
	}
}
