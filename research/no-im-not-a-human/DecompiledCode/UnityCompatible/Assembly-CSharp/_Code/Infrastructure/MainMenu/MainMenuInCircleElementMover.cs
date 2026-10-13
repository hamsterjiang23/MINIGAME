using DG.Tweening;
using UnityEngine;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuInCircleElementMover : MonoBehaviour
	{
		[SerializeField]
		private float _distance = 10f;

		[SerializeField]
		private float _speed = 10f;

		private Vector2 _position;

		private float _moveDuration;

		private float _lastMoveTime;

		private Vector2 _startPosition;

		private void Start()
		{
			_startPosition = transform.localPosition;
		}

		private void Update()
		{
			if (_lastMoveTime + _moveDuration < Time.time)
			{
				_position = Random.insideUnitCircle.normalized * _distance;
				_lastMoveTime = Time.time;
				_moveDuration = Vector2.Distance(transform.position, _position) / _speed;
				transform.DOLocalMove(_startPosition + _position, _moveDuration).SetEase(Ease.Linear);
			}
		}
	}
}
