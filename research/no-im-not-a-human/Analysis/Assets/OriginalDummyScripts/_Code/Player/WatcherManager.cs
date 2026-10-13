using System.Collections.Generic;
using Zenject;
using _Code.Infrastructure.Cursor;

namespace _Code.Player
{
	public sealed class WatcherManager : ITickable
	{
		private readonly Stack<EWatcherState> _states;

		private Dictionary<EWatcherState, Watcher> _statesDictionary;

		private Dictionary<EWatcherState, CursorState> _cursorStates;

		private EInputDevice _currentInput;

		private Watcher _currentWatcher;

		private readonly ICursorController _cursorController;

		public WatcherManager(ICursorController cursorController)
		{
		}

		public void Init()
		{
		}

		public void EnterState(EWatcherState state)
		{
		}

		public void LeaveState(EWatcherState state)
		{
		}

		public void ChangeInput(EInputDevice inputDevice)
		{
		}

		private void UpdateWatchers()
		{
		}

		private void DisableAllWatchers()
		{
		}

		private void UpdateCursor()
		{
		}

		public void Tick()
		{
		}
	}
}
