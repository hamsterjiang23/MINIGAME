using System;
using UnityEngine;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Rooms;

[Serializable]
public sealed class WindowImageData
{
	[field: SerializeField]
	public AnimationData Animation { get; private set; }

	[field: SerializeField]
	public Vector2 PositionShift { get; private set; }

	[field: SerializeField]
	public Vector2 ScaleShift { get; private set; }
}
