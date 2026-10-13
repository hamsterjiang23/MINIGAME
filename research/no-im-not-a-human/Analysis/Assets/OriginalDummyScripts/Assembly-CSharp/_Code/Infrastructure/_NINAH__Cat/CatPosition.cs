using System;
using UnityEngine;

namespace _Code.Infrastructure._NINAH__Cat;

[Serializable]
public sealed class CatPosition
{
	[field: SerializeField]
	public Vector3 Position { get; private set; }

	[field: SerializeField]
	public float Rotation { get; private set; }

	[field: SerializeField]
	public ECatAnimation Animation { get; private set; }
}
