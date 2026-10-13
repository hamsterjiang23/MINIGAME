using System.Collections.Generic;
using UnityEngine;

namespace SABI
{
	public static class DictionaryExtensions
	{
		public static Dictionary<TKey, TValue> AddOrUpdate<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			return null;
		}

		public static TKey GetKeyByValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey value)
		{
			return default;
		}

		public static bool ContainsAndNotNull<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : Object
		{
			return false;
		}
	}
}
