using AYellowpaper.SerializedCollections;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Player;

namespace _Code.Infrastructure.ControlsViewer
{
	[CreateAssetMenu(menuName = "Controls/ControlViewGaypad")]
	public sealed class ControlSpriteGaypadSOData : ScriptableObject
	{
		[field: SerializeField]
		[field: SearchableEnum]
		public EControlKeyType KeyType { get; private set; }

		[field: SerializeField]
		public SerializedDictionary<EGaypadType, Sprite> Sprites { get; private set; }
	}
}
