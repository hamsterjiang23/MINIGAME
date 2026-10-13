using System;
using UnityEngine.Serialization;
using _Scripts.Services.DataModel.Models.CustomData;

namespace _Scripts.Services.DataModel.Models.PlayerData;

[Serializable]
public sealed class SimpleDataType
{
	[FormerlySerializedAs("_prefsType")]
	public ESimpleDataType _simpleDataType;

	public string _prefName;

	public string _prefValue;

	public SimpleDataType(ESimpleDataType simpleDataType, string prefName, string prefValue)
	{
		_simpleDataType = simpleDataType;
		_prefName = prefName;
		_prefValue = prefValue;
	}
}
