using System;
using DG.Tweening;
using UnityEngine;

namespace _Code.Characters
{
	[Serializable]
	public sealed class CharacterEyeData
	{
		[SerializeField]
		private Sprite _white;

		[SerializeField]
		private Sprite _iris;

		[SerializeField]
		private Ease _easing;

		[Range(0f, 1f)]
		[SerializeField]
		private float _distanceMultiplier;

		[Range(0.1f, 5f)]
		[SerializeField]
		private float _minMoveDuration;

		[Range(0.1f, 5f)]
		[SerializeField]
		private float _maxMoveDuration;

		public Sprite White => null;

		public Sprite Iris => null;

		public float DistanceMultiplier => 0f;

		public float MinMoveDuration => 0f;

		public float MaxMoveDuration => 0f;

		public Ease Easing => Ease.Unset;
	}
}
