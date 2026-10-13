using UnityEngine;

public class FixImposter : MonoBehaviour
{
	private Vector3 _startPos;

	private void Awake()
	{
		_startPos = transform.position;
		gameObject.SetActive(value: false);
	}

	private void Update()
	{
		transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
		transform.position = new Vector3(transform.position.x, _startPos.y, transform.position.z);
	}
}
