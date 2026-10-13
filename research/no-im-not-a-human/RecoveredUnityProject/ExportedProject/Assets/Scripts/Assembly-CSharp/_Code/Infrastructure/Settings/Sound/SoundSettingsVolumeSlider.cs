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
			_onVolumeChangedAction = onVolumeChangedAction;
			_slider.minValue = 0.001f;
			_slider.maxValue = 1f;
			_slider.onValueChanged.RemoveAllListeners();
			_slider.onValueChanged.AddListener(OnVolumeChanged);
		}

		private void OnVolumeChanged(float volume)
		{
			_onVolumeChangedAction?.Invoke(MixerGroup, Mathf.Log(volume) * 20f, volume);
			_valueText.text = Mathf.RoundToInt(volume / _slider.maxValue * 100f).ToString();
		}

		public void SetValue(float value)
		{
			_slider.value = value;
		}
	}
}
