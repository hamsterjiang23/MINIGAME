using System;
using System.Collections.Generic;
using UnityEngine;

namespace SABI
{
	public static class IListExtensions
	{
		public static bool IsNullOrEmpty<T>(this IList<T> list)
		{
			if (list != null)
			{
				return list.Count == 0;
			}
			return true;
		}

		public static T GetRandomItem<T>(this IList<T> _array)
		{
			if (_array == null)
			{
				throw new IndexOutOfRangeException(" [ array is null ] Cannot select a random item from null");
			}
			if (_array.Count == 0)
			{
				throw new IndexOutOfRangeException(" [ array is empty ] Cannot select a random item from an empty array");
			}
			return _array[UnityEngine.Random.Range(0, _array.Count)];
		}

		public static List<T> GetUniqeRandomItems<T>(this IList<T> list, int count)
		{
			if (count > list.Count)
			{
				throw new ArgumentException("Requested count is larger than list size");
			}
			List<T> list2 = new List<T>();
			List<T> list3 = new List<T>(list);
			for (int i = 0; i < count; i++)
			{
				int index = UnityEngine.Random.Range(0, list3.Count);
				list2.Add(list3[index]);
				list3.RemoveAt(index);
			}
			return list2;
		}

		public static IList<T> RemoveNulls<T>(this List<T> list) where T : class
		{
			list.RemoveAll((T item) => item == null);
			return list;
		}

		public static IList<T> Shuffle<T>(this IList<T> list)
		{
			int num = list.Count;
			while (num > 1)
			{
				num--;
				int index = UnityEngine.Random.Range(0, num + 1);
				T value = list[index];
				list[index] = list[num];
				list[num] = value;
			}
			return list;
		}

		public static T GetWeightedRandom<T>(this IList<T> list, IList<float> weights)
		{
			float num = 0f;
			foreach (float weight in weights)
			{
				num += weight;
			}
			float num2 = UnityEngine.Random.Range(0f, num);
			float num3 = 0f;
			for (int i = 0; i < list.Count; i++)
			{
				num3 += weights[i];
				if (num2 <= num3)
				{
					return list[i];
				}
			}
			return list[list.Count - 1];
		}

		public static IList<T> ForEach<T>(this IList<T> list, Action<T, int> action)
		{
			for (int i = 0; i < list.Count; i++)
			{
				action?.Invoke(list[i], i);
			}
			return list;
		}

		public static IList<T> ForEach<T>(this IList<T> list, Action<T> action)
		{
			for (int i = 0; i < list.Count; i++)
			{
				action?.Invoke(list[i]);
			}
			return list;
		}

		public static IList<T> Move<T>(this IList<T> list, int oldIndex, int newIndex)
		{
			if (oldIndex == newIndex)
			{
				return list;
			}
			T item = list[oldIndex];
			list.RemoveAt(oldIndex);
			list.Insert(newIndex, item);
			return list;
		}

		public static IList<T> Swap<T>(this IList<T> list, int index1, int index2)
		{
			if (index1 == index2)
			{
				return list;
			}
			T value = list[index1];
			list[index1] = list[index2];
			list[index2] = value;
			return list;
		}

		public static IList<T> Replace<T>(this IList<T> list, T oldItem, T newItem)
		{
			int index = list.IndexOf(oldItem);
			list[index] = newItem;
			return list;
		}

		public static IList<T> RemoveDuplicates<T>(this IList<T> list)
		{
			HashSet<T> hashSet = new HashSet<T>();
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!hashSet.Add(list[num]))
				{
					list.RemoveAt(num);
				}
			}
			return list;
		}

		public static T Pop<T>(this IList<T> list, int? index = null)
		{
			if (!index.HasValue)
			{
				index = list.Count - 1;
			}
			T result = list[index.Value];
			list.RemoveAt(index.Value);
			return result;
		}

		public static List<T> PopList<T>(this IList<T> list, params int[] indexes)
		{
			List<T> list2 = new List<T>();
			foreach (int value in indexes)
			{
				list2.Add(list.Pop(value));
			}
			return list2;
		}

		public static T PopRandom<T>(this IList<T> list)
		{
			int value = UnityEngine.Random.Range(0, list.Count);
			return list.Pop(value);
		}

		public static (T element, int index) PopRandomTuple<T>(this IList<T> list)
		{
			int num = UnityEngine.Random.Range(0, list.Count);
			return (element: list.Pop(num), index: num);
		}

		public static List<T> PopRandoms<T>(this IList<T> list, int count)
		{
			List<T> list2 = new List<T>();
			for (int i = 0; i < count; i++)
			{
				list2.Add(list.PopRandom());
			}
			return list2;
		}

		public static List<(T element, int index)> PopRandomsTupleList<T>(this IList<T> list, int count)
		{
			List<(T, int)> list2 = new List<(T, int)>();
			for (int i = 0; i < count; i++)
			{
				list2.Add(list.PopRandomTuple());
			}
			return list2;
		}

		public static IList<T> RemoveRange<T>(this IList<T> list, int index)
		{
			for (int i = list.Count - 1; i >= index; i++)
			{
				list.RemoveAt(i);
			}
			return list;
		}
	}
}
