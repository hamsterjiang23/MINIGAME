using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using UnityEngine;
using UnityEngine.Localization.Components;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure._NINAH__InteractableObjects.Objects.Extra;
using _Code.Menues.HUD;
using _Code.Player;

namespace _Code.Infrastructure._NINAH__InteractableObjects.Objects
{
	public sealed class CalendarInteractable : ZoomInteractable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnLoadAsync_003Ed__7 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public CalendarInteractable _003C_003E4__this;

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
		private LocalizeStringEvent _holidayName;

		[SerializeField]
		private MeshRenderer _holidayPaperSheet;

		[SerializeField]
		private CalendarListData[] _calendarDays;

		private IDayNightController _dayNightController;

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, IDayNightController dayNightController, IInputHandlerProvider inputHandlerProvider)
		{
		}

		private void RefreshPage(int day)
		{
		}

		public override void OnLoad()
		{
		}

		[AsyncStateMachine(typeof(_003COnLoadAsync_003Ed__7))]
		private UniTaskVoid OnLoadAsync()
		{
			return default;
		}
	}
}
