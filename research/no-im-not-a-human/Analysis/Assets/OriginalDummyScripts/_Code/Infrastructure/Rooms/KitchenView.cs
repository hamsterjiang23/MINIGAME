using UnityEngine;
using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Menues.HUD;
using _Code.Rooms;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Rooms
{
	public sealed class KitchenView : ARoomView
	{
		[SerializeField]
		private UIButton _fridgeButton;

		public override void Init(IDialogManager dialogManager, IDayNightController dayNightController, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IHUDPresenter hudPresenter, IDataModelService dataModelService)
		{
		}

		private void OnFridgeClose()
		{
		}

		private void OnFridgeOpen()
		{
		}

		public void SetFridgeActivity(bool isActive)
		{
		}

		public override void Enter()
		{
		}
	}
}
