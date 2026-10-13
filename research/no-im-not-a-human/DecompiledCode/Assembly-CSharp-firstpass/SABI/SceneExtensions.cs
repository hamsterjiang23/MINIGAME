using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SABI;

public static class SceneExtensions
{
	public static T FindObjectOfType<T>(this Scene scene, bool includeInactive = false) where T : Component
	{
		foreach (GameObject item in from item in scene.GetRootGameObjects()
			where item.activeSelf | includeInactive
			select item)
		{
			if (item.TryGetComponentInChildren<T>(out var component, includeInactive))
			{
				return component;
			}
		}
		return null;
	}

	public static T[] FindObjectsOfType<T>(this Scene scene, bool includeInactive = false)
	{
		return (from item in scene.GetRootGameObjects()
			where item.activeSelf | includeInactive
			select item).SelectMany((GameObject gameObject) => gameObject.GetComponentsInChildren<T>(includeInactive)).ToArray();
	}
}
