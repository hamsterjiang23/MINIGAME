using UnityEngine;
using _Code.Infrastructure.CloseUps.Views;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.Sound.Instance;

namespace _Code.Infrastructure._NINAH__CloseUps.Views.Fridge
{
	public sealed class Cockroach : MonoBehaviour
	{
		[SerializeField]
		private Sprite _killedSprite;

		[SerializeField]
		private AnimatedImage _image;

		[SerializeField]
		private SoundServiceInstance _soundService;

		[SerializeField]
		private FridgeItemView _fridgeItemView;

		private bool _isKilled;

		public void Kill()
		{
		}
	}
}
