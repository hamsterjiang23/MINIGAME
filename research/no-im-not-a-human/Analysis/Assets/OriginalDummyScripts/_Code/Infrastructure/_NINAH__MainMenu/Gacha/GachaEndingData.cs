using System;
using UnityEngine;
using UnityEngine.Localization;

namespace _Code.Infrastructure._NINAH__MainMenu.Gacha
{
	[Serializable]
	public sealed class GachaEndingData
	{
		[SerializeField]
		private LocalizedString _description;

		[field: SerializeField]
		public Sprite[] SpoilerImages { get; private set; }

		public string Description => null;
	}
}
