using NaughtyAttributes;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.Rendering;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure._NINAH__Effects;

namespace _Code.Infrastructure.Locations;

public sealed class Location : MonoBehaviour
{
	[field: SerializeField]
	public ELocation LocationType { get; private set; }

	[field: SerializeField]
	public StartPoint StartPoint { get; private set; }

	[Header("Visuals")]
	[field: SerializeField]
	public bool OverrideVolumeProfile { get; private set; }

	[field: ShowIf("OverrideVolumeProfile")]
	[field: SerializeField]
	public VolumeProfile VolumeProfile { get; private set; }

	[field: SerializeField]
	public bool OverrideSkybox { get; private set; }

	[field: ShowIf("OverrideSkybox")]
	[field: SerializeField]
	public Material Skybox { get; private set; }

	[field: SerializeField]
	public bool OverrideFog { get; private set; }

	[field: ShowIf("OverrideFog")]
	[field: SerializeField]
	public FogData Fog { get; private set; }

	[Header("Audio")]
	[field: SerializeField]
	public ELocationAudioType MusicType { get; private set; }

	[field: SerializeField]
	[field: SearchableEnum]
	public ESound CustomMusic { get; private set; }

	[field: SerializeField]
	[field: SearchableEnum]
	public ESound AmbientLoop { get; private set; }
}
