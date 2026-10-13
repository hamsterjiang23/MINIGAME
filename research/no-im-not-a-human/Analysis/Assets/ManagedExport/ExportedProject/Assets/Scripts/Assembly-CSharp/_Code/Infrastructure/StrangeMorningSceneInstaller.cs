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

namespace _Code.Infrastructure
{
	public sealed class StrangeMorningSceneInstaller : MonoInstaller
	{
		[SerializeField]
		private ResourceMother _resourceMother;

		public override void InstallBindings()
		{
			base.Container.Bind<IMusicSOData>().FromInstance(_resourceMother).AsSingle()
				.NonLazy();
			base.Container.Bind<IPlayerViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IInputHandlerProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<ISoundServiceInstanceProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.Bind<IBodyEaterTriggerObjectsViewProvider>().FromComponentInHierarchy().AsCached();
			base.Container.BindInterfacesAndSelfTo<MockPauseController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<MockDayNightController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<MockCursorController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<MockDialogManager>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<MockHudPresenter>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<MockSoundService>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<PlayerServiceMorning>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<BodyEaterTriggerObjectsController>().AsSingle().NonLazy();
		}
	}
}
