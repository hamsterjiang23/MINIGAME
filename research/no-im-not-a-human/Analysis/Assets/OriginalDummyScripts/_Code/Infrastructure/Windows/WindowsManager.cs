using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.DialogSystem;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Windows
{
	public sealed class WindowsManager : IWindowsManager
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003COnOpenedAsync_003Ed__18 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

			public WindowView window;

			public WindowsManager _003C_003E4__this;

			private UniTask _003CtaskToAwaitResult_003E5__2;

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

		private readonly IWindowView[] _windows;

		private readonly IDayNightController _dayNightController;

		private readonly IDialogManager _dialogManager;

		private readonly IWindowsSODataProvider _windowsDataProvider;

		private readonly RoomDisplayer _roomDisplayer;

		private readonly IPlayerService _playerService;

		private readonly INotAHumanSoundService _soundService;

		private readonly IDataModelService _dataModelService;

		public bool IsInWindow { get; private set; }

		public WindowsManager(IWindowsViewProvider windowsViewProvider, IDayNightController dayNightController, IDialogManager dialogManager, IRoomDisplayerViewProvider roomDisplayerViewProvider, IWindowsSODataProvider windowsDataProvider, IPlayerService playerService, INotAHumanSoundService soundService, IDataModelService dataModelService)
		{
		}

		private void Init()
		{
		}

		private void OnStartOpen(WindowView obj)
		{
		}

		private void OnDayChanged(int day)
		{
		}

		private void StartNoise(ESound sound)
		{
		}

		private void OnOpened(WindowView window)
		{
		}

		[AsyncStateMachine(typeof(_003COnOpenedAsync_003Ed__18))]
		private UniTask OnOpenedAsync(WindowView window)
		{
			return default;
		}

		private void OnClosed(WindowView window)
		{
		}
	}
}
