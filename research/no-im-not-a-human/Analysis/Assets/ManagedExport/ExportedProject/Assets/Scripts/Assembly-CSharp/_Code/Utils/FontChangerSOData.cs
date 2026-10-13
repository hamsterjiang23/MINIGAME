using AYellowpaper.SerializedCollections;
using TMPro;
using UnityEngine;

namespace _Code.Utils
{
	[CreateAssetMenu(menuName = "FontLocaleData")]
	public sealed class FontChangerSOData : ScriptableObject
	{
		[field: SerializeField]
		public SerializedDictionary<string, TMP_FontAsset> Fonts { get; private set; } = new SerializedDictionary<string, TMP_FontAsset>();
	}
}
