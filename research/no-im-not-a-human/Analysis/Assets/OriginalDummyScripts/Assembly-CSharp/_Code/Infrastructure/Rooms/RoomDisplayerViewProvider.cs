using UnityEngine;

namespace _Code.Infrastructure.Rooms;

public sealed class RoomDisplayerViewProvider : MonoBehaviour, IRoomDisplayerViewProvider
{
	[field: SerializeField]
	public RoomDisplayer RoomDisplayer { get; private set; }
}
