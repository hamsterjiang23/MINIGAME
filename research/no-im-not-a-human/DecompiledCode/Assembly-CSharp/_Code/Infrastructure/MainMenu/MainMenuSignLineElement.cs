using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Infrastructure.MainMenu;

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
		_index = index;
	}

	public void SetSprite(Sprite sprite)
	{
		_image.sprite = sprite;
	}

	public Sprite GetSprite()
	{
		return _image.sprite;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		transform.DOScale(0.9f, 0.2f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		transform.DOScale(0.8f, 0.2f);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
	}
}
