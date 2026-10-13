using _Code.Infrastructure.CloseUps.Views;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.CloseUps.Views.Radio;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__CloseUps.Views.Consumables;
using _Code.Infrastructure._NINAH__CloseUps.Views.Mushroomlist;

namespace _Code.Infrastructure.CloseUps;

public interface ICloseUpsController
{
	IUpdateable[] Updateables { get; }

	FridgeCloseUpView Fridge { get; }

	PhoneCloseUpView Phone { get; }

	RadioCloseUpView Radio { get; }

	ConsumableCloseUpView Consumable { get; }

	bool IsAnyCloseUpActive { get; }

	MushroomlistCloseUpView Mushroomlist { get; }

	int FemaCallsCount { get; }

	void UnlockPhoneSubscriber(EPhoneSubscriber subscriber);

	string GetPhoneNumber(EPhoneSubscriber subscriber);
}
