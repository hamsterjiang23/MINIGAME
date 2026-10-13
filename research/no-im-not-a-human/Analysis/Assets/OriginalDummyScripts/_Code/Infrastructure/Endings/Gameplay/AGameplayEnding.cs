using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.Infrastructure.Endings.View;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public abstract class AGameplayEnding
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CShowClip_003Ed__13 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public float delayBeforeShow;

			public AGameplayEnding _003C_003E4__this;

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
		private Action m_Triggered;

		private IEndingShower _endingShower;

		public abstract int Priority { get; }

		public abstract EEnding Ending { get; }

		public abstract bool AreConditionsMet { get; }

		public event Action Triggered
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

		protected abstract void TriggerInner();

		public void Trigger()
		{
		}

		public void PreInit(IEndingShower endingShower)
		{
		}

		[AsyncStateMachine(typeof(_003CShowClip_003Ed__13))]
		public UniTask ShowClip(float delayBeforeShow = 0f)
		{
			return default;
		}
	}
}
