using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using _Code.Infrastructure.DataModel;
using _Code.Utils.CustomJson;
using _Code.Utils.Logger;

namespace _Scripts.Services.DataModel.DataStorages;

public sealed class EncryptedPrefsDataStorage : IDataStorage
{
	private readonly ConditionalLocalLogger _logger = new ConditionalLocalLogger();

	public async UniTask Save(string key, object data, bool useSteamCloud)
	{
		_ = Application.persistentDataPath + "/" + key + ".sav";
		JsonSerializerSettings settings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Objects,
			Converters = new List<JsonConverter>
			{
				new ASavableDataConverter()
			}
		};
		string text = JsonConvert.SerializeObject(data, settings);
		_logger.Log("Encrypted Prefs Data Storage Save : " + text);
		_logger.Log("Encrypted Prefs Data Storage Save : " + AesCrypter.Encrypt(text));
		PlayerPrefs.SetString(key, AesCrypter.Encrypt(text));
		PlayerPrefs.Save();
		if (useSteamCloud)
		{
			await SteamCloudSaver.SaveToSteamCloud(key, AesCrypter.Encrypt(text));
		}
	}

	public async UniTask<T> Load<T>(string key, bool useSteamCloud)
	{
		Debug.Log(Application.persistentDataPath + "/" + key + ".sav");
		JsonSerializerSettings settings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.Objects,
			Converters = new List<JsonConverter>
			{
				new ASavableDataConverter()
			}
		};
		string text = "";
		if (useSteamCloud)
		{
			Debug.Log("Start load from steam");
			text = await SteamCloudSaver.LoadFromSteamCloud(key);
			Debug.Log("Read from Steam Cloud: " + text);
			if (!string.IsNullOrEmpty(text))
			{
				PlayerPrefs.SetString(key, text);
				PlayerPrefs.Save();
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			if (!PlayerPrefs.HasKey(key))
			{
				return default;
			}
			Debug.Log("Json is still empty");
			text = PlayerPrefs.GetString(key);
			Debug.Log("Read from device: " + text);
		}
		if (string.IsNullOrEmpty(text))
		{
			return default;
		}
		_logger.Log("Encrypted Prefs Data Storage Load: " + text);
		string value = AesCrypter.Decrypt(text);
		if (string.IsNullOrEmpty(value))
		{
			return default;
		}
		T val = JsonConvert.DeserializeObject<T>(value, settings);
		ConditionalLocalLogger logger = _logger;
		T val2 = val;
		logger.Log("Encrypted Prefs Data Storage Load: " + val2);
		return val;
	}
}
