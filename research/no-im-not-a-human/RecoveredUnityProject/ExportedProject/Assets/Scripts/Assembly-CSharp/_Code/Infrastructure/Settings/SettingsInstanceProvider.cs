using UnityEngine;

namespace _Code.Infrastructure.Settings
{
	public sealed class SettingsInstanceProvider : MonoBehaviour, ISettingsInstanceProvider
	{
		[field: SerializeField]
		public SettingsInstance SettingsInstance { get; private set; }
	}
}
