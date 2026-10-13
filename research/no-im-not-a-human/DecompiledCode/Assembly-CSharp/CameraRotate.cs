using UnityEngine;

public class CameraRotate : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	public float speed = 50f;

	private float pitch;

	private float yaw;

	private readonly float _mouseSensetivety = 5f;

	private Vector3 _startPos;

	private bool _isDancing;

	private float _danceSpeed = 2f;

	private void Awake()
	{
		_startPos = transform.position;
	}

	private void Update()
	{
		yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * _mouseSensetivety;
		pitch -= Input.GetAxis("Mouse Y") * _mouseSensetivety;
		pitch = Mathf.Clamp(pitch, -30f, 50f);
		transform.localEulerAngles = new Vector3(pitch, yaw, 0f);
		if (_isDancing)
		{
			transform.localEulerAngles += new Vector3(0f, Mathf.Cos(Time.time * _danceSpeed) * 0.1f, Mathf.Sin(Time.time * _danceSpeed));
			transform.position = _startPos + new Vector3(Mathf.Sin(Time.time * _danceSpeed) * 0.1f, 0f, Mathf.Cos(Time.time * _danceSpeed) * 0.1f);
		}
	}

	public void StartDance()
	{
		_isDancing = true;
	}

	public void SetDanceSpeed(float speed)
	{
		_danceSpeed = speed;
	}
}
