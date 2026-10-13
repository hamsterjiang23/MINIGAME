using UnityEngine;

namespace _Scripts.Raycast
{
	public sealed class RaycastSource : MonoBehaviour
	{
		[SerializeField]
		private Camera _camera;

		[SerializeField]
		private float _maxRayDistance;

		private ARaycastTarget _currentTarget;

		private int _layerMask;

		public ARaycastTarget Target { get; set; }

		private void Awake()
		{
		}

		private void Update()
		{
		}

		private void OnDrawGizmos()
		{
		}
	}
}
