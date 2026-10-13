using UnityEngine;

namespace _Code.Rooms
{
	public sealed class TestRoomObjectView : ARoomObjectView<ERoomTestObjectStates>
	{
		[SerializeField]
		private RoomObjectState<ERoomTestObjectStates>[] _states;

		[SerializeField]
		private ERoomTestObjectStates _startState;

		protected override RoomObjectState<ERoomTestObjectStates>[] States => null;

		protected override ERoomTestObjectStates StartState => ERoomTestObjectStates.Glasses1;

		protected override void Awake()
		{
		}
	}
}
