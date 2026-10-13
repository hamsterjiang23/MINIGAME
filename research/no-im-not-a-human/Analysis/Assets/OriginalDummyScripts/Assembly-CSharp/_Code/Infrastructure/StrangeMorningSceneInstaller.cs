using UnityEngine;
using Zenject;
using _Code.DialogSystem;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.TriggerObjects;
using _Code.Infrastructure.ViewProvider;
using _Code.Infrastructure._NINAH__DayNight;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure;

public sealed class StrangeMorningSceneInstaller : MonoInstaller
{
	[SerializeField]
	private ResourceMother _resourceMother;

	public override void InstallBindings()
	{
		Container.Bind<IMusicSOData>().FromInstance(_resourceMother).AsSingle()
			.NonLazy();
		Container.Bind<IPlayerViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IInputHandlerProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<ISoundServiceInstanceProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IViewProvider>().FromComponentInHierarchy().AsCached();
		Container.Bind<IBodyEaterTriggerObjectsViewProvider>().FromComponentInHierarchy().AsCached();
		Container.BindInterfacesAndSelfTo<MockPauseController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<MockDayNightController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<MockCursorController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<MockDialogManager>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<MockHudPresenter>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<MockSoundService>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<PlayerServiceMorning>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<BodyEaterTriggerObjectsController>().AsSingle().NonLazy();
	}
}
