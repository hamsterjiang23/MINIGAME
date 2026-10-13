using RTLTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Infrastructure.Cursor;
using _Code.Player;

namespace _Code.Utils.UI.Popup;

public sealed class PopupWindow : MonoBehaviour
{
	[SerializeField]
	private EventSystem _eventSystem;

	[SerializeField]
	private RTLTextMeshPro _title;

	[SerializeField]
	private PopupButtonView[] _popupButtons;

	[SerializeField]
	private Image _bg;

	private InputHandling _inputHandler;

	private ICursorController _cursorController;

	private WatcherManager _watcherManager;

	public void Show(string title, params PopupButtonData[] buttons)
	{
		_title.text = title;
		for (int i = 0; i < _popupButtons.Length; i++)
		{
			if (buttons.Length > i)
			{
				_popupButtons[i].gameObject.SetActive(value: true);
				_popupButtons[i].Setup(buttons[i]);
			}
		}
		gameObject.SetActive(value: true);
		_watcherManager.EnterState(EWatcherState.Popup);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
		_watcherManager.LeaveState(EWatcherState.Popup);
	}

	private void Update()
	{
		if (_inputHandler.UICancelClicked)
		{
			_popupButtons[0].InvokeCommand();
		}
	}

	public void InitModules(InputHandling inputHandler, ICursorController cursorController, WatcherManager watcherManager)
	{
		_inputHandler = inputHandler;
		_cursorController = cursorController;
		_watcherManager = watcherManager;
	}
}
