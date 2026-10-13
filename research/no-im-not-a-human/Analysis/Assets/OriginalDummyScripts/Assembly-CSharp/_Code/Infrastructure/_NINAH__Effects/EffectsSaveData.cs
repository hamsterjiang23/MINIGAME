using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure._NINAH__Effects;

public sealed class EffectsSaveData : ASavableData
{
	[field: SerializeField]
	public bool IsSmokeEnabled { get; set; }
}
