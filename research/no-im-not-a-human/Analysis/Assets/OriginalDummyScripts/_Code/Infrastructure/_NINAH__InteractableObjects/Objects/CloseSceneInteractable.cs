using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.EnumEventBus;
using _Code.Infrastructure.Pause;
using _Code.Menues.HUD;

namespace _Code.Infrastructure._NINAH__InteractableObjects.Objects
{
	public sealed class CloseSceneInteractable : AInteractableObject
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInteractAsync_003Ed__9 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public CloseSceneInteractable _003C_003E4__this;

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

		private IHUDPresenter _hudPresenter;

		private CommonEnumEventus _commonEnumEventus;

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, CommonEnumEventus commonEnumEventus)
		{
		}

		public override void Interact()
		{
		}

		[AsyncStateMachine(typeof(_003CInteractAsync_003Ed__9))]
		private UniTaskVoid InteractAsync()
		{
			return default;
		}
	}
}
