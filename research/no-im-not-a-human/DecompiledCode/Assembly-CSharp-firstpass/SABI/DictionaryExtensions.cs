using System.Collections.Generic;
using UnityEngine;

namespace SABI;

public static class DictionaryExtensions
{
	public static Dictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (dictionary.ContainsKey(key))
		{
			dictionary[key] = value;
		}
		else
		{
			dictionary.Add(key, value);
		}
		return dictionary;
	}

	public static TKey GetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey value)
	{
		TKey result = default;
		foreach (KeyValuePair<TKey, TValue> item in dictionary)
		{
			if (item.Value.Equals(value))
			{
				result = item.Key;
				return result;
			}
		}
		return result;
	}

	public static bool ContainsAndNotNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : Object
	{
		if (dictionary.ContainsKey(key))
		{
			return dictionary[key] != null;
		}
		return false;
	}
}
