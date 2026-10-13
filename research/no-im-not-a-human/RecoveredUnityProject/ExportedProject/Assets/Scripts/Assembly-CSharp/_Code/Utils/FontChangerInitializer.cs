using UnityEngine;

namespace _Code.Utils
{
	public sealed class FontChangerInitializer : MonoBehaviour
	{
		private FontChanger[] _fontChangers;

		public void Start()
		{
			_fontChangers = Object.FindObjectsByType<FontChanger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			FontChanger[] fontChangers = _fontChangers;
			for (int i = 0; i < fontChangers.Length; i++)
			{
				fontChangers[i].Init();
			}
		}
	}
}
