using System;
using System.Runtime.CompilerServices;
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

		[CompilerGenerated]
		private Action<EPhoneKey> m_Pressed;

		[CompilerGenerated]
		private Action<EPhoneKey> m_PressedUp;

		[CompilerGenerated]
		private Action<PhoneButtonView> m_Selected;

		public event Action<EPhoneKey> Pressed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EPhoneKey> PressedUp
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<PhoneButtonView> Selected
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
		}

		public void OnPointerUp(PointerEventData eventData)
		{
		}

		public void OnSelect(BaseEventData eventData)
		{
		}

		public void Disable()
		{
		}

		public void Enable()
		{
		}
	}
}
