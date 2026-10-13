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
			if (Instance == null)
			{
				Instance = this;
			}
			else
			{
				Object.Destroy(gameObject);
			}
		}

		private void Raycast()
		{
			if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitInfo, 10f))
			{
				Debug.Log(hitInfo.transform.name);
				if (hitInfo.collider.gameObject.TryGetComponent<UIButton>(out var component))
				{
					component.Click();
				}
			}
		}

		private void Update()
		{
			if (IsActive && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E)))
			{
				Raycast();
			}
		}
	}
}
