using System;
using Newtonsoft.Json;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure.Locations
{
	[Serializable]
	public sealed class LocationSaveData : ASavableData
	{
		[JsonProperty]
		public ELocation CurrentLocation { get; set; }
	}
}
