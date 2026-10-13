using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DataModel.Models.Meta;
using _Code.Infrastructure.DataModel.Models.Settings;
using _Scripts.Services.DataModel;
using _Scripts.Services.DataModel.DataStorages;

namespace _Code.Infrastructure.DataModel
{
	public sealed class DataModelService : IDataModelService
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInitAsync_003Ed__36 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public DataModelService _003C_003E4__this;

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
		private struct _003CLoadCloudData_003Ed__37 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public DataModelService _003C_003E4__this;

			private List<string> _003Cerrors_003E5__2;

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
		private struct _003CLoadCloudHandlerData_003Ed__38 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public UniTask loadTask;

			public DataModelService _003C_003E4__this;

			public List<string> errors;

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
		private struct _003CWaitForInitialization_003Ed__34 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public DataModelService _003C_003E4__this;

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

		[CompilerGenerated]
		private Action<bool> m_CloudSaveLoadingChanged;

		[CompilerGenerated]
		private Action<string> m_CloudSaveLoadFailed;

		private readonly PrefsDataStorage _prefsDataStorage;

		private readonly EncryptedPrefsDataStorage _encryptedPrefsDataStorage;

		private readonly RamDataStorage _ramDataStorage;

		public IGameSaveDataHandler GameSaveDataHandler { get; private set; }

		public ISettingsPrefsDataHandler SettingsPrefsDataHandler { get; private set; }

		public IMetaPrefsDataHandler MetaPrefsDataHandler { get; private set; }

		public bool IsInitialized { get; private set; }

		public bool IsCloudSaveLoading { get; private set; }

		public string CloudSaveLoadError { get; private set; }

		public event Action<bool> CloudSaveLoadingChanged
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

		public event Action<string> CloudSaveLoadFailed
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

		[AsyncStateMachine(typeof(_003CWaitForInitialization_003Ed__34))]
		public UniTask WaitForInitialization()
		{
			return default;
		}

		private void Init()
		{
		}

		[AsyncStateMachine(typeof(_003CInitAsync_003Ed__36))]
		private UniTaskVoid InitAsync()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CLoadCloudData_003Ed__37))]
		private UniTask LoadCloudData()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CLoadCloudHandlerData_003Ed__38))]
		private UniTask LoadCloudHandlerData(UniTask loadTask, List<string> errors)
		{
			return default;
		}

		private void CollectSteamCloudLoadError(List<string> errors)
		{
		}

		private void SetCloudSaveLoading(bool isLoading)
		{
		}
	}
}
