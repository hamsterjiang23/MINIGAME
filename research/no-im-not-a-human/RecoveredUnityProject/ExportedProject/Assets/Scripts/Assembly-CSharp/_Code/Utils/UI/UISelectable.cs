using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Code.Utils.UI
{
	public sealed class UISelectable : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		public event Action<BaseEventData> Selected;

		public event Action<BaseEventData> Deselected;

		public void OnSelect(BaseEventData eventData)
		{
			Selected?.Invoke(eventData);
		}

		public void OnDeselect(BaseEventData eventData)
		{
			Deselected?.Invoke(eventData);
		}
	}
}
