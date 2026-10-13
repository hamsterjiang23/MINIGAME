using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using _Scripts.Services.DataModel.Models;

namespace _Code.Infrastructure.DataModel.Models.GameSave
{
	[Serializable]
	public sealed class GameSaveData : BaseDataStorage
	{
		[JsonProperty]
		private Dictionary<string, string> _jsonValues;

		[JsonProperty]
		private Dictionary<string, string> _jsonValuesReserve;

		private Dictionary<Type, ASavableData> _values;

		public Dictionary<Type, ASavableData> Values => null;

		public bool NeedToLoad { get; set; }

		public bool HasSaveData => false;

		public void CreateJson()
		{
		}

		public void CreateReserveJson()
		{
		}

		public T LoadFromJson<T>(ASavableClass<T> savableClass) where T : ASavableData
		{
			return null;
		}

		public void ClearReserveData()
		{
		}

		public void ResetValues()
		{
		}
	}
}
