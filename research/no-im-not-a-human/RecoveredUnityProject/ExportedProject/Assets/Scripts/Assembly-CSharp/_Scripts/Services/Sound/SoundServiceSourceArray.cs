using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Scripts.Services.Sound.Instance;

namespace _Scripts.Services.Sound
{
	[Serializable]
	public sealed class SoundServiceSourceArray
	{
		[SerializeField]
		public List<SoundServiceSource> _soundServices = new List<SoundServiceSource>();

		public IReadOnlyList<SoundServiceSource> SoundServices => _soundServices;

		public AudioSource this[ESoundSource source] => _soundServices.FirstOrDefault((SoundServiceSource x) => x.Name == (ESoundSource)source.GetHashCode())?.Source;

		public void Add(AudioSource source)
		{
			_soundServices.Add(new SoundServiceSource((ESoundSource)source.name.GetHashCode(), source));
		}

		public bool Contains(ESoundSource source)
		{
			return _soundServices.Any((SoundServiceSource x) => x.Name == source);
		}
	}
}
