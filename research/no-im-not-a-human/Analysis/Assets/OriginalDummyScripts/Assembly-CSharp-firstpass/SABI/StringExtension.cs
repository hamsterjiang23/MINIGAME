using System;

namespace SABI
{
	public static class StringExtension
	{
		public static T ToEnum<T>(this string str) where T : struct, Enum
		{
			return default;
		}

		public static string Truncate(this string str, int maxLength)
		{
			return null;
		}

		public static string ToTitleCase(this string str)
		{
			return null;
		}

		public static bool IsNullOrEmpty(this string str)
		{
			return false;
		}

		public static bool IsNullOrWhiteSpace(this string str)
		{
			return false;
		}

		public static string Reverse(this string str)
		{
			return null;
		}

		public static string RemoveWhitespace(this string str)
		{
			return null;
		}

		public static string ToCamelCase(this string str)
		{
			return null;
		}

		public static string SplitCamelCase(this string str)
		{
			return null;
		}
	}
}
