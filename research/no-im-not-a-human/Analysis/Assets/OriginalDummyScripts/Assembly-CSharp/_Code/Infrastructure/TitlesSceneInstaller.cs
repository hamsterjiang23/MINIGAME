using UnityEngine;
using Zenject;
using _Code.Infrastructure.Cursor;
using _Code.Menues.Titles;
using _Code.Player;

namespace _Code.Infrastructure;

public sealed class TitlesSceneInstaller : MonoInstaller
{
	[SerializeField]
	private ResourceMother _resourceMother;

	public override void InstallBindings()
	{
		Container.Bind<ICursorDataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.Bind<IInputHandlerProvider>().FromComponentInHierarchy().AsCached();
		Container.BindInterfacesAndSelfTo<CursorController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<TitlesController>().AsSingle().NonLazy();
	}
}
