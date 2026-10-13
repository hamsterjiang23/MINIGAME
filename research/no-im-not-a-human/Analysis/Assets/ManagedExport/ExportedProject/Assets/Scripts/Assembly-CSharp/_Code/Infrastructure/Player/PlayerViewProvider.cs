using UnityEngine;
using _Code.Infrastructure.Locations;

namespace _Code.Infrastructure.Player
{
	public sealed class PlayerViewProvider : MonoBehaviour, IPlayerViewProvider
	{
		[field: SerializeField]
		public PlayerInstance PlayerInstance { get; private set; }

		[field: SerializeField]
		public StartPoint AfterSaveStartPoint { get; private set; }
	}
}
