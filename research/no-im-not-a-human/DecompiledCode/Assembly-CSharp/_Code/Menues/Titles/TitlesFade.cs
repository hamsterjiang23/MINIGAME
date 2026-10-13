using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Code.Menues.Titles;

public sealed class TitlesFade : MonoBehaviour
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private TitlesFadeData[] _fadeData;

	private readonly List<int> _workedFadeData = new List<int>();

	private void Update()
	{
		for (int i = 0; i < _fadeData.Length; i++)
		{
			TitlesFadeData titlesFadeData = _fadeData[i];
			if (Time.timeSinceLevelLoad >= titlesFadeData.Time && !_workedFadeData.Contains(i))
			{
				if (titlesFadeData.IsFadeIn)
				{
					FadeIn(titlesFadeData.Duration);
				}
				else
				{
					FadeOut(titlesFadeData.Duration);
				}
				_workedFadeData.Add(i);
			}
		}
	}

	public void FadeIn(float duration)
	{
		_image.DOFade(1f, duration).SetEase(Ease.InCubic);
	}

	public void FadeOut(float duration)
	{
		_image.DOFade(0f, duration).SetEase(Ease.OutCubic);
	}
}
