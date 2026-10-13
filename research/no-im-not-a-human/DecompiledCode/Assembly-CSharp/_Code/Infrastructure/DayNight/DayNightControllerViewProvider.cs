using UnityEngine;

namespace _Code.Infrastructure.DayNight;

public sealed class DayNightControllerViewProvider : MonoBehaviour, IDayNightControllerViewProvider
{
	[field: SerializeField]
	public GameObject[] Lights { get; private set; }

	[field: SerializeField]
	public MeshRenderer[] LightBeamsRenderers { get; private set; }
}
