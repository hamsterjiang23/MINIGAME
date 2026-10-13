using UnityEngine;

namespace AYellowpaper.SerializedCollections;

public class SerializedDictionarySample : MonoBehaviour
{
	public enum ElementType
	{
		Fire,
		Air,
		Earth,
		Water
	}

	public SerializedDictionary<ElementType, string> ElementDescriptions;
}
