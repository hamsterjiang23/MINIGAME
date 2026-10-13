using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadCursor : MonoBehaviour
{
	[SerializeField]
	private float _cursorSpeed = 1000f;

	private Vector2 _cursorPosition;

	private Vector2 _counted;

	private float _accelerationFactor;

	private bool _isEnabled;

	private bool _isActive;

	[SerializeField]
	private float _extraSensitivity = 1f;

	public float ExtraSensitivity
	{
		get
		{
			return _extraSensitivity;
		}
		set
		{
			_extraSensitivity = value;
			_cursorSpeed = (float)Screen.width / 10f * _extraSensitivity;
		}
	}

	private void Start()
	{
		_cursorPosition = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
		_cursorSpeed = (float)Screen.width / 10f * ExtraSensitivity;
	}

	private void Update()
	{
		if (_isEnabled && _isActive)
		{
			Vector2 vector = Gamepad.current?.leftStick.ReadValue() ?? Vector2.zero;
			Vector2 vector2 = Mouse.current.delta.ReadValue();
			if (vector != Vector2.zero)
			{
				_accelerationFactor += 0.66f * Time.fixedUnscaledDeltaTime;
				_accelerationFactor = Mathf.Clamp(_accelerationFactor, 1f, 3f);
			}
			else
			{
				_accelerationFactor = 1f;
			}
			_counted = vector * (_cursorSpeed * Time.fixedUnscaledDeltaTime * _accelerationFactor);
			_cursorPosition += _counted;
			_cursorPosition.x = Mathf.Clamp(_cursorPosition.x, 0f, Screen.width);
			_cursorPosition.y = Mathf.Clamp(_cursorPosition.y, 0f, Screen.height);
			if (vector != Vector2.zero && vector2 == Vector2.zero)
			{
				Mouse.current?.WarpCursorPosition(_cursorPosition);
			}
			if (vector2 != Vector2.zero)
			{
				_cursorPosition = Mouse.current.position.ReadValue();
			}
		}
	}

	public void Disable()
	{
		_isEnabled = false;
	}

	public void Enable()
	{
		_isEnabled = true;
		_cursorPosition = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
	}

	public void SetActiveState(bool state)
	{
		_isActive = state;
		_cursorPosition = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
	}
}
