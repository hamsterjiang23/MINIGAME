using UnityEngine;
using Zenject;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DataModel;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.FocusState;
using _Code.Player;

namespace _Code.Infrastructure;

public sealed class CommonInstaller : MonoInstaller
{
	[SerializeField]
	private ResourceMother _resourceMother;

	public override void InstallBindings()
	{
		Container.Bind<ICursorDataProvider>().FromInstance(_resourceMother).AsSingle();
		Container.BindInterfacesAndSelfTo<CursorController>().AsSingle().NonLazy();
		Container.BindInterfacesAndSelfTo<DataModelService>().AsSingle();
		Container.BindInterfacesAndSelfTo<CommonEnumEventus>().AsSingle();
		Container.BindInterfacesAndSelfTo<FocusStateManager>().AsSingle();
		Container.BindInterfacesAndSelfTo<WatcherManager>().AsSingle();
	}
}
