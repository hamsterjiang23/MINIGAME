using UnityEngine;

namespace _Code.Infrastructure.Windows
{
	public sealed class WindowsViewProvider : MonoBehaviour, IWindowsViewProvider
	{
		[field: SerializeField]
		public WindowView[] WindowsViews { get; private set; }
	}
}
