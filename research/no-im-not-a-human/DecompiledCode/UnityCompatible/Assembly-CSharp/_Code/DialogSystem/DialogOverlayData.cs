using System;
using UnityEngine;

namespace _Code.DialogSystem
{
	[Serializable]
	public sealed class DialogOverlayData
	{
		[field: SerializeField]
		public EDialogOverlayType OverlayType { get; private set; }

		[field: SerializeField]
		public Sprite Sprite { get; private set; }

		[field: SerializeField]
		public Color Color { get; private set; }

		[field: SerializeField]
		public bool ShowEvenIfDialogEnded { get; private set; }

		[field: SerializeField]
		public bool ShowBehindTextsContent { get; private set; }
	}
}
