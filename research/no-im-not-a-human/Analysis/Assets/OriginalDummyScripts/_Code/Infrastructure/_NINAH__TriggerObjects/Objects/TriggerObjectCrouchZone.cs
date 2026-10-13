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
		}

		protected override void OnExitInner(Collider other)
		{
		}

		public void InitModules(IPlayerService playerService)
		{
		}
	}
}
