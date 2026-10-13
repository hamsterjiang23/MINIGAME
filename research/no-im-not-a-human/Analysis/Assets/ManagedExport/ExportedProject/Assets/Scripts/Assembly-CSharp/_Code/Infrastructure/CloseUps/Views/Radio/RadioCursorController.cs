using UnityEngine;

namespace _Code.Infrastructure.CloseUps.Views.Radio
{
	public class RadioCursorController : MonoBehaviour
	{
		[SerializeField]
		private RadioKnobController knob;

		[SerializeField]
		private RectTransform scale;

		private RectTransform cursor;

		private void Awake()
		{
			cursor = GetComponent<RectTransform>();
		}

		private void OnEnable()
		{
			knob.ValueChanged += UpdateCursorPosition;
		}

		private void OnDisable()
		{
			knob.ValueChanged -= UpdateCursorPosition;
		}

		private void UpdateCursorPosition(float value)
		{
			Vector3[] array = new Vector3[4];
			scale.GetLocalCorners(array);
			float a = array[0].x + cursor.rect.width * 0.5f;
			float b = array[2].x - cursor.rect.width * 0.5f;
			float x = Mathf.Lerp(a, b, value);
			Vector3 localPosition = cursor.localPosition;
			localPosition.x = x;
			cursor.localPosition = localPosition;
		}
	}
}
