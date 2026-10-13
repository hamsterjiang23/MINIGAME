using UnityEngine;
using Zenject;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DataModel;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.FocusState;
using _Code.Player;

namespace _Code.Infrastructure
{
	public sealed class CommonInstaller : MonoInstaller
	{
		[SerializeField]
		private ResourceMother _resourceMother;

		public override void InstallBindings()
		{
			base.Container.Bind<ICursorDataProvider>().FromInstance(_resourceMother).AsSingle();
			base.Container.BindInterfacesAndSelfTo<CursorController>().AsSingle().NonLazy();
			base.Container.BindInterfacesAndSelfTo<DataModelService>().AsSingle();
			base.Container.BindInterfacesAndSelfTo<CommonEnumEventus>().AsSingle();
			base.Container.BindInterfacesAndSelfTo<FocusStateManager>().AsSingle();
			base.Container.BindInterfacesAndSelfTo<WatcherManager>().AsSingle();
		}
	}
}
