using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure._NINAH__Rooms;

namespace _Code.Infrastructure._NINAH__DayNight
{
	[Serializable]
	public sealed class DayNightSaveData : ASavableData
	{
		[SerializeField]
		private int _day;

		[field: SerializeField]
		public ETimeOfDay TimeOfDay { get; set; } = ETimeOfDay.Night;

		[field: SerializeField]
		public float LastChange { get; set; }

		[field: SerializeField]
		public int DayActions { get; set; }

		[field: SerializeField]
		public int ExtraEnergySlots { get; set; }

		[field: SerializeField]
		public Dictionary<int, int> EnergyFinesByDays { get; set; } = new Dictionary<int, int>();

		[field: SerializeField]
		public bool ImpostersKillingBlocked { get; set; }

		[field: SerializeField]
		public int LastCourierOrderedDay { get; set; } = -1;

		[field: SerializeField]
		public List<ChangePoseData> ChangePosesTomorrow { get; set; } = new List<ChangePoseData>();

		[field: SerializeField]
		public bool IsReadyToRunBodyeater { get; set; }

		[field: SerializeField]
		public bool IsNeedToRepairAfterBodyeater { get; set; }

		[field: SerializeField]
		public float AccumulatedTime { get; set; }

		[field: SerializeField]
		public List<int> CoffeeDays { get; set; } = new List<int>();

		public int Day
		{
			get
			{
				return _day;
			}
			set
			{
				_day = value;
			}
		}
	}
}
