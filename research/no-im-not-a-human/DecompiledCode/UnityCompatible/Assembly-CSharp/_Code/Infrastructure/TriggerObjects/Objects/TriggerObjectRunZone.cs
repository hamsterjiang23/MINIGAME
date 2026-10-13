using UnityEngine;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Player;
using _Code.Menues.HUD;

namespace _Code.Infrastructure.TriggerObjects.Objects
{
	public sealed class TriggerObjectRunZone : ATriggerObject
	{
		private IPlayerService _playerService;

		private IHUDPresenter _hudPresenter;

		protected override void OnEnterInner(Collider other)
		{
			_hudPresenter.SetupAndShowControlsView(EControlsList.RunZone);
			_playerService.SetRunAvailability(isAvailable: true);
		}

		protected override void OnExitInner(Collider other)
		{
			_hudPresenter.HideControlsView();
			_playerService.SetRunAvailability(isAvailable: false);
		}

		public void InitModules(IHUDPresenter hudPresenter, IPlayerService playerService)
		{
			_playerService = playerService;
			_hudPresenter = hudPresenter;
		}
	}
}
