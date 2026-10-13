using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Code.Characters.Cat
{
	public sealed class CatPathController : MonoBehaviour
	{
		[SerializeField]
		private Vector3[] _positions;

		[SerializeField]
		private Transform _cat;

		[SerializeField]
		private float _speed;

		private int _currentIndex;

		private bool _isWalking;

		private void Start()
		{
			StartWalking();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.K))
			{
				StartWalking();
			}
			if (Input.GetKeyDown(KeyCode.L))
			{
				_isWalking = false;
			}
		}

		private void StartWalking()
		{
			_isWalking = true;
			WalkAsync().Forget();
		}

		private async UniTaskVoid WalkAsync()
		{
			while (_isWalking)
			{
				await GoToNextPosition();
			}
		}

		private async UniTask GoToNextPosition()
		{
			_currentIndex++;
			if (_currentIndex >= _positions.Length)
			{
				_currentIndex = 0;
			}
			int num = _currentIndex + 1;
			if (num >= _positions.Length)
			{
				num = 0;
			}
			float num2 = Vector3.Distance(_positions[_currentIndex], _cat.position);
			_cat.DOLookAt(_positions[num], num2 / _speed);
			await (Tween)_cat.DOMove(_positions[_currentIndex], num2 / _speed).SetEase(Ease.Linear);
		}

		private void OnDrawGizmosSelected()
		{
			for (int i = 0; i < _positions.Length; i++)
			{
				Gizmos.DrawSphere(_positions[i], 0.1f);
				if (i < _positions.Length - 1)
				{
					Gizmos.DrawLine(_positions[i], _positions[i + 1]);
				}
			}
			Vector3[] positions = _positions;
			Gizmos.DrawLine(positions[positions.Length - 1], _positions[0]);
		}
	}
}
