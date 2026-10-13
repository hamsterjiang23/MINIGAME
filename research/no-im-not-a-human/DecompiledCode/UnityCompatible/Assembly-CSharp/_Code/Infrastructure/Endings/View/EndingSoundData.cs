using System;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Infrastructure.Sound;

namespace _Code.Infrastructure.Endings.View
{
	[Serializable]
	public sealed class EndingSoundData
	{
		[SerializeField]
		private bool _useSound;

		[SerializeField]
		private bool _isLooped;

		[SerializeField]
		[SearchableEnum]
		private ESound _sound;

		public bool UseSound => _useSound;

		public bool IsLooped => _isLooped;

		public ESound Sound => _sound;
	}
}
