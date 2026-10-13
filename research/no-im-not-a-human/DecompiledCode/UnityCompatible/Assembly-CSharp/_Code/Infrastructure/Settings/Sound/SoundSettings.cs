using Cysharp.Threading.Tasks;
using UnityEngine.Audio;

namespace _Code.Infrastructure.Settings.Sound
{
	public sealed class SoundSettings : ISetting
	{
		private SoundSettingsData _settings = new SoundSettingsData();

		private AudioMixer _audioMixer;

		private bool _isSceneStarted;

		private const string VOLUME_SETTING = "Volume";

		public ASettingsData SettingsData
		{
			get
			{
				return _settings;
			}
			set
			{
				_settings = (SoundSettingsData)value;
			}
		}

		public void Init(AudioMixer audioMixer)
		{
			_audioMixer = audioMixer;
		}

		public void SetVolume(AudioMixerGroup group, float volume, float volumeSliderValue)
		{
			InitVolumeAsync(group, volume, volumeSliderValue).Forget();
		}

		private async UniTaskVoid InitVolumeAsync(AudioMixerGroup group, float volume, float volumeSliderValue)
		{
			await UniTask.WaitUntil(() => _isSceneStarted);
			if (!(group == null))
			{
				_audioMixer.SetFloat(group.name + "Volume", volume);
				_settings.VolumesByChanel[group.name] = volumeSliderValue;
			}
		}

		public void OnStarted()
		{
			_isSceneStarted = true;
		}
	}
}
