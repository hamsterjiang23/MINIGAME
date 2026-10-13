using UnityEngine;
using UnityEngine.Audio;

namespace _Code.Infrastructure.Settings.Sound
{
	public sealed class SoundSettingsInstance : ASettingsInstance
	{
		[SerializeField]
		private SoundSettingsVolumeSlider[] _volumeSliders;

		[SerializeField]
		private AudioMixer _audioMixer;

		private readonly SoundSettings _soundSettings;

		public override ISetting Setting => null;

		protected override void Init()
		{
		}

		protected override void UpdateVisualsForLoadedData()
		{
		}

		public void OnStarted()
		{
		}
	}
}
