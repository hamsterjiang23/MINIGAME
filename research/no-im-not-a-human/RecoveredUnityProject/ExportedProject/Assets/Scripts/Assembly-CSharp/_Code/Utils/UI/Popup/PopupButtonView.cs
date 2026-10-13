using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using _Code.Utils.Extensions;

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
			_text.text = button.Text.GetLocalization("UI");
			_button.onClick.RemoveAllListeners();
			_button.onClick.AddListener(() =>
			{
				button.Action?.Invoke();
			});
		}

		public void InvokeCommand()
		{
			_button.onClick.Invoke();
		}
	}
}
