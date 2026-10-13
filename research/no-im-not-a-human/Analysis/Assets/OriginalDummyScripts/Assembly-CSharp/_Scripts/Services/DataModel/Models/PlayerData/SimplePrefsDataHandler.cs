using System;
using System.Globalization;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Scripts.Services.DataModel.DataStorages;
using _Scripts.Services.DataModel.Models.CustomData;

namespace _Scripts.Services.DataModel.Models.PlayerData;

public sealed class SimplePrefsDataHandler : ABaseDataHandler<SimplePrefsData>, ISimplePrefsDataHandler
{
	protected override bool UseSteamCloud => false;

	public SimplePrefsDataHandler(IDataStorage dataStorage)
		: base(dataStorage)
	{
	}

	public void SavePref(ESimpleDataType type, string key, string value)
	{
		SimpleDataType simpleDataType = Data.dataTypes.FirstOrDefault((SimpleDataType x) => x._prefName == key);
		if (simpleDataType == null)
		{
			Data.dataTypes.Add(new SimpleDataType(type, key, value));
			Debug.Log(type.ToString() + " " + key + " " + value);
		}
		else
		{
			if (simpleDataType._simpleDataType != type)
			{
				throw new Exception("Prefs type doesn't match");
			}
			simpleDataType._prefValue = value;
			Debug.Log(type.ToString() + " " + key + " " + value + "!");
		}
		SaveData().Forget();
	}

	public string LoadString(string key)
	{
		SimpleDataType simpleDataType = Data.dataTypes.FirstOrDefault((SimpleDataType x) => x._prefName == key);
		if (simpleDataType != null && simpleDataType._simpleDataType == ESimpleDataType.String)
		{
			return simpleDataType._prefValue;
		}
		return null;
	}

	public int? LoadInt(string key)
	{
		SimpleDataType simpleDataType = Data.dataTypes.FirstOrDefault((SimpleDataType x) => x._prefName == key);
		if (simpleDataType != null && simpleDataType._simpleDataType == ESimpleDataType.Int)
		{
			return int.Parse(simpleDataType._prefValue);
		}
		return null;
	}

	public float? LoadFloat(string key)
	{
		SimpleDataType simpleDataType = Data.dataTypes.FirstOrDefault((SimpleDataType x) => x._prefName == key);
		if (simpleDataType != null && simpleDataType._simpleDataType == ESimpleDataType.Float)
		{
			return float.Parse(simpleDataType._prefValue, CultureInfo.InvariantCulture);
		}
		return null;
	}
}
