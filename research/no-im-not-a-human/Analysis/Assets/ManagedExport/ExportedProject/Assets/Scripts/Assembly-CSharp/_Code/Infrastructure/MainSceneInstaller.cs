using UnityEngine;
using Zenject;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure.Localization;
using _Code.Infrastructure.MainMenu;
using _Code.Infrastructure.Settings;
using _Code.Infrastructure.Showcase;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.UI;
using _Code.Infrastructure.ViewProvider;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
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
			base.Container.Bind<IEndingDataProvider>().FromInstance(_resourceMother).AsSingle()
				.NonLazy();
			base.Container.Bind<ICharactersSODataProvider>().FromInstance(_resourceMother).AsSingle()
				.NonLazy();
			base.Container.Bind<IMusicSOData>().FromInstance(_resourceMother).AsSingle()
				.NonLazy();
			base.Container.Bind<IMainMenuViewProvider>().FromInstance(_mainMenuViewProvider).AsSingle()
				.NonLazy();
			base.Container.Bind<ICustomYarnReaderProvider>().FromInstance(_yarnReader).AsSingle()
				.NonLazy();
			base.Container.Bind<ISoundServiceInstanceProvider>().FromInstance(_soundServiceInstance).AsSingle()
				.NonLazy();
			base.Container.Bind<ISettingsInstanceProvider>().FromInstance(_settingsInstanceProvider).AsSingle()
				.NonLazy();
			base.Container.Bind<IInputHandlerProvider>().FromInstance(_inputHandlerProvider).AsSingle()
				.NonLazy();
			base.Container.Bind<IShowcaseManager>().To<ShowcaseManager>().AsSingle()
				.NonLazy();
			base.Container.Bind<ILocalizationManager>().To<LocalizationManager>().AsSingle()
				.NonLazy();
			base.Container.Bind<INotAHumanSoundService>().To<NotAHumanSoundService>().AsSingle()
				.NonLazy();
			base.Container.Bind<IUIInitializer>().To<UIInitializer>().AsSingle()
				.NonLazy();
			base.Container.BindInterfacesAndSelfTo<SettingsManager>().AsSingle().NonLazy();
			base.Container.Bind<IMainMenuController>().To<MainMenuController>().AsSingle()
				.NonLazy();
		}
	}
}
