using UnityEngine;
using _Code.DialogSystem;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.StateObjects;
using _Code.Menues.HUD;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms
{
	public sealed class BigRoomView : ARoomView
	{
		[SerializeField]
		private UIButton _appleButton;

		[SerializeField]
		private Material _roomBlood;

		public void Init(IHUDPresenter hudPresenter, IDialogManager dialogManager, IGameplayEndingManager gameplayEndingManager, IStateObjectController stateObjectController)
		{
		}

		public void RevealBlood()
		{
		}

		public void CleanBlood()
		{
		}
	}
}
