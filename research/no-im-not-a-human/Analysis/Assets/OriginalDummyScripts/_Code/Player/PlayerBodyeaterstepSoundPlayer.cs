using ECM2;
using UnityEngine;
using _Code.Infrastructure.EnumEventBus;

namespace _Code.Player
{
	public sealed class PlayerBodyeaterstepSoundPlayer : MonoBehaviour
	{
		[SerializeField]
		private Character _character;

		private bool _isMovingNow;

		private CommonEnumEventus _commonEnumEventus;

		private void Update()
		{
		}

		private void Move()
		{
		}

		private void Stop()
		{
		}

		public void InitModules(CommonEnumEventus commonEnumEventus)
		{
		}
	}
}
