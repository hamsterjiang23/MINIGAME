using UnityEngine;
using UnityEngine.Rendering;

namespace _Code.Infrastructure._NINAH__Effects;

public interface IEffectsViewProvider
{
	Volume Volume { get; }

	Camera Camera { get; }

	VolumeProfile NightVolumeProfile { get; }

	VolumeProfile DayVolumeProfile { get; }

	GameObject RoomSmoke3D { get; }
}
