using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.ControlsViewer;
using _Code.Player;

namespace _Code.Menues.Titles
{
	public sealed class TitlesControl : MonoBehaviour
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CResetSkipHint_003Ed__13 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public TitlesControl _003C_003E4__this;

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
		private struct _003CStartCheckingVideoEnd_003Ed__11 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public float length;

			public TitlesControl _003C_003E4__this;

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
		private CanvasGroup _skipFillHint;

		[SerializeField]
		private Image _skippFillBar;

		[SerializeField]
		private ControlView _controlView;

		private CancellationTokenSource _cancellationToken;

		private float _skipProgress;

		private const float SkipDuration = 3f;

		private InputHandling _inputHandler;

		private bool _isSkipping;

		private int _resetsHintCount;

		private WatcherManager _watcherManager;

		public void Init(IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
		{
		}

		[AsyncStateMachine(typeof(_003CStartCheckingVideoEnd_003Ed__11))]
		private UniTaskVoid StartCheckingVideoEnd(float length)
		{
			return default;
		}

		private void Update()
		{
		}

		[AsyncStateMachine(typeof(_003CResetSkipHint_003Ed__13))]
		private UniTaskVoid ResetSkipHint()
		{
			return default;
		}
	}
}
