using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using _Code.Infrastructure.Consumables;

namespace _Code.Infrastructure.CloseUps.Views
{
	public sealed class FridgeItemView : MonoBehaviour
	{
		private readonly float _hoverScaleFactor = 1.075f;

		private readonly float _useScaleFactor = 0.125f;

		private readonly float _maxUseTime = 0.5f;

		private float _useProgress;

		private bool _isHovered;

		private bool _isClicked;

		private bool _isLockPointerExit;

		public Func<(string, string, string)> GotTexts { get; private set; }

		public EConsumable ItemType { get; private set; }

		public event Action Used;

		public void InitGotTexts(Func<(string, string, string)> gotTexts, EConsumable consumable)
		{
			GotTexts = gotTexts;
			ItemType = consumable;
		}

		public void Use()
		{
			Used?.Invoke();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_isClicked = true;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			_isClicked = false;
			_useProgress = 0f;
			transform.localScale = (_isHovered ? (Vector3.one * _hoverScaleFactor) : Vector3.one);
		}

		public void OnHover()
		{
			transform.DOScale(_hoverScaleFactor, 0.2f).SetEase(Ease.InCubic);
		}

		public void OnUnhover()
		{
			if ((bool)transform)
			{
				transform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutCubic);
			}
		}
	}
}
