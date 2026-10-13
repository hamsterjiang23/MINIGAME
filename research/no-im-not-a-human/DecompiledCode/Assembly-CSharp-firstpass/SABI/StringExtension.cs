using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace SABI;

public static class StringExtension
{
	public static T ToEnum<T>(this string str) where T : struct, Enum
	{
		if (Enum.TryParse<T>(str, ignoreCase: true, out var result))
		{
			return result;
		}
		throw new ArgumentException("Cannot convert '" + str + "' to enum " + typeof(T).Name);
	}

	public static string Truncate(this string str, int maxLength)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		if (str.Length > maxLength)
		{
			return str.Substring(0, maxLength);
		}
		return str;
	}

	public static string ToTitleCase(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
	}

	public static bool IsNullOrEmpty(this string str)
	{
		return string.IsNullOrEmpty(str);
	}

	public static bool IsNullOrWhiteSpace(this string str)
	{
		return string.IsNullOrWhiteSpace(str);
	}

	public static string Reverse(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		char[] array = str.ToCharArray();
		Array.Reverse(array);
		return new string(array);
	}

	public static string RemoveWhitespace(this string str)
	{
		return new string(str.Where((char c) => !char.IsWhiteSpace(c)).ToArray());
	}

	public static string ToCamelCase(this string str)
	{
		if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
		{
			return str;
		}
		string text = char.ToLower(str[0]).ToString();
		if (str.Length > 1)
		{
			text += str.Substring(1);
		}
		return text;
	}

	public static string SplitCamelCase(this string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return str;
		}
		return Regex.Replace(str, "([a-z])([A-Z])", "$1 $2");
	}
}
