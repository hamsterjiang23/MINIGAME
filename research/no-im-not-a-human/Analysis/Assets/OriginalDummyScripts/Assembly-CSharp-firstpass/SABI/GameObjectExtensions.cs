using UnityEngine;

namespace SABI
{
	public static class GameObjectExtensions
	{
		public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
		{
			return null;
		}

		public static bool HasComponent<T>(this GameObject gameObject) where T : Component
		{
			return false;
		}

		public static GameObject ToggleActive(this GameObject gameObject)
		{
			return null;
		}

		public static GameObject DestroyAllChildren(this GameObject gameObject)
		{
			return null;
		}

		public static GameObject AddComponentIfMissing<T>(this GameObject gameObject) where T : Component
		{
			return null;
		}

		public static bool TryGetComponentInChildren<T>(this GameObject gameObject, out T component, bool includeInactive = false) where T : Component
		{
			component = null;
			return false;
		}

		public static bool TryGetComponentInParent<T>(this GameObject gameObject, out T component, bool includeInactive = false) where T : Component
		{
			component = null;
			return false;
		}
	}
}
