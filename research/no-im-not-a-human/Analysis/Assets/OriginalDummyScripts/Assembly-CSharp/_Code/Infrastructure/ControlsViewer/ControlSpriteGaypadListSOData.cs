using UnityEngine;

namespace _Code.Infrastructure.ControlsViewer;

[CreateAssetMenu(menuName = "Controls/ControlViewListGaypad")]
public sealed class ControlSpriteGaypadListSOData : ScriptableObject
{
	[field: SerializeField]
	public ControlSpriteGaypadSOData[] ControlsData { get; private set; }
}
