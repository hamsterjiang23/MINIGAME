using UnityEngine;
using _Code.Infrastructure.DayNight;

namespace _Code.Rooms
{
	public sealed class ObjectRoomObjectView : ARoomObjectView<EObjectsRoomObject>
	{
		[SerializeField]
		private RoomObjectState<EObjectsRoomObject>[] _states;

		[SerializeField]
		private EObjectsRoomObject _startState;

		private IDayNightController _dayNightController;

		protected override RoomObjectState<EObjectsRoomObject>[] States => null;

		protected override EObjectsRoomObject StartState => EObjectsRoomObject.Bed;

		protected override void Awake()
		{
		}

		private void ObjectActionHandler()
		{
		}

		private void Sleep()
		{
		}
	}
}
