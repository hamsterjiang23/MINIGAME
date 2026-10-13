using System;
using Newtonsoft.Json;
using UnityEngine;
using _Code.Infrastructure.DataModel.Models.GameSave;

namespace _Code.Infrastructure.Player;

[Serializable]
public sealed class PlayerServiceSaveData : ASavableData
{
	[field: SerializeField]
	public int DayOfTeethReset { get; set; } = -1;

	[field: SerializeField]
	public int DayOfHandsReset { get; set; } = -1;

	[field: SerializeField]
	public int DayOfEyesReset { get; set; } = -1;

	[field: SerializeField]
	public int DayOfArmpitReset { get; set; } = 2;

	[JsonProperty]
	public float PositionX { get; set; }

	[JsonProperty]
	public float PositionY { get; set; }

	[JsonProperty]
	public float PositionZ { get; set; }

	[field: SerializeField]
	public float Yaw { get; set; }

	[JsonIgnore]
	public Vector3 Position => new Vector3(PositionX, PositionY, PositionZ);
}
