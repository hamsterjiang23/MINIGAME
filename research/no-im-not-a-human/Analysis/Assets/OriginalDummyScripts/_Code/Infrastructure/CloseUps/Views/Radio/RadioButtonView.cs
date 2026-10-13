using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Code.Infrastructure.CloseUps.Views.Radio
{
	public sealed class RadioButtonView : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		private Image _image;

		[SerializeField]
		private Sprite _usualSprite;

		[SerializeField]
		private Sprite _pressedSprite;

		[SerializeField]
		private ERadioState _radioState;

		[SerializeField]
		private Material _outlineMaterialSource;

		[SerializeField]
		private Material _hintMaterialSource;

		[CompilerGenerated]
		private Action<ERadioState> m_Pressed;

		private bool _isPressed;

		private Material _outlineMaterial;

		private Material _hintMaterial;

		private bool _isOutlineEnabled;

		public event Action<ERadioState> Pressed
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

		private void Awake()
		{
		}

		public void SetState(bool isPressed)
		{
		}

		private void UpdateSprite()
		{
		}

		public void OnPointerDown(PointerEventData eventData)
		{
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
		}

		public void OnPointerExit(PointerEventData eventData)
		{
		}

		public void SetOutlineEnabled(bool isEnabled)
		{
		}
	}
}
