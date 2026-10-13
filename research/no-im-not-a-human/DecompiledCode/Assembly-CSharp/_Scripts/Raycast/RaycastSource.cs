using UnityEngine;

namespace _Scripts.Raycast;

public sealed class RaycastSource : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	[SerializeField]
	private float _maxRayDistance = 1f;

	private ARaycastTarget _currentTarget;

	private int _layerMask;

	public ARaycastTarget Target { get; set; }

	private void Awake()
	{
		_layerMask = LayerMask.GetMask("Default");
	}

	private void Update()
	{
		if (Physics.Raycast(_camera.ScreenPointToRay(Input.mousePosition), out var hitInfo, _maxRayDistance, _layerMask))
		{
			if (hitInfo.collider.TryGetComponent<ARaycastTarget>(out var component))
			{
				if (!(_currentTarget == component) && (!(Target != null) || !(Target != component)))
				{
					if (_currentTarget != null)
					{
						_currentTarget.IsTargeted = false;
					}
					_currentTarget = component;
					_currentTarget.IsTargeted = true;
				}
			}
			else
			{
				ResetRaycastedObject();
			}
		}
		else
		{
			ResetRaycastedObject();
		}
		bool ResetRaycastedObject()
		{
			if (_currentTarget == null)
			{
				return true;
			}
			_currentTarget.IsTargeted = false;
			_currentTarget = null;
			return false;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawRay(_camera.ScreenPointToRay(Input.mousePosition));
	}
}
