using UnityEngine;

public class GamepadCursor : MonoBehaviour
{
	[SerializeField]
	private float _cursorSpeed;

	private Vector2 _cursorPosition;

	private Vector2 _counted;

	private float _accelerationFactor;

	private bool _isEnabled;

	private bool _isActive;

	[SerializeField]
	private float _extraSensitivity;

	public float ExtraSensitivity
	{
		get
		{
			return 0f;
		}
		set
		{
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Disable()
	{
	}

	public void Enable()
	{
	}

	public void SetActiveState(bool state)
	{
	}
}
