using UnityEngine;

namespace _Code.Utils.Extensions;

public static class NormalizedAngle
{
	public static Vector3 NormalizeAngle(this Vector3 vector, float strength)
	{
		if (vector.x > 360f - strength)
		{
			vector.x -= 360f;
		}
		if (vector.y > 360f - strength)
		{
			vector.y -= 360f;
		}
		if (vector.z > 360f - strength)
		{
			vector.z -= 360f;
		}
		return vector;
	}
}
