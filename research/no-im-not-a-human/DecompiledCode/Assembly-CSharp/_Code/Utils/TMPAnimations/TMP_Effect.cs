using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace _Code.Utils.TMPAnimations;

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
		Init();
	}

	private void Init()
	{
		_effectFunc = _effect switch
		{
			ETMPEffect.Wobbling => (Func<float, Vector2>)Wobble, 
			ETMPEffect.Jumping => Jumping, 
			ETMPEffect.Random => Random, 
			_ => None, 
		};
		if (_effectType == ETMPEffectType.ByWord)
		{
			_wordIndexes = new List<int> { 0 };
			_wordLengths = new List<int>();
			string text = _text.text;
			for (int num = text.IndexOf(' '); num > -1; num = text.IndexOf(' ', num + 1))
			{
				List<int> wordLengths = _wordLengths;
				int num2 = num;
				List<int> wordIndexes = _wordIndexes;
				wordLengths.Add(num2 - wordIndexes[wordIndexes.Count - 1]);
				_wordIndexes.Add(num + 1);
			}
			List<int> wordLengths2 = _wordLengths;
			int length = text.Length;
			List<int> wordIndexes2 = _wordIndexes;
			wordLengths2.Add(length - wordIndexes2[wordIndexes2.Count - 1]);
		}
	}

	private void FixedUpdate()
	{
		if (_effect == ETMPEffect.None)
		{
			return;
		}
		_text.ForceMeshUpdate();
		_mesh = _text.mesh;
		_vertices = _mesh.vertices;
		switch (_effectType)
		{
		case ETMPEffectType.ByVertice:
		{
			for (int k = 0; k < _vertices.Length; k++)
			{
				Vector3 vector2 = _effectFunc(Time.time + (float)k);
				_vertices[k] += vector2;
			}
			break;
		}
		case ETMPEffectType.ByCharacter:
		{
			for (int l = 0; l < _text.textInfo.characterCount; l++)
			{
				int vertexIndex2 = _text.textInfo.characterInfo[l].vertexIndex;
				Vector3 vector3 = _effectFunc(Time.time + (float)l);
				_vertices[vertexIndex2] += vector3;
				_vertices[vertexIndex2 + 1] += vector3;
				_vertices[vertexIndex2 + 2] += vector3;
				_vertices[vertexIndex2 + 3] += vector3;
			}
			break;
		}
		case ETMPEffectType.ByWord:
		{
			for (int i = 0; i < _wordIndexes.Count; i++)
			{
				int num = _wordIndexes[i];
				Vector3 vector = _effectFunc(Time.time + (float)i);
				for (int j = 0; j < _wordLengths[i]; j++)
				{
					int vertexIndex = _text.textInfo.characterInfo[num + j].vertexIndex;
					_vertices[vertexIndex] += vector;
					_vertices[vertexIndex + 1] += vector;
					_vertices[vertexIndex + 2] += vector;
					_vertices[vertexIndex + 3] += vector;
				}
			}
			break;
		}
		}
		_mesh.vertices = _vertices;
		_text.canvasRenderer.SetMesh(_mesh);
	}

	private Vector2 None(float time)
	{
		return Vector2.zero;
	}

	private Vector2 Wobble(float time)
	{
		return new Vector2(Mathf.Sin(time * _timeScale) * _force, Mathf.Cos(time * _timeScale) * _force);
	}

	private Vector2 Jumping(float time)
	{
		return new Vector2(0f, Mathf.Sin(time * _timeScale) * _force);
	}

	private Vector2 Random(float time)
	{
		return new Vector2(UnityEngine.Random.Range(0f - _force, _force), UnityEngine.Random.Range(_force, _force));
	}
}
