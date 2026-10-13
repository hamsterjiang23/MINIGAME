using UnityEngine;
using UnityEngine.Rendering;

namespace _Code.Infrastructure._NINAH__Effects
{
	public sealed class EffectsViewProvider : MonoBehaviour, IEffectsViewProvider
	{
		[field: SerializeField]
		public Volume Volume { get; private set; }

		[field: SerializeField]
		public Camera Camera { get; private set; }

		[field: SerializeField]
		public VolumeProfile NightVolumeProfile { get; private set; }

		[field: SerializeField]
		public VolumeProfile DayVolumeProfile { get; private set; }

		[field: SerializeField]
		public GameObject RoomSmoke3D { get; private set; }
	}
}
