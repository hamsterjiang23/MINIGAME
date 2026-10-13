using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using _Scripts.Services.DataModel.Models;

namespace _Code.Infrastructure.DataModel.Models.GameSave;

[Serializable]
public sealed class GameSaveData : BaseDataStorage
{
	[JsonProperty]
	private Dictionary<string, string> _jsonValues = new Dictionary<string, string>();

	[JsonProperty]
	private Dictionary<string, string> _jsonValuesReserve = new Dictionary<string, string>();

	private Dictionary<Type, ASavableData> _values = new Dictionary<Type, ASavableData>();

	public Dictionary<Type, ASavableData> Values => _values;

	public bool NeedToLoad { get; set; }

	public bool HasSaveData
	{
		get
		{
			if (_jsonValuesReserve.Count <= 0)
			{
				return _jsonValues.Count > 0;
			}
			return true;
		}
	}

	public void CreateJson()
	{
		_jsonValues.Clear();
		foreach (KeyValuePair<string, string> item in _values.ToDictionary((KeyValuePair<Type, ASavableData> x) => x.Key.ToString(), (KeyValuePair<Type, ASavableData> x) => JsonConvert.SerializeObject(x.Value)))
		{
			_jsonValues.Add(item.Key, item.Value);
		}
		Debug.Log(_jsonValues.Aggregate("", (string a, KeyValuePair<string, string> b) => a + b.Key + " : " + b.Value + "\n"));
	}

	public void CreateReserveJson()
	{
		_jsonValuesReserve.Clear();
		foreach (KeyValuePair<string, string> item in _values.ToDictionary((KeyValuePair<Type, ASavableData> x) => x.Key.ToString(), (KeyValuePair<Type, ASavableData> x) => JsonConvert.SerializeObject(x.Value)))
		{
			_jsonValuesReserve.Add(item.Key, item.Value);
		}
		Debug.Log(_jsonValuesReserve.Aggregate("", (string a, KeyValuePair<string, string> b) => a + b.Key + " : " + b.Value + "\n"));
	}

	public T LoadFromJson<T>(ASavableClass<T> savableClass) where T : ASavableData
	{
		Dictionary<string, string> jsonValuesReserve = _jsonValuesReserve;
		KeyValuePair<string, string> keyValuePair = ((jsonValuesReserve != null && jsonValuesReserve.Count > 0) ? _jsonValuesReserve.FirstOrDefault((KeyValuePair<string, string> x) => x.Key.ToString() == savableClass.GetType().ToString()) : _jsonValues.FirstOrDefault((KeyValuePair<string, string> x) => x.Key.ToString() == savableClass.GetType().ToString()));
		if (keyValuePair.Key == null)
		{
			return null;
		}
		if (string.IsNullOrEmpty(keyValuePair.Value))
		{
			return null;
		}
		Debug.Log(keyValuePair.Value);
		_values[savableClass.GetType()] = JsonConvert.DeserializeObject<T>(keyValuePair.Value);
		return _values[savableClass.GetType()] as T;
	}

	public void ClearReserveData()
	{
		_jsonValuesReserve.Clear();
	}

	public void ResetValues()
	{
		_values.Clear();
	}
}
