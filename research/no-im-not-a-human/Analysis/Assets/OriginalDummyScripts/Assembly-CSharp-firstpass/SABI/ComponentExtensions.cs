using UnityEngine;

namespace SABI
{
	public static class ComponentExtensions
	{
		public static T AddComponent<T>(this Component component) where T : Component
		{
			return null;
		}

		public static T GetOrAddComponent<T>(this Component component) where T : Component
		{
			return null;
		}

		public static bool HasComponent<T>(this Component component) where T : Component
		{
			return false;
		}

		public static void DestroyComponent<T>(this Component component) where T : Component
		{
		}

		public static bool TryGetComponentInParent<T>(this Component component, out T componentFound, bool includeInactive = false) where T : Component
		{
			componentFound = null;
			return false;
		}

		public static bool TryGetComponentInChildren<T>(this Component component, out T componentFound, bool includeInactive = false) where T : Component
		{
			componentFound = null;
			return false;
		}
	}
}
