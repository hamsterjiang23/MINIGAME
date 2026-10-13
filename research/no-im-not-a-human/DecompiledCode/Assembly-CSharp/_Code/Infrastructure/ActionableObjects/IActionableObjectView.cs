using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.ActionableObjects;

public interface IActionableObjectView
{
	void Init(IHUDPresenter hudPresenter, IDayNightController dayNightController, IRoomsManager roomsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService notAHumanSoundService, IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager);

	void InitModules(IDialogManager dialogManager);

	void TryLeave();

	void SetLockedState(bool isLocked);
}
