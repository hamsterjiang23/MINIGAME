using UnityEngine;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Player;

namespace _Code.Infrastructure.Notepad
{
	public sealed class NotepadView : MonoBehaviour
	{
		[SerializeField]
		private RectTransform _page1;

		[SerializeField]
		private RectTransform _page2;

		private const float OpenCloseTime = 0.3f;

		private const float ClosedYPos = -1000f;

		private const float OpenedYPos = -190f;

		private const float ClosedYPosPage = -740f;

		private const float OpenedYPosPage = 0f;

		private Vector3 _camStartRotation;

		private Camera _cam;

		private bool _isMouseDown;

		private bool _isOpened;

		private ICursorController _cursorController;

		private IPlayerService _playerService;

		public void Init(ICursorController cursorController, IPlayerService playerService)
		{
		}

		private void Start()
		{
		}

		public void Open()
		{
		}

		public void Close()
		{
		}

		private void Update()
		{
		}
	}
}
