using UnityEngine;

public class CameraRotate : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	public float speed;

	private float pitch;

	private float yaw;

	private readonly float _mouseSensetivety;

	private Vector3 _startPos;

	private bool _isDancing;

	private float _danceSpeed;

	private void Awake()
	{
	}

	private void Update()
	{
	}

	public void StartDance()
	{
	}

	public void SetDanceSpeed(float speed)
	{
	}
}
