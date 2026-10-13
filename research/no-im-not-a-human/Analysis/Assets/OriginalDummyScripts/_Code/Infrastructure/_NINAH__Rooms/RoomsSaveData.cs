using System;
using System.Collections.Generic;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Rooms;

namespace _Code.Infrastructure._NINAH__Rooms
{
	[Serializable]
	public sealed class RoomsSaveData : ASavableData
	{
		[field: SerializeField]
		public Dictionary<ERoom, Dictionary<string, int>> UIButtons { get; set; }

		[field: SerializeField]
		public int WatchTVTimesToday { get; set; }

		[field: SerializeField]
		public int BellyProgress { get; set; }

		[field: SerializeField]
		public int DayOfBloodClean { get; set; }
	}
}
