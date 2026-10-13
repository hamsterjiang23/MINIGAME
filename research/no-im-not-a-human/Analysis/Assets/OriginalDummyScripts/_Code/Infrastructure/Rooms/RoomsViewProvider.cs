using UnityEngine;
using UnityEngine.UI;

namespace _Code.Infrastructure.Rooms
{
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

		public ARoomView Kitchen => null;

		public ARoomView Office => null;

		public ARoomView Bedroom => null;

		public ARoomView BigRoom => null;

		public ARoomView Bathroom => null;

		public ARoomView Pantry => null;

		public ARoomView Entrance => null;

		public Canvas RoomDisplayerCanvas => null;

		public Image RoomDisplayerOverlay => null;
	}
}
