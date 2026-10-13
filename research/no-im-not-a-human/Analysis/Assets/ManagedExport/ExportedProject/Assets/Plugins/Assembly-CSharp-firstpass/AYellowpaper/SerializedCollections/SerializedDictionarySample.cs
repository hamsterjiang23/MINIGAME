using UnityEngine;

namespace AYellowpaper.SerializedCollections
{
	public class SerializedDictionarySample : MonoBehaviour
	{
		public enum ElementType
		{
			Fire = 0,
			Air = 1,
			Earth = 2,
			Water = 3
		}

		public SerializedDictionary<ElementType, string> ElementDescriptions;
	}
}
