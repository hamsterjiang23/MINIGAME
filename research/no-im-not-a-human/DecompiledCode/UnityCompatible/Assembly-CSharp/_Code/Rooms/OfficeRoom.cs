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

namespace _Code.Rooms
{
	public sealed class OfficeRoom : ARoom
	{
		private readonly IGameplayEndingManager _gameplayEndingManager;

		public override ERoom RoomType => ERoom.Office;

		private OfficeView OfficeView => (OfficeView)View;

		public OfficeRoom(IDayNightController dayNightController, IOtherGameSODataProvider otherGameSoDataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, ICharactersManager charactersManager, IDataModelService dataModelService, IStateObjectController stateObjectController)
			: base(dayNightController, otherGameSoDataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController)
		{
			AwaitViewInitThenInit().Forget();
			_gameplayEndingManager = gameplayEndingManager;
		}

		private async UniTask AwaitViewInitThenInit()
		{
			await UniTask.WaitUntil(() => OfficeView != null);
			OfficeView.Init(_gameplayEndingManager);
		}

		public void ActivateBaby()
		{
			OfficeView.ActivateBaby();
		}
	}
}
