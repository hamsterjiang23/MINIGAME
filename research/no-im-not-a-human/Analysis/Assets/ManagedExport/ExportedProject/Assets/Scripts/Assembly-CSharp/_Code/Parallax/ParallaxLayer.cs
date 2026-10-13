using System;
using UnityEngine;

namespace _Code.Parallax
{
	[Serializable]
	public sealed class ParallaxLayer
	{
		[field: SerializeField]
		public Transform Transform { get; private set; }

		[field: SerializeField]
		public float PositionStrength { get; private set; }

		[field: SerializeField]
		public float RotationStrength { get; private set; }

		public Vector3 StartPosition { get; private set; }

		public Vector3 StartRotation { get; private set; }

		public void Init(Vector3 startPosition, Vector3 startRotation)
		{
			StartPosition = startPosition;
			StartRotation = startRotation;
		}
	}
}
