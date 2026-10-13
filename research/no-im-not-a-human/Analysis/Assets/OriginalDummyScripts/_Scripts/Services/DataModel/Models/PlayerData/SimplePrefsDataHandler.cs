using _Scripts.Services.DataModel.DataStorages;
using _Scripts.Services.DataModel.Models.CustomData;

namespace _Scripts.Services.DataModel.Models.PlayerData
{
	public sealed class SimplePrefsDataHandler : ABaseDataHandler<SimplePrefsData>, ISimplePrefsDataHandler
	{
		protected override bool UseSteamCloud => false;

		public SimplePrefsDataHandler(IDataStorage dataStorage)
			: base((IDataStorage)null)
		{
		}

		public void SavePref(ESimpleDataType type, string key, string value)
		{
		}

		public string LoadString(string key)
		{
			return null;
		}

		public int? LoadInt(string key)
		{
			return null;
		}

		public float? LoadFloat(string key)
		{
			return null;
		}
	}
}
