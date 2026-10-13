using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuSignLineElement : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		[SerializeField]
		private Image _image;

		[SerializeField]
		private AnimatedImage _animatedImage;

		[SerializeField]
		private AnimationData _hoverAnimation;

		[SerializeField]
		private AnimationData _leaveAnimation;

		private int _index;

		public void Init(int index)
		{
		}

		public void SetSprite(Sprite sprite)
		{
		}

		public Sprite GetSprite()
		{
			return null;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		public void OnPointerExit(PointerEventData eventData)
		{
		}

		public void OnPointerClick(PointerEventData eventData)
		{
		}
	}
}
