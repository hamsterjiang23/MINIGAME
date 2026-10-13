using UnityEngine;
using UnityEngine.SceneManagement;

namespace SABI
{
	public static class SceneExtensions
	{
		public static T FindObjectOfType<T>(this Scene scene, bool includeInactive = false) where T : Component
		{
			return null;
		}

		public static T[] FindObjectsOfType<T>(this Scene scene, bool includeInactive = false)
		{
			return null;
		}
	}
}
