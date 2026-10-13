using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Sound;

namespace _Code.Infrastructure.TriggerObjects.Objects
{
	public sealed class TriggerObjectOpenDoor : ATriggerObject
	{
		[SerializeField]
		private Transform _doorTransform;

		[SerializeField]
		private Vector3 _rotateAngle;

		[SerializeField]
		private float _duration;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _sounds;

		private CommonEnumEventus _commonEventBus;

		private bool _isTriggered;

		protected override void OnEnterInner(Collider other)
		{
		}

		public void Init(CommonEnumEventus eventus)
		{
		}
	}
}
