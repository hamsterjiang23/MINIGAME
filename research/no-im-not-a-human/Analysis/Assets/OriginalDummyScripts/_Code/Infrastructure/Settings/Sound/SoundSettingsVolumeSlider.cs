using RTLTMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace _Code.Infrastructure.Settings.Sound
{
	public sealed class SoundSettingsVolumeSlider : MonoBehaviour
	{
		[HideInInspector]
		[SerializeField]
		private LocalizeStringEvent _groupNameText;

		[HideInInspector]
		[SerializeField]
		private Slider _slider;

		[SerializeField]
		private RTLTextMeshPro _valueText;

		private UnityAction<AudioMixerGroup, float, float> _onVolumeChangedAction;

		[field: SerializeField]
		public AudioMixerGroup MixerGroup { get; private set; }

		public void Init(UnityAction<AudioMixerGroup, float, float> onVolumeChangedAction)
		{
		}

		private void OnVolumeChanged(float volume)
		{
		}

		public void SetValue(float value)
		{
		}
	}
}
