using UnityEngine;
using Zenject;
using _Code.Infrastructure.MainMenu;
using _Code.Infrastructure.Settings;
using _Code.Infrastructure.ViewProvider;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure
{
	public sealed class MainSceneInstaller : MonoInstaller
	{
		[SerializeField]
		private ResourceMother _resourceMother;

		[SerializeField]
		private MainMenuViewProvider _mainMenuViewProvider;

		[SerializeField]
		private SoundServiceInstanceProvider _soundServiceInstance;

		[SerializeField]
		private SettingsInstanceProvider _settingsInstanceProvider;

		[SerializeField]
		private InputHandlerProvider _inputHandlerProvider;

		[SerializeField]
		private _Code.Infrastructure.ViewProvider.ViewProvider _yarnReader;

		public override void InstallBindings()
		{
		}
	}
}
