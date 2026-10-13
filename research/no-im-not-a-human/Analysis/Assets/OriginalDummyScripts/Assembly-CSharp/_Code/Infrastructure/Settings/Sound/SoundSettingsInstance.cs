using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

namespace _Code.Infrastructure.Settings.Sound;

public sealed class SoundSettingsInstance : ASettingsInstance
{
	[SerializeField]
	private SoundSettingsVolumeSlider[] _volumeSliders;

	[SerializeField]
	private AudioMixer _audioMixer;

	private readonly SoundSettings _soundSettings = new SoundSettings();

	public override ISetting Setting => _soundSettings;

	protected override void Init()
	{
		_soundSettings.Init(_audioMixer);
		SoundSettingsVolumeSlider[] volumeSliders = _volumeSliders;
		for (int i = 0; i < volumeSliders.Length; i++)
		{
			volumeSliders[i].Init(_soundSettings.SetVolume);
		}
	}

	protected override void UpdateVisualsForLoadedData()
	{
		Dictionary<AudioMixerGroup, float> dictionary = new Dictionary<AudioMixerGroup, float>();
		foreach (KeyValuePair<string, float> item in ((SoundSettingsData)_soundSettings.SettingsData).VolumesByChanel)
		{
			AudioMixerGroup audioMixerGroup = _audioMixer.FindMatchingGroups(item.Key).FirstOrDefault();
			if (audioMixerGroup != null && !dictionary.TryAdd(audioMixerGroup, item.Value))
			{
				dictionary[audioMixerGroup] = item.Value;
			}
		}
		foreach (KeyValuePair<AudioMixerGroup, float> group in dictionary)
		{
			_volumeSliders.FirstOrDefault((SoundSettingsVolumeSlider x) => x.MixerGroup == group.Key)?.SetValue(group.Value);
		}
	}

	public void OnStarted()
	{
		_soundSettings.OnStarted();
		SoundSettingsData soundSettingsData = _soundSettings.SettingsData as SoundSettingsData;
		if (!soundSettingsData.IsNotFirstLaunch)
		{
			soundSettingsData.IsNotFirstLaunch = true;
			_volumeSliders.FirstOrDefault((SoundSettingsVolumeSlider x) => x.MixerGroup.name == "Music").SetValue(0.5f);
		}
	}
}
