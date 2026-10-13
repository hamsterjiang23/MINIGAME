using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SABI;

public static class ObjectExtensions
{
	public static void DestroyGameObject(this UnityEngine.Object value, float delay = 0f)
	{
		if (!(value == null))
		{
			if (value is MonoBehaviour monoBehaviour)
			{
				value = monoBehaviour.gameObject;
			}
			if (value is Transform transform)
			{
				value = transform.gameObject;
			}
			if (!Application.isPlaying)
			{
				UnityEngine.Object.DestroyImmediate(value);
			}
			else
			{
				UnityEngine.Object.Destroy(value, delay);
			}
		}
	}

	public static bool EqualsToAny(this object obj, params object[] objects)
	{
		return objects.Any((object o) => o.Equals(obj));
	}

	public static bool Spawn(this object obj, GameObject objectToInstantiate, float radius, int count, Vector3? boundsThatCantOverlap = null, Action<GameObject> OnObjectCreated = null)
	{
		int num = Mathf.RoundToInt(count);
		if (num <= 0)
		{
			return false;
		}
		Vector3 vector = Vector3.zero;
		if (obj is GameObject gameObject)
		{
			vector = gameObject.transform.position;
		}
		else if (obj is Component component)
		{
			vector = component.transform.position;
		}
		List<Vector3> list = new List<Vector3>();
		int num2 = 0;
		int num3 = 100 * num;
		while (num2 < num && num3 > 0)
		{
			num3--;
			Vector3 vector2 = vector + UnityEngine.Random.insideUnitSphere * radius;
			if (boundsThatCantOverlap.HasValue)
			{
				bool flag = false;
				foreach (Vector3 item in list)
				{
					if (Mathf.Abs(vector2.x - item.x) < boundsThatCantOverlap.Value.x && Mathf.Abs(vector2.y - item.y) < boundsThatCantOverlap.Value.y && Mathf.Abs(vector2.z - item.z) < boundsThatCantOverlap.Value.z)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			GameObject obj2 = UnityEngine.Object.Instantiate(objectToInstantiate, vector2, Quaternion.identity);
			OnObjectCreated?.Invoke(obj2);
			list.Add(vector2);
			num2++;
		}
		return num2 == num;
	}
}
