using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Utils.UI.Popup
{
	public sealed class PopupButtonView : MonoBehaviour
	{
		[SerializeField]
		private Button _button;

		[SerializeField]
		private RTLTextMeshPro _text;

		public void Setup(PopupButtonData button)
		{
		}

		public void InvokeCommand()
		{
		}
	}
}
