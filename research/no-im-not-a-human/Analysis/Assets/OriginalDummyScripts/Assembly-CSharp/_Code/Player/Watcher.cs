using System;
using UnityEngine;
using UnityEngine.EventSystems;
using _Code.Player.Markers;

namespace _Code.Player;

public sealed class Watcher
{
	private const float Delay = 1f;

	private float _lastChange;

	private Type _markerType;

	private GameObject _cachedFirstObject;

	public bool IsEnabled { get; set; }

	public string Type => _markerType.Name;

	public void Init<T>() where T : AMarker
	{
		_markerType = typeof(T);
	}

	public void Tick()
	{
		if (IsEnabled && Time.unscaledTime > _lastChange + 1f)
		{
			_lastChange = Time.unscaledTime;
			Watch();
		}
	}

	private void Watch()
	{
		if (!TryGetMarker(EventSystem.current.currentSelectedGameObject, out var _))
		{
			SelectFirstObject();
		}
	}

	private bool TryGetMarker(GameObject go, out AMarker marker)
	{
		if (!go || go == null)
		{
			marker = null;
			return false;
		}
		if (_markerType == null)
		{
			marker = null;
			return false;
		}
		if (go.TryGetComponent(_markerType, out var component))
		{
			marker = component as AMarker;
			return marker != null;
		}
		marker = null;
		return false;
	}

	private void SelectFirstObject()
	{
		if (_cachedFirstObject == null)
		{
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsByType(_markerType, FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			foreach (UnityEngine.Object obj in array)
			{
				if (((Component)obj).TryGetComponent<FirstMarker>(out var _))
				{
					_cachedFirstObject = (obj as Component).gameObject;
					break;
				}
			}
		}
		EventSystem.current.SetSelectedGameObject(_cachedFirstObject);
	}
}
