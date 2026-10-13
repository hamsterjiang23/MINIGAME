using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public sealed class TMP_ForceArabicAutoFix : MonoBehaviour
{
	private TMP_Text _tmp;

	private void Awake()
	{
		_tmp = GetComponent<TMP_Text>();
		_tmp.textPreprocessor = new ForceArabicTMPPreprocessor();
	}
}
