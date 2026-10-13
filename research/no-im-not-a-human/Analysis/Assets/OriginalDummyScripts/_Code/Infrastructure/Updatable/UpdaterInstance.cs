using System.Collections.Generic;
using UnityEngine;

namespace _Code.Infrastructure.Updatable
{
	public sealed class UpdaterInstance : MonoBehaviour, IUpdaterInstance
	{
		private IReadOnlyList<IUpdateable> _updatables;

		private bool _isActive;

		public void Init(IReadOnlyList<IUpdateable> updatables)
		{
		}

		public void SetActiveState(bool isActive)
		{
		}

		private void Update()
		{
		}
	}
}
