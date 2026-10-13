using System;
using UnityEngine;

namespace SABI
{
	public static class Vector2Extensions
	{
		public static Vector2 SetValue(this Vector2 v, float value)
		{
			return new Vector2(value, value);
		}

		public static Vector2 WithX(this Vector2 vector, float x)
		{
			return new Vector2(x, vector.y);
		}

		public static Vector2 WithY(this Vector2 vector, float y)
		{
			return new Vector2(vector.x, y);
		}

		public static Vector2 SetMagnitude(this Vector2 vector, float magnitude)
		{
			return vector.normalized * magnitude;
		}

		public static Vector2 WithAddX(this Vector2 v, float x)
		{
			return new Vector2(v.x + x, v.y);
		}

		public static Vector2 WithSubtractX(this Vector2 v, float x)
		{
			return new Vector2(v.x - x, v.y);
		}

		public static Vector2 WithMultiplyX(this Vector2 v, float x)
		{
			return new Vector2(v.x * x, v.y);
		}

		public static Vector2 WithAddY(this Vector2 v, float y)
		{
			return new Vector2(v.x, v.y + y);
		}

		public static Vector2 WithSubtractY(this Vector2 v, float y)
		{
			return new Vector2(v.x, v.y - y);
		}

		public static Vector2 WithMultiplyY(this Vector2 v, float y)
		{
			return new Vector2(v.x, v.y * y);
		}

		public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max)
		{
			return new Vector2(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));
		}

		public static Vector2 Clamp01(this Vector2 value)
		{
			return new Vector2(Mathf.Clamp01(value.x), Mathf.Clamp01(value.y));
		}

		public static Vector2 ClampMagnitude(this Vector2 vector, float maxLength)
		{
			return Vector2.ClampMagnitude(vector, maxLength);
		}

		public static Vector2 Lerp(this Vector2 current, Vector2 target, float t)
		{
			return Vector2.Lerp(current, target, Mathf.Clamp01(t));
		}

		public static Vector2 LerpUnclamped(this Vector2 current, Vector2 target, float t)
		{
			return Vector2.LerpUnclamped(current, target, t);
		}

		public static Vector2 MoveTowards(this Vector2 current, Vector2 target, float maxDelta)
		{
			return Vector2.MoveTowards(current, target, maxDelta);
		}

		public static Vector2 Max(this Vector2 a, Vector2 b)
		{
			return new Vector2(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
		}

		public static Vector2 Min(this Vector2 a, Vector2 b)
		{
			return new Vector2(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
		}

		public static Vector2 WithRandomBias(this Vector2 v, float biasValue)
		{
			return new Vector2(v.x.RandomBias(biasValue), v.y.RandomBias(biasValue));
		}

		public static Vector2 WithRandomBias(this Vector2 v, Vector2 biasValue)
		{
			return new Vector2(v.x.RandomBias(biasValue.x), v.y.RandomBias(biasValue.y));
		}

		public static Vector2 Remap(this Vector2 vector, Vector2 sourceMinMax, Vector2 targetMinMax)
		{
			return new Vector2(vector.x.Remap(sourceMinMax.x, sourceMinMax.y, targetMinMax.x, targetMinMax.y), vector.y.Remap(sourceMinMax.x, sourceMinMax.y, targetMinMax.x, targetMinMax.y));
		}

		public static bool IsUniform(this Vector2 vector)
		{
			return vector.x.Approximately(vector.y);
		}

		public static Vector2 Abs(this Vector2 vector)
		{
			return new Vector2(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
		}

		public static Vector2 Round(this Vector2 vector)
		{
			vector.x = Mathf.Round(vector.x);
			vector.y = Mathf.Round(vector.y);
			return vector;
		}

		public static Vector2 Rotate(this Vector2 vector, float angleInDegrees)
		{
			float num = Mathf.Sin(angleInDegrees * ((float)Math.PI / 180f));
			float num2 = Mathf.Cos(angleInDegrees * ((float)Math.PI / 180f));
			float x = vector.x;
			float y = vector.y;
			vector.x = num2 * x - num * y;
			vector.y = num * x + num2 * y;
			return vector;
		}
	}
}
