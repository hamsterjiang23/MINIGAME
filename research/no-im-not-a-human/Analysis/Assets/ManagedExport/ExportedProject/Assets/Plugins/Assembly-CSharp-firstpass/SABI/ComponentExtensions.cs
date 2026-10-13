using UnityEngine;

namespace SABI
{
	public static class ComponentExtensions
	{
		public static T AddComponent<T>(this Component component) where T : Component
		{
			return component.gameObject.AddComponent<T>();
		}

		public static T GetOrAddComponent<T>(this Component component) where T : Component
		{
			if (!component.TryGetComponent<T>(out var component2))
			{
				return component.AddComponent<T>();
			}
			return component2;
		}

		public static bool HasComponent<T>(this Component component) where T : Component
		{
			T component2;
			return component.TryGetComponent<T>(out component2);
		}

		public static void DestroyComponent<T>(this Component component) where T : Component
		{
			if (component.TryGetComponent<T>(out var component2))
			{
				Object.Destroy(component2);
			}
		}

		public static bool TryGetComponentInParent<T>(this Component component, out T componentFound, bool includeInactive = false) where T : Component
		{
			componentFound = component.gameObject.GetComponentInParent<T>(includeInactive);
			return componentFound != null;
		}

		public static bool TryGetComponentInChildren<T>(this Component component, out T componentFound, bool includeInactive = false) where T : Component
		{
			componentFound = component.gameObject.GetComponentInChildren<T>(includeInactive);
			return componentFound != null;
		}
	}
}
