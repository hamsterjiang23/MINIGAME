using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms
{
	public sealed class OfficeView : ARoomView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnLoadAsync_003Ed__7 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public OfficeView _003C_003E4__this;

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
		private UIButton _baby;

		[SerializeField]
		private GameObject _cultistsSheets;

		private IGameplayEndingManager _gameplayEndingManager;

		public void Init(IGameplayEndingManager endingManager)
		{
		}

		public void ActivateBaby()
		{
		}

		public void ActivateCultists()
		{
		}

		public void OnLoad(RoomsSaveData saveData)
		{
		}

		[AsyncStateMachine(typeof(_003COnLoadAsync_003Ed__7))]
		private UniTaskVoid OnLoadAsync()
		{
			return default;
		}
	}
}
