using UnityEngine;

namespace RoboRyanTron.SearchableEnum
{
	[CreateAssetMenu]
	public class SearchableEnumDemo : ScriptableObject
	{
		[Tooltip("This enum is fucking miserable.")]
		public KeyCode LameKeyCode;

		[Tooltip("The finest enum browsing experience one can have.")]
		[SearchableEnum]
		public KeyCode AwesomeKeyCode;
	}
}
