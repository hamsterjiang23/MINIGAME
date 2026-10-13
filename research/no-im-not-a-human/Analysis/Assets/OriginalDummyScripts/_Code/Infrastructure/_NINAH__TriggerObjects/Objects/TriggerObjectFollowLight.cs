using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.TriggerObjects;
using _Code.Player;

namespace _Code.Infrastructure._NINAH__TriggerObjects.Objects
{
	public sealed class TriggerObjectFollowLight : ATriggerObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CResetIsEntered_003Ed__8 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public TriggerObjectFollowLight _003C_003E4__this;

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
		private Light _lightTarget;

		[SerializeField]
		private Light _lightTarget2;

		[SerializeField]
		private float _targetIntensity;

		[SerializeField]
		private Vector3 _bias;

		private bool _isEntered;

		private PlayerController _playerTarget;

		protected override void OnEnterInner(Collider other)
		{
		}

		protected override void OnExitInner(Collider other)
		{
		}

		[AsyncStateMachine(typeof(_003CResetIsEntered_003Ed__8))]
		private UniTask ResetIsEntered()
		{
			return default;
		}

		private void Update()
		{
		}
	}
}
