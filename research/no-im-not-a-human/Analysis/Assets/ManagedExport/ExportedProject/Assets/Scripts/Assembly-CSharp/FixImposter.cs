using UnityEngine;

public class FixImposter : MonoBehaviour
{
	private Vector3 _startPos;

	private void Awake()
	{
		_startPos = base.transform.position;
		base.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		base.transform.eulerAngles = new Vector3(0f, base.transform.eulerAngles.y, 0f);
		base.transform.position = new Vector3(base.transform.position.x, _startPos.y, base.transform.position.z);
	}
}
