using System;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure._NINAH__Cat;

[Serializable]
public sealed class CatSaveData : ASavableData
{
	[field: SerializeField]
	public bool IsActive { get; set; }

	[field: SerializeField]
	public int LastPosIndex { get; set; }

	[field: SerializeField]
	public ETimeOfDay LastTimeOfDay { get; set; }
}
