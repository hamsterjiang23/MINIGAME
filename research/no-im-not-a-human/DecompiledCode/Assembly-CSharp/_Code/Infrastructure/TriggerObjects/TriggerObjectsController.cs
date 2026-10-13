using SABI;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.TriggerObjects.Objects;
using _Code.Infrastructure._NINAH__TriggerObjects.Objects;
using _Code.Menues.HUD;

namespace _Code.Infrastructure.TriggerObjects;

public sealed class TriggerObjectsController : ITriggerObjectsController
{
	private TriggerObjectGoToLocation _preDeathPortal;

	private TriggerObjectFollowLight _followLight;

	private TriggerObjectRunZone[] _runZones;

	public TriggerObjectsController(ITriggerObjectsProvider triggerObjectsProvider, ILocationsManager locationsManager, IHUDPresenter hudPresenter, IPlayerService playerService)
	{
		_preDeathPortal = triggerObjectsProvider.PreDeathPortal;
		_runZones = triggerObjectsProvider.RunZones;
		_followLight = triggerObjectsProvider.FollowLight;
		_preDeathPortal.InitModules(locationsManager);
		_runZones.ForEach((TriggerObjectRunZone x) =>
		{
			x.InitModules(hudPresenter, playerService);
		});
		triggerObjectsProvider.CrouchZones.ForEach((TriggerObjectCrouchZone x) =>
		{
			x.InitModules(playerService);
		});
	}
}
