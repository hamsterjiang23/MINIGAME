using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Menues.Titles
{
	public sealed class TitlesFade : MonoBehaviour
	{
		[SerializeField]
		private Image _image;

		[SerializeField]
		private TitlesFadeData[] _fadeData;

		private readonly List<int> _workedFadeData;

		private void Update()
		{
		}

		public void FadeIn(float duration)
		{
		}

		public void FadeOut(float duration)
		{
		}
	}
}
