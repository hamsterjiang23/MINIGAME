using UnityEngine;

namespace _Code.Infrastructure.Settings
{
	public abstract class ASettingsInstance : MonoBehaviour
	{
		public abstract ISetting Setting { get; }

		public void Initialize()
		{
			Init();
			UpdateVisualsForLoadedData();
		}

		protected abstract void Init();

		protected abstract void UpdateVisualsForLoadedData();
	}
}
