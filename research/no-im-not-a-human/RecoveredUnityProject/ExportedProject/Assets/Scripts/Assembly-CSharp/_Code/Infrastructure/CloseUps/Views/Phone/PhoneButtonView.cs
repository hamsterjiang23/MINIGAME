using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Code.Infrastructure.CloseUps.Views.Phone
{
	public sealed class PhoneButtonView : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, ISelectHandler
	{
		[SerializeField]
		private Image _image;

		[SerializeField]
		private Sprite _usualSprite;

		[SerializeField]
		private Sprite _clickedSprite;

		[SerializeField]
		private EPhoneKey _phoneKey;

		[SerializeField]
		private Selectable _selectable;

		public event Action<EPhoneKey> Pressed;

		public event Action<EPhoneKey> PressedUp;

		public event Action<PhoneButtonView> Selected;

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_selectable.interactable)
			{
				_image.sprite = _clickedSprite;
				Pressed?.Invoke(_phoneKey);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (_selectable.interactable)
			{
				_image.sprite = _usualSprite;
				PressedUp?.Invoke(_phoneKey);
			}
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (_selectable.interactable)
			{
				Selected?.Invoke(this);
			}
		}

		public void Disable()
		{
			_selectable.interactable = false;
		}

		public void Enable()
		{
			_selectable.interactable = true;
		}
	}
}
