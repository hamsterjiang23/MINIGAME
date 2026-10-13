using UnityEngine;

namespace _Code.Menues.HUD
{
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
		}
	}
}
