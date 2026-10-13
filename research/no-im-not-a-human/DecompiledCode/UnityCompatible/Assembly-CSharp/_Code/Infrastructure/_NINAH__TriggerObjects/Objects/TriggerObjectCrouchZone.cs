using UnityEngine;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.TriggerObjects;

namespace _Code.Infrastructure._NINAH__TriggerObjects.Objects
{
	public sealed class TriggerObjectCrouchZone : ATriggerObject
	{
		private IPlayerService _playerService;

		protected override void OnEnterInner(Collider other)
		{
			_playerService.IsCrouched = true;
		}

		protected override void OnExitInner(Collider other)
		{
			_playerService.IsCrouched = false;
		}

		public void InitModules(IPlayerService playerService)
		{
			_playerService = playerService;
		}
	}
}
