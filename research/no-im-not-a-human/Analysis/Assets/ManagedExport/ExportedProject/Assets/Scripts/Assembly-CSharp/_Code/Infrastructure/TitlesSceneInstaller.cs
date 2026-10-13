using UnityEngine;
using Zenject;
using _Code.Infrastructure.Cursor;
using _Code.Menues.Titles;
using _Code.Player;

namespace _Code.Infrastructure
{
	public sealed class TitlesSceneInstaller : MonoInstaller
	{
		[SerializeField]
		private ResourceMother _resourceMother;

		public override void InstallBindings()
		{
			base.Container.Bind<ICursorDataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.Bind<IInputHandlerProvider>().FromComponentInHierarchy().AsCached();
			base.Container.BindInterfacesAndSelfTo<CursorController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<TitlesController>().AsSingle().NonLazy();
		}
	}
}
