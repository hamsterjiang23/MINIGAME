using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Menues;

public sealed class TestCheck : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _text;

	[SerializeField]
	private Toggle _toggle;

	public void SetData(Action onOn, Action onOff)
	{
	}
}
