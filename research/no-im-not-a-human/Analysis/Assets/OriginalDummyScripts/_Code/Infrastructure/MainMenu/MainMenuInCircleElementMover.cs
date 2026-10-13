using UnityEngine;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuInCircleElementMover : MonoBehaviour
	{
		[SerializeField]
		private float _distance;

		[SerializeField]
		private float _speed;

		private Vector2 _position;

		private float _moveDuration;

		private float _lastMoveTime;

		private Vector2 _startPosition;

		private void Start()
		{
		}

		private void Update()
		{
		}
	}
}
