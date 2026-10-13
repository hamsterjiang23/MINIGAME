using RTLTMPro;
using UnityEngine;

namespace _Code.Utils.UI;

public sealed class VersionText : MonoBehaviour
{
	[SerializeField]
	private RTLTextMeshPro _text;

	private void Awake()
	{
		_text.text = "ver. " + Application.version;
	}
}
