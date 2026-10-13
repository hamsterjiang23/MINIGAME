using System;
using UnityEngine;

namespace _Code.Infrastructure._NINAH__Effects;

[Serializable]
public sealed class FogData
{
	[field: SerializeField]
	public float StartDistance { get; private set; }

	[field: SerializeField]
	public float EndDistance { get; private set; }

	[field: SerializeField]
	public Color Color { get; private set; }
}
