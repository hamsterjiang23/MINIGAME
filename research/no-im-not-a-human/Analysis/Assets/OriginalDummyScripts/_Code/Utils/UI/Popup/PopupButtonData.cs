using System;
using UnityEngine;

namespace _Code.Utils.UI.Popup
{
	public sealed class PopupButtonData
	{
		[field: SerializeField]
		public string Text { get; private set; }

		[field: SerializeField]
		public Action Action { get; private set; }

		public PopupButtonData(string text, Action action)
		{
		}
	}
}
