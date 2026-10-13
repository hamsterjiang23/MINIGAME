using UnityEngine;

namespace _Code.Infrastructure.MainMenu
{
	public sealed class MainMenuViewProvider : MonoBehaviour, IMainMenuViewProvider
	{
		[field: SerializeField]
		public MainMenuView MainMenuView { get; private set; }
	}
}
