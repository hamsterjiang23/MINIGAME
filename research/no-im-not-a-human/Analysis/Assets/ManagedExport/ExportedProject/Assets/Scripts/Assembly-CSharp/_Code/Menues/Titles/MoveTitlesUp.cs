using UnityEngine;

namespace _Code.Menues.Titles
{
	public sealed class MoveTitlesUp : MonoBehaviour
	{
		[SerializeField]
		private float _speed = 1f;

		[SerializeField]
		private bool _lockInCenter;

		private void Update()
		{
			if (!_lockInCenter || !(base.transform.localPosition.y >= 0f))
			{
				base.transform.Translate(Vector3.up * (Time.deltaTime * _speed));
			}
		}
	}
}
