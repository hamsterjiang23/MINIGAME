using System.Collections.Generic;
using UnityEngine;

namespace _Code.Infrastructure.Updatable
{
	public sealed class UpdaterInstance : MonoBehaviour, IUpdaterInstance
	{
		private IReadOnlyList<IUpdateable> _updatables = new List<IUpdateable>();

		private bool _isActive;

		public void Init(IReadOnlyList<IUpdateable> updatables)
		{
			_updatables = updatables;
		}

		public void SetActiveState(bool isActive)
		{
			_isActive = isActive;
		}

		private void Update()
		{
			if (_isActive)
			{
				for (int i = 0; i < _updatables.Count; i++)
				{
					_updatables[i].OnUpdateAction();
				}
			}
		}
	}
}
