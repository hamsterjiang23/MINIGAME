using System;
using System.Collections.Generic;
using UnityEngine;
using _Scripts.Services.Sound.Instance;

namespace _Scripts.Services.Sound
{
	[Serializable]
	public sealed class SoundServiceSourceArray
	{
		[SerializeField]
		public List<SoundServiceSource> _soundServices;

		public IReadOnlyList<SoundServiceSource> SoundServices => null;

		public AudioSource this[ESoundSource source] => null;

		public void Add(AudioSource source)
		{
		}

		public bool Contains(ESoundSource source)
		{
			return false;
		}
	}
}
