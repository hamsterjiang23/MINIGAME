using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure._NINAH__CloseUps
{
	[Serializable]
	public sealed class CloseUpSaveData : ASavableData
	{
		[field: SerializeField]
		public List<int> DaysWhenRadioHasUsed { get; set; } = new List<int>();

		[field: SerializeField]
		public List<EPhoneSubscriber> UnlockedPhoneSubscribers { get; private set; } = new List<EPhoneSubscriber>();

		[field: SerializeField]
		public Dictionary<EPhoneSubscriber, string> PhoneNumbers { get; private set; } = new Dictionary<EPhoneSubscriber, string>();

		[field: SerializeField]
		public bool DrunkBeerToday { get; set; }

		[field: SerializeField]
		public bool HasOpenedRadio { get; set; }

		[field: SerializeField]
		public int ExtraBeerUsed { get; set; }

		[field: SerializeField]
		public int FemaCalls { get; set; }

		[field: SerializeField]
		public Dictionary<EPhoneSubscriber, int> CallsCount { get; set; } = new Dictionary<EPhoneSubscriber, int>();

		[field: SerializeField]
		public List<EPhoneSubscriber> TodayCalls { get; private set; } = new List<EPhoneSubscriber>();

		public void DistinctNumbers()
		{
			UnlockedPhoneSubscribers = UnlockedPhoneSubscribers.Distinct().ToList();
		}
	}
}
