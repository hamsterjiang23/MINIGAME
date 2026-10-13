using RTLTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Infrastructure.Cursor;
using _Code.Player;

namespace _Code.Utils.UI.Popup
{
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
		}

		public void Hide()
		{
		}

		private void Update()
		{
		}

		public void InitModules(InputHandling inputHandler, ICursorController cursorController, WatcherManager watcherManager)
		{
		}
	}
}
