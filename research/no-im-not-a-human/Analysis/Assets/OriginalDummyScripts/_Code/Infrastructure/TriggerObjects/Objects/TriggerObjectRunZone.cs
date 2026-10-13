using UnityEngine;
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
		}

		protected override void OnExitInner(Collider other)
		{
		}

		public void InitModules(IHUDPresenter hudPresenter, IPlayerService playerService)
		{
		}
	}
}
