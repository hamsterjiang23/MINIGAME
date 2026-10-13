using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.Sound;
using _Code.Utils.Attributes.MinMaxRange;

namespace _Code.Infrastructure._NINAH__RandomSoundEmitters
{
	[CreateAssetMenu(menuName = "Data/RandomSoundEmitter")]
	public sealed class RandomSoundsEmitterSOData : ScriptableObject
	{
		[field: SerializeField]
		[field: MinMaxRange(0f, 50f)]
		public Vector2 DistanceRange { get; private set; }

		[field: SerializeField]
		[field: MinMaxRange(0f, 60f)]
		public Vector2 TimeRange { get; private set; }

		[field: SerializeField]
		[field: SearchableEnum]
		public ESound[] SoundsPool { get; private set; }
	}
}
