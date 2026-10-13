using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Player;

namespace _Code.Infrastructure.ControlsViewer
{
	public sealed class ControlsListView : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CClearControls_003Ed__6 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public ControlsListView _003C_003E4__this;

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

		[SerializeField]
		private ControlView[] _controls;

		private int _maxUsedControl;

		private InputHandling _inputHandling;

		public int ActiveCount => 0;

		public void InitModules(IInputHandlerProvider inputHandlerProvider)
		{
		}

		[AsyncStateMachine(typeof(_003CClearControls_003Ed__6))]
		public UniTask ClearControls()
		{
			return default;
		}

		public void AddControl(EControl control)
		{
		}

		public void SetControlAvailability(EControl control, bool isAvailable)
		{
		}
	}
}
