using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace _Code.Infrastructure.Sound
{
	[CreateAssetMenu(menuName = "SoundData")]
	public sealed class MusicSOData : ScriptableObject, IMusicSOData
	{
		[field: SerializeField]
		public SerializedDictionary<int, MusicDayData> MusicByDay { get; private set; }
	}
}
