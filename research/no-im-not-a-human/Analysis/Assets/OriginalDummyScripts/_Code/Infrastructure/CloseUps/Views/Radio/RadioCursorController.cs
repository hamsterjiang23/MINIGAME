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
		}

		private void OnEnable()
		{
		}

		private void OnDisable()
		{
		}

		private void UpdateCursorPosition(float value)
		{
		}
	}
}
