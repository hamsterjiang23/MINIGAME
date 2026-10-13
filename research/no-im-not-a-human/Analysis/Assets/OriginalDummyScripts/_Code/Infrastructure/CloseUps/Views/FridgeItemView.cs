using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using _Code.Infrastructure.Consumables;

namespace _Code.Infrastructure.CloseUps.Views
{
	public sealed class FridgeItemView : MonoBehaviour
	{
		private readonly float _hoverScaleFactor;

		private readonly float _useScaleFactor;

		[CompilerGenerated]
		private Action m_Used;

		private readonly float _maxUseTime;

		private float _useProgress;

		private bool _isHovered;

		private bool _isClicked;

		private bool _isLockPointerExit;

		public Func<(string, string, string)> GotTexts { get; private set; }

		public EConsumable ItemType { get; private set; }

		public event Action Used
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

		public void InitGotTexts(Func<(string, string, string)> gotTexts, EConsumable consumable)
		{
		}

		public void Use()
		{
		}

		public void OnPointerDown(PointerEventData eventData)
		{
		}

		public void OnPointerUp(PointerEventData eventData)
		{
		}

		public void OnHover()
		{
		}

		public void OnUnhover()
		{
		}
	}
}
