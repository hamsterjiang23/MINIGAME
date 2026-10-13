using System.Linq;
using UnityEngine;
using _Code.Characters;
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
			base.Init(dialogManager, dayNightController, closeUpsController, cursorController, soundService, hudPresenter, dataModelService);
			CloseUpsController.Fridge.ShowStarted -= OnFridgeOpen;
			CloseUpsController.Fridge.ShowStarted += OnFridgeOpen;
			CloseUpsController.Fridge.HideEnded -= OnFridgeClose;
			CloseUpsController.Fridge.HideEnded += OnFridgeClose;
		}

		private void OnFridgeClose()
		{
			UIButton[] uiButtons = _uiButtons;
			for (int i = 0; i < uiButtons.Length; i++)
			{
				uiButtons[i].IsActive = true;
			}
		}

		private void OnFridgeOpen()
		{
			UIButton[] uiButtons = _uiButtons;
			for (int i = 0; i < uiButtons.Length; i++)
			{
				uiButtons[i].IsActive = false;
			}
		}

		public void SetFridgeActivity(bool isActive)
		{
			_fridgeButton.IsActive = isActive;
		}

		public override void Enter()
		{
			base.Enter();
			CharacterRoomObjectView characterRoomObjectView = CharacterViews.FirstOrDefault((CharacterRoomObjectView x) => x.Data.CharacterType == ECharacterType.Blind);
			if (characterRoomObjectView.gameObject.activeSelf && characterRoomObjectView.SelectedState == ERoomPeopleState.Hanged)
			{
				SteamAchievements.Unlock("SPEC_BLIND_HANGED");
			}
		}
	}
}
