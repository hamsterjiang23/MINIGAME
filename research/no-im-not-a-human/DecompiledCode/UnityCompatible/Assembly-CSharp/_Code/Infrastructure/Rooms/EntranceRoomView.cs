using UnityEngine;
using UnityEngine.UI;

namespace _Code.Infrastructure.Rooms
{
	public sealed class EntranceRoomView : ARoomView
	{
		[SerializeField]
		private Image _background;

		[SerializeField]
		private Sprite _burningBackground;

		[SerializeField]
		private Sprite _destroyedBackground;

		public void DestroyedBackground()
		{
			_background.sprite = _destroyedBackground;
		}

		public void BurnBackground()
		{
			_background.sprite = _burningBackground;
		}
	}
}
