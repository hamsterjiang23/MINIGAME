using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.EnumEventBus;

namespace _Code.Infrastructure.TriggerObjects.Objects
{
	public sealed class TriggerObjectCloseScene : ATriggerObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnEnterAsync_003Ed__4 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public TriggerObjectCloseScene _003C_003E4__this;

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
		private int _sceneIndex;

		private CommonEnumEventus _commonEnumEventBus;

		public void Init(CommonEnumEventus commonEnumEventBus)
		{
		}

		protected override void OnEnterInner(Collider other)
		{
		}

		[AsyncStateMachine(typeof(_003COnEnterAsync_003Ed__4))]
		private UniTask OnEnterAsync()
		{
			return default;
		}
	}
}
