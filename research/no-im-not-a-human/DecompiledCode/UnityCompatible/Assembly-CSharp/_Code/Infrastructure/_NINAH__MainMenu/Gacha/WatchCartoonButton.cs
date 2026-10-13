using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Code.Infrastructure._NINAH__MainMenu.Gacha
{
	public sealed class WatchCartoonButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
	{
		[SerializeField]
		private Image _playButtonOverlay;

		private bool _isUnlocked;

		public event Action Clicked;

		public void SetUnlockedState(bool isUnlocked)
		{
			_isUnlocked = isUnlocked;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (_isUnlocked)
			{
				_playButtonOverlay.DOFade(1f, 0.2f);
			}
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (_isUnlocked)
			{
				_playButtonOverlay.DOFade(0f, 0.2f);
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (_isUnlocked)
			{
				Clicked?.Invoke();
			}
		}
	}
}
