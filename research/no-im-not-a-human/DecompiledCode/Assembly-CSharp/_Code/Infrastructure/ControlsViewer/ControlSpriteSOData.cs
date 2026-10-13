using RoboRyanTron.SearchableEnum;
using UnityEngine;

namespace _Code.Infrastructure.ControlsViewer;

[CreateAssetMenu(menuName = "Controls/ControlView")]
public sealed class ControlSpriteSOData : ScriptableObject
{
	[field: SerializeField]
	[field: SearchableEnum]
	public EControlKeyType KeyType { get; private set; }

	[field: SerializeField]
	public Sprite Sprite { get; private set; }
}
