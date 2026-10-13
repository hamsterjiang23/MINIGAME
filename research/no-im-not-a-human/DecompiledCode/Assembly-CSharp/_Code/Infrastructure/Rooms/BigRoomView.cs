using DG.Tweening;
using UnityEngine;
using _Code.Characters.DialogSystem;
using _Code.DialogSystem;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.StateObjects;
using _Code.Menues.HUD;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms;

public sealed class BigRoomView : ARoomView
{
	[SerializeField]
	private UIButton _appleButton;

	[SerializeField]
	private Material _roomBlood;

	public void Init(IHUDPresenter hudPresenter, IDialogManager dialogManager, IGameplayEndingManager gameplayEndingManager, IStateObjectController stateObjectController)
	{
		_appleButton.SetAction(() =>
		{
			dialogManager.ShowSubtitlePopup(EInfoMessageType.Apple);
			((GameplayEndingMushroom)gameplayEndingManager.GetEnding(EEnding.Mushroom)).FindApple();
			stateObjectController.SetState(EStateObjectType.Apple, 0);
		});
	}

	public void RevealBlood()
	{
		_roomBlood.DOFloat(1f, "_Reveal", 0.75f);
	}

	public void CleanBlood()
	{
		_roomBlood.SetFloat("_Reveal", 0f);
	}
}
