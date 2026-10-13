using Cysharp.Threading.Tasks;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.StateObjects;
using _Code.Menues.HUD;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Rooms;

public sealed class Bathroom : ARoom
{
	private readonly IGameplayEndingManager _gameplayEndingManager;

	private readonly IDialogManager _dialogManager;

	private readonly IDayNightController _dayNightController;

	public override ERoom RoomType => ERoom.Bathroom;

	private BathroomView BathroomView => (BathroomView)View;

	public Bathroom(IDayNightController dayNightController, IOtherGameSODataProvider otherGameSoDataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, ICharactersManager charactersManager, IDataModelService dataModelService, IStateObjectController stateObjectController)
		: base(dayNightController, otherGameSoDataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController)
	{
		AwaitViewInitThenInit().Forget();
		_gameplayEndingManager = gameplayEndingManager;
		_dialogManager = dialogManager;
		_dayNightController = dayNightController;
	}

	private async UniTask AwaitViewInitThenInit()
	{
		await UniTask.WaitUntil(() => BathroomView != null);
		BathroomView.Init(_gameplayEndingManager, _dialogManager, _dayNightController);
	}

	public void ActivateBaby()
	{
		BathroomView.ActivateBaby();
	}
}
