using UnityEngine;
using _Code.Infrastructure.Locations;
using _Code.Menues.HUD;

namespace _Code.Infrastructure.TriggerObjects.Objects
{
	public sealed class TriggerObjectGoToLocation : ATriggerObject
	{
		[SerializeField]
		private ELocation _location;

		private ILocationsManager _locationsManager;

		private IHUDPresenter _hudPresenter;

		public void InitModules(ILocationsManager locationsManager)
		{
		}

		protected override void OnEnterInner(Collider other)
		{
		}
	}
}
