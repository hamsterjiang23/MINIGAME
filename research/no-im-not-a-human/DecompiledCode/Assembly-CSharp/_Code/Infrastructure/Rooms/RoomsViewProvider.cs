using UnityEngine;
using UnityEngine.UI;

namespace _Code.Infrastructure.Rooms;

public sealed class RoomsViewProvider : MonoBehaviour, IRoomsViewProvider
{
	[SerializeField]
	private Canvas _roomDisplayerCanvas;

	[SerializeField]
	private Image _roomDisplayerOverlay;

	[SerializeField]
	private KitchenView _kitchen;

	[SerializeField]
	private OfficeView _office;

	[SerializeField]
	private BedroomView _bedroom;

	[SerializeField]
	private BigRoomView _bigRoom;

	[SerializeField]
	private BathroomView _bathroom;

	[SerializeField]
	private PantryView _pantry;

	[SerializeField]
	private EntranceRoomView _entrance;

	public ARoomView Kitchen => _kitchen;

	public ARoomView Office => _office;

	public ARoomView Bedroom => _bedroom;

	public ARoomView BigRoom => _bigRoom;

	public ARoomView Bathroom => _bathroom;

	public ARoomView Pantry => _pantry;

	public ARoomView Entrance => _entrance;

	public Canvas RoomDisplayerCanvas => _roomDisplayerCanvas;

	public Image RoomDisplayerOverlay => _roomDisplayerOverlay;
}
