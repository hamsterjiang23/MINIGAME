using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RandomElementFromCollectionExtension
{
	private static int _lastIndex;

	public static T GetRandomElement<T>(this IEnumerable<T> array)
	{
		int num = array.Count();
		if (num == 0)
		{
			return default;
		}
		return array.ElementAt(Random.Range(0, num));
	}

	public static T GetRandomElementWithoutRepeating<T>(this IEnumerable<T> array)
	{
		int num = array.Count();
		if (num == 0)
		{
			return default;
		}
		int num2 = 0;
		do
		{
			num2 = Random.Range(0, num);
		}
		while (_lastIndex == num2);
		_lastIndex = num2;
		return array.ElementAt(num2);
	}
}
