using UnityEngine;

namespace SABI;

public static class Vector2IntExtensions
{
	public static Vector2Int SetValue(this Vector2Int v, int value)
	{
		return new Vector2Int(value, value);
	}

	public static Vector2Int WithX(this Vector2Int vector, int x)
	{
		return new Vector2Int(x, vector.y);
	}

	public static Vector2Int WithY(this Vector2Int vector, int y)
	{
		return new Vector2Int(vector.x, y);
	}

	public static Vector2Int WithAddX(this Vector2Int v, int x)
	{
		return new Vector2Int(v.x + x, v.y);
	}

	public static Vector2Int WithSubtractX(this Vector2Int v, int x)
	{
		return new Vector2Int(v.x - x, v.y);
	}

	public static Vector2Int WithMultiplyX(this Vector2Int v, int x)
	{
		return new Vector2Int(v.x * x, v.y);
	}

	public static Vector2Int WithAddY(this Vector2Int v, int y)
	{
		return new Vector2Int(v.x, v.y + y);
	}

	public static Vector2Int WithSubtractY(this Vector2Int v, int y)
	{
		return new Vector2Int(v.x, v.y - y);
	}

	public static Vector2Int WithMultiplyY(this Vector2Int v, int y)
	{
		return new Vector2Int(v.x, v.y * y);
	}

	public static Vector2Int Clamp(this Vector2Int value, Vector2Int min, Vector2Int max)
	{
		return new Vector2Int(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));
	}

	public static Vector2Int Max(this Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
	}

	public static Vector2Int Min(this Vector2Int a, Vector2Int b)
	{
		return new Vector2Int(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y));
	}

	public static Vector2Int Remap(this Vector2Int vector, Vector2Int sourceRange, Vector2Int targetRange)
	{
		return new Vector2Int(vector.x.Remap(sourceRange, targetRange), vector.y.Remap(sourceRange, targetRange));
	}

	public static Vector2Int Abs(this Vector2Int vector)
	{
		return new Vector2Int(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
	}
}
