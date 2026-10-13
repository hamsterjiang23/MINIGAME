using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;

namespace _Code.Utils.Logger
{
	public sealed class ConditionalLocalLogger
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CConditionAwaiter_003Ed__5 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public ConditionalLocalLogger _003C_003E4__this;

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

		private Func<bool> _condition;

		private Queue<object> _messageQueue;

		public void InitCondition(Func<bool> condition, bool runConditionAwaiter = false)
		{
		}

		public void Log(object message)
		{
		}

		private void CheckQueue()
		{
		}

		[AsyncStateMachine(typeof(_003CConditionAwaiter_003Ed__5))]
		private UniTaskVoid ConditionAwaiter()
		{
			return default;
		}
	}
}
