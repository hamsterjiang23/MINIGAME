using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Infrastructure.DayNight;

namespace _Code.Rooms;

public sealed class ObjectRoomObjectView : ARoomObjectView<EObjectsRoomObject>
{
	[SerializeField]
	private RoomObjectState<EObjectsRoomObject>[] _states;

	[SerializeField]
	private EObjectsRoomObject _startState;

	private IDayNightController _dayNightController;

	protected override RoomObjectState<EObjectsRoomObject>[] States => _states;

	protected override EObjectsRoomObject StartState => _startState;

	protected override void Awake()
	{
		base.Awake();
		if (SelectedState != EObjectsRoomObject.None)
		{
			SetAction(ObjectActionHandler);
		}
	}

	private void ObjectActionHandler()
	{
		switch (SelectedState)
		{
		case EObjectsRoomObject.Bed:
			Sleep();
			break;
		case EObjectsRoomObject.Fridge:
			CloseUpsController.Fridge.Show().Forget();
			break;
		}
	}

	private void Sleep()
	{
		DayNightController.Change();
	}
}
