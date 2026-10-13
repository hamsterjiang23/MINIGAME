using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Zenject;
using _Code.Events;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure._NINAH__Cat
{
	public sealed class CatController : ASavableClass<CatSaveData>, ICatController, IInitializable, IDisposable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CActivateCatOnLoad_003Ed__20 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public CatController _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		private CatSaveData _saveData;

		private readonly ICatViewProvider _catViewProvider;

		private CatInstance _cat;

		private ECatAnimation _animation;

		private IDataModelService _dataModelService;

		[CompilerGenerated]
		private Action m_DayStarted;

		[CompilerGenerated]
		private Action m_NightStarted;

		[CompilerGenerated]
		private Action m_Born;

		public bool IsCatActive
		{
			get
			{
				return false;
			}
			private set
			{
			}
		}

		public event Action DayStarted
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action NightStarted
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action Born
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public CatController(ICatViewProvider viewProvider, IDataModelService dataModelService)
		{
		}

		public void Initialize()
		{
		}

		public void ActivateCat()
		{
		}

		[AsyncStateMachine(typeof(_003CActivateCatOnLoad_003Ed__20))]
		private UniTaskVoid ActivateCatOnLoad()
		{
			return default;
		}

		private void ChangePositionTo(ETimeOfDay timeOfDay, int posIndex)
		{
		}

		public void ChangePosition(ETimeOfDay currentTimeOfDay)
		{
		}

		public void PlayAnimation()
		{
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}
	}
}
