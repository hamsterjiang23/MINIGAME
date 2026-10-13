using UnityEngine;

namespace _Code.Infrastructure.DayNight;

public interface IDayNightControllerViewProvider
{
	GameObject[] Lights { get; }

	MeshRenderer[] LightBeamsRenderers { get; }
}
