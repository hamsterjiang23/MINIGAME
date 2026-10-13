using UnityEngine;

namespace _Code.Player
{
	public sealed class InputHandlerProvider : MonoBehaviour, IInputHandlerProvider
	{
		[field: SerializeField]
		public InputHandling InputHandler { get; private set; }
	}
}
