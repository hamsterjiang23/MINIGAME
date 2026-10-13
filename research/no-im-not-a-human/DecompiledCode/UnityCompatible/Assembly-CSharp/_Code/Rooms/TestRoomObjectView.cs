using UnityEngine;

namespace _Code.Rooms
{
	public sealed class TestRoomObjectView : ARoomObjectView<ERoomTestObjectStates>
	{
		[SerializeField]
		private RoomObjectState<ERoomTestObjectStates>[] _states;

		[SerializeField]
		private ERoomTestObjectStates _startState;

		protected override RoomObjectState<ERoomTestObjectStates>[] States => _states;

		protected override ERoomTestObjectStates StartState => _startState;

		protected override void Awake()
		{
			base.Awake();
			SetAction(() =>
			{
				if (SelectedState == ERoomTestObjectStates.Glasses1)
				{
					SelectState(ERoomTestObjectStates.Glasses2);
				}
				else if (SelectedState == ERoomTestObjectStates.Glasses2)
				{
					SelectState(ERoomTestObjectStates.Glasses1);
				}
			});
		}
	}
}
