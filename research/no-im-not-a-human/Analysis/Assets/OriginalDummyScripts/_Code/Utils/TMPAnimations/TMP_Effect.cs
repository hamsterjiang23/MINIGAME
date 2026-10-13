using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Code.Utils.TMPAnimations
{
	public sealed class TMP_Effect : MonoBehaviour
	{
		[SerializeField]
		private ETMPEffect _effect;

		[SerializeField]
		private ETMPEffectType _effectType;

		[SerializeField]
		private TMP_Text _text;

		[SerializeField]
		private float _force;

		[SerializeField]
		private float _timeScale;

		private Mesh _mesh;

		private Vector3[] _vertices;

		private Func<float, Vector2> _effectFunc;

		private List<int> _wordIndexes;

		private List<int> _wordLengths;

		private void Awake()
		{
		}

		private void Init()
		{
		}

		private void FixedUpdate()
		{
		}

		private Vector2 None(float time)
		{
			return default;
		}

		private Vector2 Wobble(float time)
		{
			return default;
		}

		private Vector2 Jumping(float time)
		{
			return default;
		}

		private Vector2 Random(float time)
		{
			return default;
		}
	}
}
