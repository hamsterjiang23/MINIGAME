using UnityEngine;

namespace _Code.Infrastructure.Pause
{
	public sealed class PauseMenuViewProvider : MonoBehaviour, IPauseMenuViewProvider
	{
		[field: SerializeField]
		public PauseMenuView PauseMenuView { get; private set; }
	}
}
