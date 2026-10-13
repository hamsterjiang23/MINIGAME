using System;
using UnityEngine;

namespace _Scripts.Services.Sound.Instance
{
	[Serializable]
	public sealed class SoundServiceSource
	{
		[field: SerializeField]
		public ESoundSource Name { get; private set; }

		[field: SerializeField]
		public AudioSource Source { get; private set; }

		public SoundServiceSource(ESoundSource name, AudioSource source)
		{
			Name = name;
			Source = source;
		}
	}
}
