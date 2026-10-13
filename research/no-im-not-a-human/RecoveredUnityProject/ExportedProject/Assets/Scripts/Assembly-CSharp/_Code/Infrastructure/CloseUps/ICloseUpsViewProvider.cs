using _Code.Infrastructure.CloseUps.Views;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.CloseUps.Views.Radio;
using _Code.Infrastructure._NINAH__CloseUps.Views.Consumables;
using _Code.Infrastructure._NINAH__CloseUps.Views.Mushroomlist;

namespace _Code.Infrastructure.CloseUps
{
	public interface ICloseUpsViewProvider
	{
		FridgeCloseUpView FridgeCloseUpView { get; }

		PhoneCloseUpView PhoneCloseUpView { get; }

		RadioCloseUpView RadioCloseUpView { get; }

		MushroomlistCloseUpView MushroomlistCloseUpView { get; }

		ConsumableCloseUpView ConsumableCloseUpView { get; }
	}
}
