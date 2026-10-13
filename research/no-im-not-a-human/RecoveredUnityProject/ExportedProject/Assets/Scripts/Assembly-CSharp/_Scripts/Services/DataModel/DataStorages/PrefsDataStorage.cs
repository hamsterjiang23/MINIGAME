using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace _Scripts.Services.DataModel.DataStorages
{
	public sealed class PrefsDataStorage : IDataStorage
	{
		public async UniTask Save(string key, object data, bool useSteamCloud)
		{
			string value = JsonConvert.SerializeObject(data);
			PlayerPrefs.SetString(key, value);
			PlayerPrefs.Save();
		}

		public async UniTask<T> Load<T>(string key, bool useSteamCloud)
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return default;
			}
			return JsonConvert.DeserializeObject<T>(PlayerPrefs.GetString(key));
		}
	}
}
