using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.Player;

namespace _Code.Menues.Titles
{
	public sealed class TitlesController
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInitTitles_003Ed__2 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public IInputHandlerProvider inputHandlerProvider;

			public TitlesController _003C_003E4__this;

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

		private readonly WatcherManager _watcherManager;

		public TitlesController(IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
		{
		}

		[AsyncStateMachine(typeof(_003CInitTitles_003Ed__2))]
		private UniTask InitTitles(IInputHandlerProvider inputHandlerProvider)
		{
			return default;
		}
	}
}
