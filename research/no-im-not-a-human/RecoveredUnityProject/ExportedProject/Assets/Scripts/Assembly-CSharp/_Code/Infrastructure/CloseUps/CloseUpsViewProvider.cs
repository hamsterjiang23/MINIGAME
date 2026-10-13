using UnityEngine;
using _Code.Infrastructure.CloseUps.Views;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.CloseUps.Views.Radio;
using _Code.Infrastructure._NINAH__CloseUps.Views.Consumables;
using _Code.Infrastructure._NINAH__CloseUps.Views.Mushroomlist;

namespace _Code.Infrastructure.CloseUps
{
	public sealed class CloseUpsViewProvider : MonoBehaviour, ICloseUpsViewProvider
	{
		[field: SerializeField]
		public FridgeCloseUpView FridgeCloseUpView { get; private set; }

		[field: SerializeField]
		public PhoneCloseUpView PhoneCloseUpView { get; private set; }

		[field: SerializeField]
		public RadioCloseUpView RadioCloseUpView { get; private set; }

		[field: SerializeField]
		public MushroomlistCloseUpView MushroomlistCloseUpView { get; private set; }

		[field: SerializeField]
		public ConsumableCloseUpView ConsumableCloseUpView { get; private set; }
	}
}
