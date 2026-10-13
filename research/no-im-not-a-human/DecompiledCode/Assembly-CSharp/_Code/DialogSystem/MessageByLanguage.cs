using System;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Characters.DialogSystem;

namespace _Code.DialogSystem;

[Serializable]
public class MessageByLanguage
{
	[field: SerializeField]
	public EInfoMessageType Type { get; private set; }

	[field: SerializeField]
	public LocalizedString MessageLocalizationKey { get; private set; }
}
