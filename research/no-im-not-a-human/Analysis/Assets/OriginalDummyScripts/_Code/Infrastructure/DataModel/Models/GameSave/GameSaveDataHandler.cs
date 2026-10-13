using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.Utils.Logger;
using _Scripts.Services.DataModel.DataStorages;
using _Scripts.Services.DataModel.Models;

namespace _Code.Infrastructure.DataModel.Models.GameSave
{
	public sealed class GameSaveDataHandler : ABaseDataHandler<GameSaveData>, IGameSaveDataHandler
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CSaveAllReserve_003Ed__13 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public GameSaveDataHandler _003C_003E4__this;

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

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CSaveAsync_003Ed__14 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public GameSaveDataHandler _003C_003E4__this;

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

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CSaveAsyncReserve_003Ed__15 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public GameSaveDataHandler _003C_003E4__this;

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

		private ConditionalLocalLogger _logger;

		[CompilerGenerated]
		private Action<IGameSaveDataHandler> m_LoadedAll;

		[CompilerGenerated]
		private Action<bool> m_SavedAll;

		protected override bool UseSteamCloud => false;

		public bool NeedToLoadGame => false;

		public bool HasSaveData => false;

		public event Action<IGameSaveDataHandler> LoadedAll
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

		public event Action<bool> SavedAll
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

		public GameSaveDataHandler(IDataStorage dataStorage)
			: base((IDataStorage)null)
		{
		}

		public void ConnectKey<T>(ASavableClass<T> savable, T data) where T : ASavableData
		{
		}

		public T LoadKey<T>(ASavableClass<T> savable) where T : ASavableData
		{
			return null;
		}

		public void SaveAll()
		{
		}

		[AsyncStateMachine(typeof(_003CSaveAllReserve_003Ed__13))]
		public UniTask SaveAllReserve()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CSaveAsync_003Ed__14))]
		private UniTaskVoid SaveAsync()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CSaveAsyncReserve_003Ed__15))]
		private UniTask SaveAsyncReserve()
		{
			return default;
		}

		public void LoadAll()
		{
		}

		public void PrepareToLoadGame()
		{
		}

		public void UnprepareToLoadGame()
		{
		}

		public void LoadIfNeeded()
		{
		}

		public void ClearSavedData()
		{
		}

		public void ResetEvents()
		{
		}
	}
}
