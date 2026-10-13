using System;
using UnityEngine;

namespace SABI;

public static class IntExtensions
{
	public static string ToAbbreviatedString(this int n, uint digits = 0u)
	{
		int num = Math.Abs(n);
		if (num < 1000)
		{
			return n.ToString() ?? "";
		}
		if (num < 1000000)
		{
			return ((decimal)n / 1000m).TruncateTo(digits) + "k";
		}
		if (num < 1000000000)
		{
			return ((decimal)n / 1000000m).TruncateTo(digits) + "m";
		}
		return ((decimal)n / 1000000000m).TruncateTo(digits) + "g";
	}

	public static int RoundToMultipleOf(this int n, int binSize)
	{
		int num = n / binSize * binSize;
		if (n < 0)
		{
			num -= binSize;
		}
		return num;
	}

	public static bool IsInRange(this int value, int minValue, int maxValue)
	{
		if (value >= minValue)
		{
			return value <= maxValue;
		}
		return false;
	}

	public static int ClosestInRange(this int value, int minValue, int maxValue)
	{
		if (value.IsInRange(minValue, maxValue))
		{
			return value;
		}
		int num = Mathf.Abs(value - minValue);
		int num2 = Mathf.Abs(value - maxValue);
		return (int)MathF.Min(num, num2);
	}

	public static int Max(this int value, int max)
	{
		if (value > max)
		{
			return max;
		}
		return value;
	}

	public static int Min(this int value, int min)
	{
		if (value > min)
		{
			return value;
		}
		return min;
	}

	public static int Clamp(this int value, int min, int max)
	{
		return Math.Max(min, Math.Min(max, value));
	}

	public static int Lerp(this int current, int target, float t)
	{
		t = Mathf.Clamp01(t);
		return Mathf.RoundToInt(Mathf.Lerp(current, target, t));
	}

	public static int LerpUnclamped(this int current, int target, float t)
	{
		return Mathf.RoundToInt(Mathf.LerpUnclamped(current, target, t));
	}

	public static int MoveTowards(this int current, int target, int maxDelta)
	{
		if (Mathf.Abs(target - current) <= maxDelta)
		{
			return target;
		}
		return current + (int)Mathf.Sign(target - current) * maxDelta;
	}

	public static int Remap(this int value, Vector2Int sourceRange, Vector2Int targetRange)
	{
		if (sourceRange.x == sourceRange.y)
		{
			return targetRange.x;
		}
		float t = (float)(value - sourceRange.x) / (float)(sourceRange.y - sourceRange.x);
		return Mathf.RoundToInt(Mathf.Lerp(targetRange.x, targetRange.y, t));
	}
}
