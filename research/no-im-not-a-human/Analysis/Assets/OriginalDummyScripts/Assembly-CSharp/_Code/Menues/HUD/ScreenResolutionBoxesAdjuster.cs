using UnityEngine;

namespace _Code.Menues.HUD;

public sealed class ScreenResolutionBoxesAdjuster : MonoBehaviour
{
	[SerializeField]
	private RectTransform _topLetterBox;

	[SerializeField]
	private RectTransform _bottomLetterBox;

	[SerializeField]
	private RectTransform _leftPillarBox;

	[SerializeField]
	private RectTransform _rightPillarBox;

	public void AdjustBars()
	{
		float num = (float)Screen.width / (float)Screen.height;
		if (num > 1.7777778f)
		{
			float num2 = 1.7777778f / num;
			float num3 = (1f - num2) / 2f;
			_leftPillarBox.anchorMin = new Vector2(0f, 0f);
			_leftPillarBox.anchorMax = new Vector2(num3, 1f);
			_rightPillarBox.anchorMin = new Vector2(1f - num3, 0f);
			_rightPillarBox.anchorMax = new Vector2(1f, 1f);
			_topLetterBox.gameObject.SetActive(value: false);
			_bottomLetterBox.gameObject.SetActive(value: false);
		}
		else if (num < 1.7777778f)
		{
			float num4 = num / 1.7777778f;
			float num5 = (1f - num4) / 2f;
			_topLetterBox.anchorMin = new Vector2(0f, 1f - num5);
			_topLetterBox.anchorMax = new Vector2(1f, 1f);
			_bottomLetterBox.anchorMin = new Vector2(0f, 0f);
			_bottomLetterBox.anchorMax = new Vector2(1f, num5);
			_leftPillarBox.gameObject.SetActive(value: false);
			_rightPillarBox.gameObject.SetActive(value: false);
		}
	}
}
