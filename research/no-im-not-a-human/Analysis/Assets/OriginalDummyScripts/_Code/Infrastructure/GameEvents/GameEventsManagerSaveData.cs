using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure.GameEvents
{
	[Serializable]
	public sealed class GameEventsManagerSaveData : ASavableData
	{
		[field: SerializeField]
		public List<string> CompletedEvents { get; set; }

		[field: SerializeField]
		public List<CharacterDingDongEvent> DingDongEvents { get; private set; }

		[field: SerializeField]
		public List<GrowingBellyEvent> OtherEvents { get; private set; }

		[field: SerializeField]
		public int ProphetVisitsCount { get; set; }

		[field: SerializeField]
		public int MushromeaterVisitsCount { get; set; }

		[field: SerializeField]
		public int PriestVisitCount { get; set; }

		[field: SerializeField]
		public int DayWithoutSuper { get; set; }
	}
}
