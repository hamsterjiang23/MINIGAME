using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;

namespace _Scripts.Services.DataModel.DataStorages
{
	public sealed class RamDataStorage : IDataStorage
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CSave_003Ed__0 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

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

		[AsyncStateMachine(typeof(_003CSave_003Ed__0))]
		public UniTask Save(string key, object data, bool useSteamCloud)
		{
			return default;
		}

		public UniTask<T> Load<T>(string key, bool useSteamCloud)
		{
			return default;
		}
	}
}
