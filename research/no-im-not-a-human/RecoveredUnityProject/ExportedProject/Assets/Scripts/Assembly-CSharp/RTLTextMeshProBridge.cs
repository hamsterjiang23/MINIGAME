using System;
using RTLTMPro;
using UnityEngine;

public class RTLTextMeshProBridge : MonoBehaviour
{
	private RTLTextMeshPro _rtl;

	private void Awake()
	{
		_rtl = GetComponent<RTLTextMeshPro>();
	}

	public void SetRTLText(string value)
	{
		if ((object)_rtl == null)
		{
			_rtl = GetComponent<RTLTextMeshPro>();
		}
		if (_rtl == null)
		{
			throw new Exception(gameObject.name + ": RTLTextMeshPro is not set" + gameObject.transform.parent.name);
		}
		_rtl.text = value;
	}
}
