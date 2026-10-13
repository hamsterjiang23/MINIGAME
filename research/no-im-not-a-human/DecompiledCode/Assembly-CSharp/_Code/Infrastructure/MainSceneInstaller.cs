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

namespace _Code.Infrastructure;

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
		Container.Bind<IEndingDataProvider>().FromInstance(_resourceMother).AsSingle()
			.NonLazy();
		Container.Bind<ICharactersSODataProvider>().FromInstance(_resourceMother).AsSingle()
			.NonLazy();
		Container.Bind<IMusicSOData>().FromInstance(_resourceMother).AsSingle()
			.NonLazy();
		Container.Bind<IMainMenuViewProvider>().FromInstance(_mainMenuViewProvider).AsSingle()
			.NonLazy();
		Container.Bind<ICustomYarnReaderProvider>().FromInstance(_yarnReader).AsSingle()
			.NonLazy();
		Container.Bind<ISoundServiceInstanceProvider>().FromInstance(_soundServiceInstance).AsSingle()
			.NonLazy();
		Container.Bind<ISettingsInstanceProvider>().FromInstance(_settingsInstanceProvider).AsSingle()
			.NonLazy();
		Container.Bind<IInputHandlerProvider>().FromInstance(_inputHandlerProvider).AsSingle()
			.NonLazy();
		Container.Bind<IShowcaseManager>().To<ShowcaseManager>().AsSingle()
			.NonLazy();
		Container.Bind<ILocalizationManager>().To<LocalizationManager>().AsSingle()
			.NonLazy();
		Container.Bind<INotAHumanSoundService>().To<NotAHumanSoundService>().AsSingle()
			.NonLazy();
		Container.Bind<IUIInitializer>().To<UIInitializer>().AsSingle()
			.NonLazy();
		Container.BindInterfacesAndSelfTo<SettingsManager>().AsSingle().NonLazy();
		Container.Bind<IMainMenuController>().To<MainMenuController>().AsSingle()
			.NonLazy();
	}
}
