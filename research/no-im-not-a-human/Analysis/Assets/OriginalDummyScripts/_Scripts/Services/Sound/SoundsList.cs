using UnityEngine;
using _Code.Infrastructure.Sound;

namespace _Scripts.Services.Sound
{
	[CreateAssetMenu(menuName = "SoundsList")]
	public sealed class SoundsList : ScriptableObject
	{
		[SerializeReference]
		[SerializeField]
		private SoundListElement[] _sounds;

		public AudioClip this[ESound soundName] => null;
	}
}
