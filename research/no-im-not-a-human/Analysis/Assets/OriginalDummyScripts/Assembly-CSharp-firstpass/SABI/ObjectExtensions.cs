using System;
using UnityEngine;

namespace SABI
{
	public static class ObjectExtensions
	{
		public static void DestroyGameObject(this UnityEngine.Object value, float delay = 0f)
		{
		}

		public static bool EqualsToAny(this object obj, params object[] objects)
		{
			return false;
		}

		public static bool Spawn(this object obj, GameObject objectToInstantiate, float radius, int count, Vector3? boundsThatCantOverlap = null, Action<GameObject> OnObjectCreated = null)
		{
			return false;
		}
	}
}
