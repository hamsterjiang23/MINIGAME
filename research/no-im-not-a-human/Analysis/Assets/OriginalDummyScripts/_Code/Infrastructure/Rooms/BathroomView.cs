using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms
{
	public sealed class BathroomView : ARoomView
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CLoadSink_003Ed__15 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public BathroomView _003C_003E4__this;

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
		private struct _003COnLoadAsync_003Ed__17 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public BathroomView _003C_003E4__this;

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
		private CharacterSOData _donorForPlayerSigns;

		[SerializeField]
		private UIButton _sink;

		[SerializeField]
		private UIButton _baby;

		private IGameplayEndingManager _gameplayEndingManager;

		private IDialogManager _dialogManager;

		private IDayNightController _dayNightController;

		private bool _isStarted;

		private bool _isTalkingWithMirror;

		public void Init(IGameplayEndingManager endingManager, IDialogManager dialogManager, IDayNightController dayNightController)
		{
		}

		private void OnDayChanged(int day)
		{
		}

		public void ActivateBaby()
		{
		}

		private void SelfCheck()
		{
		}

		protected override void OnDialogShowed()
		{
		}

		protected override void OnDialogHidden(bool isEndedDialog, bool isEndedSubtitle)
		{
		}

		private void OnDialogEnded(bool endedDialog, bool endedSubtitle)
		{
		}

		[AsyncStateMachine(typeof(_003CLoadSink_003Ed__15))]
		public UniTaskVoid LoadSink()
		{
			return default;
		}

		public void OnLoad(RoomsSaveData saveData)
		{
		}

		[AsyncStateMachine(typeof(_003COnLoadAsync_003Ed__17))]
		private UniTaskVoid OnLoadAsync()
		{
			return default;
		}
	}
}
