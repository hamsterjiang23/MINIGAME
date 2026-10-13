using ECM2;
using UnityEngine;
using _Code.Player;

namespace _Code.Infrastructure.Player
{
	public sealed class PlayerInstance : MonoBehaviour
	{
		[field: SerializeField]
		public Character Character { get; private set; }

		[field: SerializeField]
		public PlayerController PlayerController { get; private set; }

		[field: SerializeField]
		public PlayerFootstepSoundPlayer PlayerFootstepSoundPlayer { get; private set; }

		[field: SerializeField]
		public PlayerBodyeaterstepSoundPlayer PlayerBodyeaterstepPlayer { get; private set; }

		public void Init(InputHandling inputHandler)
		{
		}
	}
}
