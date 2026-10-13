using System.Collections.Generic;
using Zenject;
using _Code.Infrastructure.Cursor;
using _Code.Player.Markers;

namespace _Code.Player
{
	public sealed class WatcherManager : ITickable
	{
		private readonly Stack<EWatcherState> _states = new Stack<EWatcherState>();

		private Dictionary<EWatcherState, Watcher> _statesDictionary = new Dictionary<EWatcherState, Watcher>();

		private Dictionary<EWatcherState, CursorState> _cursorStates = new Dictionary<EWatcherState, CursorState>();

		private EInputDevice _currentInput = EInputDevice.MouseAndKeyboard;

		private Watcher _currentWatcher;

		private readonly ICursorController _cursorController;

		public WatcherManager(ICursorController cursorController)
		{
			_cursorController = cursorController;
			Init();
		}

		public void Init()
		{
			_statesDictionary = new Dictionary<EWatcherState, Watcher>
			{
				{
					EWatcherState.Dialog,
					new Watcher()
				},
				{
					EWatcherState.Pause,
					new Watcher()
				},
				{
					EWatcherState.Phone,
					new Watcher()
				},
				{
					EWatcherState.MainMenu,
					new Watcher()
				},
				{
					EWatcherState.ConsumableController,
					new Watcher()
				},
				{
					EWatcherState.Popup,
					new Watcher()
				},
				{
					EWatcherState.Settings,
					new Watcher()
				},
				{
					EWatcherState.CollectionCharacters,
					new Watcher()
				},
				{
					EWatcherState.CollectionEndings,
					new Watcher()
				}
			};
			_cursorStates = new Dictionary<EWatcherState, CursorState>
			{
				{
					EWatcherState.World3d,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: false)
				},
				{
					EWatcherState.Credits,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: false)
				},
				{
					EWatcherState.Room,
					new CursorState(enabledForGamepad: true, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.Dialog,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.Phone,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.Pause,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.Settings,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.MainMenu,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.Movie,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: false)
				},
				{
					EWatcherState.ConsumableController,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.Radio,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.Popup,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.Peephole,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: false)
				},
				{
					EWatcherState.Window,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: false)
				},
				{
					EWatcherState.CollectionCharacters,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.CollectionEndings,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: true)
				},
				{
					EWatcherState.Dream,
					new CursorState(enabledForGamepad: false, enabledForKeyboardAndMouse: false)
				}
			};
			_statesDictionary[EWatcherState.Dialog].Init<DialogMarker>();
			_statesDictionary[EWatcherState.Pause].Init<PauseMarker>();
			_statesDictionary[EWatcherState.Phone].Init<PhoneMarker>();
			_statesDictionary[EWatcherState.MainMenu].Init<MainMenuMarker>();
			_statesDictionary[EWatcherState.ConsumableController].Init<ConsumableControllerMarker>();
			_statesDictionary[EWatcherState.Popup].Init<PopupMarker>();
			_statesDictionary[EWatcherState.Settings].Init<SettingsMarker>();
			_statesDictionary[EWatcherState.CollectionCharacters].Init<CollectionCharactersMarker>();
			_statesDictionary[EWatcherState.CollectionEndings].Init<CollectionEndingsMarker>();
		}

		public void EnterState(EWatcherState state)
		{
			_states.Push(state);
			UpdateWatchers();
			UpdateCursor();
		}

		public void LeaveState(EWatcherState state)
		{
			if (_states.Peek() == state)
			{
				_states.Pop();
			}
			UpdateWatchers();
			UpdateCursor();
		}

		public void ChangeInput(EInputDevice inputDevice)
		{
			_currentInput = inputDevice;
			UpdateWatchers();
			UpdateCursor();
		}

		private void UpdateWatchers()
		{
			DisableAllWatchers();
			if (_currentInput != EInputDevice.MouseAndKeyboard && _states.Count != 0)
			{
				EWatcherState key = _states.Peek();
				if (_statesDictionary.TryGetValue(key, out var value))
				{
					_currentWatcher = value;
					_currentWatcher.IsEnabled = true;
				}
			}
		}

		private void DisableAllWatchers()
		{
			foreach (KeyValuePair<EWatcherState, Watcher> item in _statesDictionary)
			{
				item.Value.IsEnabled = false;
			}
			_currentWatcher = null;
		}

		private void UpdateCursor()
		{
			if (_states.Count == 0)
			{
				return;
			}
			CursorState cursorState = _cursorStates[_states.Peek()];
			if (_currentInput == EInputDevice.Gaypad)
			{
				if (cursorState.EnabledForGamepad)
				{
					_cursorController.Unlock();
				}
				else
				{
					_cursorController.Lock();
				}
			}
			else if (cursorState.EnabledForKeyboardAndMouse)
			{
				_cursorController.Unlock();
			}
			else
			{
				_cursorController.Lock();
			}
		}

		public void Tick()
		{
			_currentWatcher?.Tick();
		}
	}
}
