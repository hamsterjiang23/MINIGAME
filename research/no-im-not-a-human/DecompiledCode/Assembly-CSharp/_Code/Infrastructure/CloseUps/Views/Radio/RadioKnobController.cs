using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Player;

namespace _Code.Infrastructure.CloseUps.Views.Radio;

public class RadioKnobController : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private float sensitivity = 0.5f;

	[SerializeField]
	private float minAngle = -135f;

	[SerializeField]
	private float maxAngle = 135f;

	private float _currentAngle;

	private Vector2 _lastMousePosition;

	private float _previousValue = -1f;

	private bool _isActive;

	private bool _isRotating;

	private float _previousAngle;

	private InputHandling _inputHandler;

	private Material _outlineMaterial;

	private bool _isClicked;

	public float Value { get; private set; }

	public event Action<float> ValueChanged;

	public event Action<bool> PointerPressedStateChanged;

	public void OnPointerDown(PointerEventData eventData)
	{
		_lastMousePosition = eventData.position;
		_isClicked = true;
		PointerPressedStateChanged?.Invoke(obj: true);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		_isClicked = false;
		if (!RectTransformUtility.RectangleContainsScreenPoint(_image.rectTransform, eventData.position, eventData.enterEventCamera))
		{
			OnPointerExit(eventData);
		}
		PointerPressedStateChanged?.Invoke(obj: false);
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (_isActive)
		{
			Vector2 vector = eventData.position - _lastMousePosition;
			_lastMousePosition = eventData.position;
			RotateKnob(0f - RotateVector(vector, 45f).y);
		}
	}

	private void Awake()
	{
		if (!(_image == null))
		{
			_outlineMaterial = UnityEngine.Object.Instantiate(_image.material);
			_image.material = _outlineMaterial;
			_outlineMaterial.SetFloat("_OutlineThickness", 0.002f);
		}
	}

	private void Update()
	{
		if (_inputHandler == null)
		{
			return;
		}
		Vector2 uIRadioKnob = _inputHandler.UIRadioKnob;
		if (uIRadioKnob.magnitude < 0.4f)
		{
			_isRotating = false;
			return;
		}
		float num = Mathf.Atan2(uIRadioKnob.y, uIRadioKnob.x) * 57.29578f;
		if (num < 0f)
		{
			num += 360f;
		}
		if (!_isRotating)
		{
			_previousAngle = num;
			_isRotating = true;
		}
		else
		{
			float delta = Mathf.DeltaAngle(_previousAngle, num);
			_previousAngle = num;
			RotateKnob(delta);
		}
	}

	private void RotateKnob(float delta)
	{
		_currentAngle += delta * sensitivity;
		_currentAngle = Mathf.Clamp(_currentAngle, minAngle, maxAngle);
		transform.localEulerAngles = new Vector3(0f, 0f, _currentAngle);
		Value = Mathf.InverseLerp(maxAngle, minAngle, _currentAngle);
		if (Mathf.Abs(Value - _previousValue) > Mathf.Epsilon)
		{
			_previousValue = Value;
			ValueChanged?.Invoke(Value);
		}
	}

	private Vector2 RotateVector(Vector2 vector, float angleDegrees)
	{
		float f = angleDegrees * ((float)Math.PI / 180f);
		float num = Mathf.Cos(f);
		float num2 = Mathf.Sin(f);
		return new Vector2(vector.x * num - vector.y * num2, vector.x * num2 + vector.y * num);
	}

	public void SetActiveMode(bool b)
	{
		_isActive = b;
	}

	public void InitModules(InputHandling inputHandler)
	{
		_inputHandler = inputHandler;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!(_image == null))
		{
			_outlineMaterial.SetFloat("_IsActive", 1f);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!(_image == null) && !_isClicked)
		{
			_outlineMaterial.SetFloat("_IsActive", 0f);
		}
	}
}
