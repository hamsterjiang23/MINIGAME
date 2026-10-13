using RTLTMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Code.Infrastructure.Settings.Sound
{
	public sealed class FakeSlider : MonoBehaviour
	{
		[SerializeField]
		private Slider _slider;

		[SerializeField]
		private RTLTextMeshPro _valueText;

		private UnityAction<float> _onValueChangedAction;

		public void Init(UnityAction<float> onValueChangedAction)
		{
		}

		public void SetValue(float value)
		{
		}

		private void OnValueChanged(float value)
		{
		}
	}
}
