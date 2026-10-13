using System.Globalization;
using RTLTMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Code.Infrastructure.Settings.Sound;

public sealed class FakeSlider : MonoBehaviour
{
	[SerializeField]
	private Slider _slider;

	[SerializeField]
	private RTLTextMeshPro _valueText;

	private UnityAction<float> _onValueChangedAction;

	public void Init(UnityAction<float> onValueChangedAction)
	{
		_onValueChangedAction = onValueChangedAction;
		_slider.onValueChanged.RemoveAllListeners();
		_slider.onValueChanged.AddListener(OnValueChanged);
	}

	public void SetValue(float value)
	{
		_slider.value = value;
	}

	private void OnValueChanged(float value)
	{
		_valueText.text = _slider.value.ToString("F2", CultureInfo.InvariantCulture);
		_onValueChangedAction?.Invoke(value);
	}
}
