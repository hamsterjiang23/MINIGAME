using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Code.Utils.UI
{
	public sealed class UISelectable : MonoBehaviour, ISelectHandler, IEventSystemHandler, IDeselectHandler
	{
		[CompilerGenerated]
		private Action<BaseEventData> m_Selected;

		[CompilerGenerated]
		private Action<BaseEventData> m_Deselected;

		public event Action<BaseEventData> Selected
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

		public event Action<BaseEventData> Deselected
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

		public void OnSelect(BaseEventData eventData)
		{
		}

		public void OnDeselect(BaseEventData eventData)
		{
		}
	}
}
