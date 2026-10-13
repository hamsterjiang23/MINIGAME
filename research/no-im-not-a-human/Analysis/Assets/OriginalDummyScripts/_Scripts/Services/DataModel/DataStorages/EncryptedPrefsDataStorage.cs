using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using Newtonsoft.Json;
using _Code.Utils.Logger;

namespace _Scripts.Services.DataModel.DataStorages
{
	public sealed class EncryptedPrefsDataStorage : IDataStorage
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CLoad_003Ed__3<T> : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder<T> _003C_003Et__builder;

			public string key;

			public bool useSteamCloud;

			public EncryptedPrefsDataStorage _003C_003E4__this;

			private JsonSerializerSettings _003Csettings_003E5__2;

			private UniTask<string>.Awaiter _003C_003Eu__1;

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
		private struct _003CSave_003Ed__2 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public string key;

			public object data;

			public EncryptedPrefsDataStorage _003C_003E4__this;

			public bool useSteamCloud;

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

		private readonly ConditionalLocalLogger _logger;

		[AsyncStateMachine(typeof(_003CSave_003Ed__2))]
		public UniTask Save(string key, object data, bool useSteamCloud)
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CLoad_003Ed__3<>))]
		public UniTask<T> Load<T>(string key, bool useSteamCloud)
		{
			return default;
		}
	}
}
