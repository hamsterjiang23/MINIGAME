using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure._NINAH__CloseUps
{
	[Serializable]
	public sealed class CloseUpSaveData : ASavableData
	{
		[field: SerializeField]
		public List<int> DaysWhenRadioHasUsed { get; set; }

		[field: SerializeField]
		public List<EPhoneSubscriber> UnlockedPhoneSubscribers { get; private set; }

		[field: SerializeField]
		public Dictionary<EPhoneSubscriber, string> PhoneNumbers { get; private set; }

		[field: SerializeField]
		public bool DrunkBeerToday { get; set; }

		[field: SerializeField]
		public bool HasOpenedRadio { get; set; }

		[field: SerializeField]
		public int ExtraBeerUsed { get; set; }

		[field: SerializeField]
		public int FemaCalls { get; set; }

		[field: SerializeField]
		public Dictionary<EPhoneSubscriber, int> CallsCount { get; set; }

		[field: SerializeField]
		public List<EPhoneSubscriber> TodayCalls { get; private set; }

		public void DistinctNumbers()
		{
		}
	}
}
