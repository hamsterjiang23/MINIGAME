using UnityEngine;
using _Scripts.Services.Sound.Instance;

namespace _Scripts.Services.Sound.Service
{
	public sealed class SoundServiceInstanceProvider : MonoBehaviour, ISoundServiceInstanceProvider
	{
		[field: SerializeField]
		public SoundServiceInstance SoundServiceInstance { get; private set; }
	}
}
