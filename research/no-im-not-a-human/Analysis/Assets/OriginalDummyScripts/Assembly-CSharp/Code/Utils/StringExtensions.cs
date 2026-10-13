using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Code.Utils;

public static class StringExtensions
{
	public static string FirstLetterToUpper(this string value)
	{
		return char.ToUpper(value[0]) + value.Substring(1, value.Length - 1);
	}

	public static string ToMonospace(this string value, float scale = 1f)
	{
		return "<mspace=" + scale.ToString(CultureInfo.InvariantCulture) + "em>" + value + "</mspace>";
	}

	public static string ElementsToString<T>(this IEnumerable<T> enumerable, string separator = ", ")
	{
		return enumerable.Aggregate("", (string current, T element) =>
		{
			T val = element;
			return current + val?.ToString() + separator;
		}).Trim().Trim(separator[0]);
	}

	public static string ElementsToString<TK, TV>(this IDictionary<TK, TV> dictionary, string separator = "\n")
	{
		return dictionary.Aggregate("", (string current, KeyValuePair<TK, TV> element) => current + element.Key?.ToString() + ": " + element.Value?.ToString() + separator).Trim().Trim(separator[0]);
	}

	public static bool IsNullOrEmpty(this string value)
	{
		return string.IsNullOrEmpty(value);
	}
}
