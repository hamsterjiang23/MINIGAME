using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using DG.Tweening;
using UnityEngine;

namespace _Code.DialogSystem
{
	public sealed class DialogSignElementViewMovingContent : ADialogSignElementView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CMove_003Ed__20 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public DialogSignElementViewMovingContent _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private DOTweenAsyncExtensions.TweenAwaiter _003C_003Eu__2;

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

		[field: SerializeField]
		public Vector3 StartPosition { get; private set; }

		[field: SerializeField]
		public Vector3 EndPosition { get; private set; }

		[field: SerializeField]
		public Ease Ease { get; private set; }

		[field: SerializeField]
		public float Duration { get; private set; }

		[field: SerializeField]
		public float BeforeDelay { get; private set; }

		[AsyncStateMachine(typeof(_003CMove_003Ed__20))]
		public UniTask Move()
		{
			return default;
		}
	}
}
