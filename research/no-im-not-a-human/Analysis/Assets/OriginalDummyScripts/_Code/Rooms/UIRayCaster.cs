using UnityEngine;

namespace _Code.Rooms
{
	public sealed class UIRayCaster : MonoBehaviour
	{
		[SerializeField]
		private Camera _camera;

		public bool IsActive { get; set; }

		public static UIRayCaster Instance { get; private set; }

		private void Awake()
		{
		}

		private void Raycast()
		{
		}

		private void Update()
		{
		}
	}
}
