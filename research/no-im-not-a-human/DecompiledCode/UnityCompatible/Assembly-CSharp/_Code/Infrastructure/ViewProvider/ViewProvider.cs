using UnityEngine;
using _Code.Menues.HUD;
using _Code.Utils.CustomYarnReading;

namespace _Code.Infrastructure.ViewProvider
{
	public sealed class ViewProvider : MonoBehaviour, IViewProvider, ICustomYarnReaderProvider
	{
		[field: SerializeField]
		public HUDView HUDView { get; private set; }

		[field: SerializeField]
		public CustomYarnReader CustomYarnReader { get; private set; }
	}
}
