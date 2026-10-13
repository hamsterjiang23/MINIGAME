using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Player;

namespace _Code.Infrastructure.CloseUps.Views.Radio
{
	public class RadioKnobController : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private Image _image;

		[SerializeField]
		private float sensitivity;

		[SerializeField]
		private float minAngle;

		[SerializeField]
		private float maxAngle;

		private float _currentAngle;

		private Vector2 _lastMousePosition;

		private float _previousValue;

		private bool _isActive;

		private bool _isRotating;

		private float _previousAngle;

		private InputHandling _inputHandler;

		private Material _outlineMaterial;

		private bool _isClicked;

		[CompilerGenerated]
		private Action<float> m_ValueChanged;

		[CompilerGenerated]
		private Action<bool> m_PointerPressedStateChanged;

		public float Value { get; private set; }

		public event Action<float> ValueChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<bool> PointerPressedStateChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
		}

		public void OnPointerUp(PointerEventData eventData)
		{
		}

		public void OnDrag(PointerEventData eventData)
		{
		}

		private void Awake()
		{
		}

		private void Update()
		{
		}

		private void RotateKnob(float delta)
		{
		}

		private Vector2 RotateVector(Vector2 vector, float angleDegrees)
		{
			return default;
		}

		public void SetActiveMode(bool b)
		{
		}

		public void InitModules(InputHandling inputHandler)
		{
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		public void OnPointerExit(PointerEventData eventData)
		{
		}
	}
}
