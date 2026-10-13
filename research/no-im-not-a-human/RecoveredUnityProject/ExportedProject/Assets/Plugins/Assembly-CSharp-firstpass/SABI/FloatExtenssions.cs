using System;
using UnityEngine;

namespace SABI
{
	public static class FloatExtenssions
	{
		public static float RandomBias(this float value, float range, bool useNegativeBias = true)
		{
			return value + value * UnityEngine.Random.Range(useNegativeBias ? (0f - range) : 0f, range);
		}

		public static float Round(this float value, int digits = 0)
		{
			return (float)Math.Round(value, digits);
		}

		public static bool IsApproximatelyEqual(this float value, float other, float tolerance = 0.0001f)
		{
			return Math.Abs(value - other) < tolerance;
		}

		public static float ToPercentage(this float value, float total)
		{
			return value / total * 100f;
		}

		public static bool IsInRange(this float value, float minValue, float maxValue)
		{
			if (value >= minValue)
			{
				return value <= maxValue;
			}
			return false;
		}

		public static float ClosestInRange(this float value, float minValue, float maxValue)
		{
			if (value.IsInRange(minValue, maxValue))
			{
				return value;
			}
			float x = Mathf.Abs(value - minValue);
			float y = Mathf.Abs(value - maxValue);
			return MathF.Min(x, y);
		}

		public static float Max(this float value, float max)
		{
			if (!(value <= max))
			{
				return max;
			}
			return value;
		}

		public static float Min(this float value, float min)
		{
			if (!(value <= min))
			{
				return value;
			}
			return min;
		}

		public static float Clamp(this float value, float min, float max)
		{
			return Mathf.Clamp(value, min, max);
		}

		public static float Lerp(this float current, float target, float t)
		{
			return Mathf.Lerp(current, target, Mathf.Clamp01(t));
		}

		public static float LerpUnclamped(this float current, float target, float t)
		{
			return Mathf.LerpUnclamped(current, target, t);
		}

		public static float MoveTowards(this float current, float target, float maxDelta)
		{
			return Mathf.MoveTowards(current, target, maxDelta);
		}

		public static float Remap(this float value, float min1, float max1, float min2, float max2)
		{
			return min2 + (value - min1) * (max2 - min2) / (max1 - min1);
		}

		public static bool Approximately(this float value, float other)
		{
			return Mathf.Approximately(value, other);
		}
	}
}
