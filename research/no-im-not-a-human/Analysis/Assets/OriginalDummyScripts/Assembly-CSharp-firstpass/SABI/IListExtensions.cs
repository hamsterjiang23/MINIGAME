using System;
using System.Collections.Generic;

namespace SABI
{
	public static class IListExtensions
	{
		public static bool IsNullOrEmpty<T>(this IList<T> list)
		{
			return false;
		}

		public static T GetRandomItem<T>(this IList<T> _array)
		{
			return default;
		}

		public static List<T> GetUniqeRandomItems<T>(this IList<T> list, int count)
		{
			return null;
		}

		public static IList<T> RemoveNulls<T>(this List<T> list) where T : class
		{
			return null;
		}

		public static IList<T> Shuffle<T>(this IList<T> list)
		{
			return null;
		}

		public static T GetWeightedRandom<T>(this IList<T> list, IList<float> weights)
		{
			return default;
		}

		public static IList<T> ForEach<T>(this IList<T> list, Action<T, int> action)
		{
			return null;
		}

		public static IList<T> ForEach<T>(this IList<T> list, Action<T> action)
		{
			return null;
		}

		public static IList<T> Move<T>(this IList<T> list, int oldIndex, int newIndex)
		{
			return null;
		}

		public static IList<T> Swap<T>(this IList<T> list, int index1, int index2)
		{
			return null;
		}

		public static IList<T> Replace<T>(this IList<T> list, T oldItem, T newItem)
		{
			return null;
		}

		public static IList<T> RemoveDuplicates<T>(this IList<T> list)
		{
			return null;
		}

		public static T Pop<T>(this IList<T> list, int? index = null)
		{
			return default;
		}

		public static List<T> PopList<T>(this IList<T> list, params int[] indexes)
		{
			return null;
		}

		public static T PopRandom<T>(this IList<T> list)
		{
			return default;
		}

		public static (T, int) PopRandomTuple<T>(this IList<T> list)
		{
			return default;
		}

		public static List<T> PopRandoms<T>(this IList<T> list, int count)
		{
			return null;
		}

		public static List<(T, int)> PopRandomsTupleList<T>(this IList<T> list, int count)
		{
			return null;
		}

		public static IList<T> RemoveRange<T>(this IList<T> list, int index)
		{
			return null;
		}
	}
}
