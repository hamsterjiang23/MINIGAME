using System;
using UnityEngine;

namespace AYellowpaper.SerializedCollections;

public class SerializedDictionarySampleTwo : MonoBehaviour
{
	[Serializable]
	public class Person
	{
		public string FirstName;

		public string LastName;
	}

	public SerializedDictionary<int, Person> People;
}
