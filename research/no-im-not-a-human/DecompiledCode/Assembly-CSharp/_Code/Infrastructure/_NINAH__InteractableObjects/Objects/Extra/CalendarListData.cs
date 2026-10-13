using System;
using UnityEngine;
using UnityEngine.Localization;

namespace _Code.Infrastructure._NINAH__InteractableObjects.Objects.Extra;

[Serializable]
public sealed class CalendarListData
{
	[field: SerializeField]
	public LocalizedString HolidayName { get; private set; }

	[field: SerializeField]
	public Sprite HolidayPaperSheetMaterial { get; private set; }
}
