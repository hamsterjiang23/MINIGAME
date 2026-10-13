using TMPro;
using UnityEngine;
using UnityEngine.Localization;

namespace _Code.Utils
{
	public sealed class FontChanger : MonoBehaviour
	{
		[SerializeField]
		private FontChangerSOData _fontChangerSOData;

		private TextMeshProUGUI _tmp;

		public void Init()
		{
		}

		private void OnLocaleChanged(Locale locale)
		{
		}
	}
}
