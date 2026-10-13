using System.Collections.Generic;
using _Scripts.Services.DataModel.Models.PlayerData;

namespace _Scripts.Services.DataModel.Models.CustomData
{
	public sealed class SimplePrefsData : BaseDataStorage
	{
		public readonly List<SimpleDataType> dataTypes;
	}
}
