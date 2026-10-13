using System;
using DG.Tweening;
using UnityEngine;

namespace _Code.Characters;

[Serializable]
public sealed class CharacterEyeData
{
	[SerializeField]
	private Sprite _white;

	[SerializeField]
	private Sprite _iris;

	[SerializeField]
	private Ease _easing = Ease.Linear;

	[Range(0f, 1f)]
	[SerializeField]
	private float _distanceMultiplier = 0.5f;

	[Range(0.1f, 5f)]
	[SerializeField]
	private float _minMoveDuration = 0.5f;

	[Range(0.1f, 5f)]
	[SerializeField]
	private float _maxMoveDuration = 1f;

	public Sprite White => _white;

	public Sprite Iris => _iris;

	public float DistanceMultiplier => _distanceMultiplier;

	public float MinMoveDuration => _minMoveDuration;

	public float MaxMoveDuration => _maxMoveDuration;

	public Ease Easing => _easing;
}
