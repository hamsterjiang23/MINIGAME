using System;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.Sound;

namespace _Scripts.Services.Sound
{
	[Serializable]
	public sealed class SoundListElement
	{
		[field: SerializeField]
		[field: SearchableEnum]
		public ESound Name { get; private set; }

		[field: SerializeField]
		public AudioClip Sound { get; private set; }

		public SoundListElement(ESound name, AudioClip sound)
		{
			Name = name;
			Sound = sound;
		}
	}
}
