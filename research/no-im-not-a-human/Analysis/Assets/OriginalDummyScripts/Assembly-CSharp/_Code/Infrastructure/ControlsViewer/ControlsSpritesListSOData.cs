using UnityEngine;

namespace _Code.Infrastructure.ControlsViewer;

[CreateAssetMenu(menuName = "Controls/ControlViewList")]
public sealed class ControlsSpritesListSOData : ScriptableObject
{
	[field: SerializeField]
	public ControlSpriteSOData[] ControlsData { get; private set; }
}
