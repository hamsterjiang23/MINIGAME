using System;
using UnityEngine;
using _Code.Player.Markers;

namespace _Code.Player
{
	public sealed class Watcher
	{
		private const float Delay = 1f;

		private float _lastChange;

		private Type _markerType;

		private GameObject _cachedFirstObject;

		public bool IsEnabled { get; set; }

		public string Type => null;

		public void Init<T>() where T : AMarker
		{
		}

		public void Tick()
		{
		}

		private void Watch()
		{
		}

		private bool TryGetMarker(GameObject go, out AMarker marker)
		{
			marker = null;
			return false;
		}

		private void SelectFirstObject()
		{
		}
	}
}
