using System;
using UnityEngine;

namespace _Code.Infrastructure.Settings.Language
{
	[Serializable]
	public sealed class TextSettingsData : ASettingsData
	{
		[field: SerializeField]
		public string SelectedLanguage { get; set; }

		[field: SerializeField]
		public bool IsUseTypewriter { get; set; }
	}
}
