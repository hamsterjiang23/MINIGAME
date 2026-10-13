using DG.Tweening;
using UnityEngine;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Player;

namespace _Code.Infrastructure.Notepad;

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
		_cursorController = cursorController;
	}

	private void Start()
	{
		_cam = Camera.main;
	}

	public void Open()
	{
		if ((object)_cam == null)
		{
			_cam = Camera.main;
		}
		_camStartRotation = _cam.transform.eulerAngles;
		gameObject.SetActive(value: true);
		_playerService.MakeImmovable();
		transform.DOLocalMoveY(-190f, 0.3f).SetEase(Ease.InSine);
		_isOpened = true;
	}

	public void Close()
	{
		gameObject.SetActive(value: false);
		_playerService.MakeMovable();
		transform.DOLocalMoveY(-1000f, 0.3f).SetEase(Ease.InSine);
		_isOpened = false;
	}

	private void Update()
	{
		if (_isOpened)
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition = new Vector3(mousePosition.x / (float)Screen.width - 0.5f, mousePosition.y / (float)Screen.height - 0.5f, 0f);
			_cam.transform.eulerAngles = _camStartRotation + new Vector3(0f - mousePosition.y, mousePosition.x);
			if (_isMouseDown)
			{
				transform.localPosition = new Vector3(mousePosition.x * -150f, -190f + mousePosition.y * -150f, -250f);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				_page1.DOScaleY(0f, 0.2f).SetEase(Ease.InSine);
				_page1.DOLocalMoveY(-740f, 0.2f).SetEase(Ease.InSine);
			}
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				_page1.DOScaleY(1f, 0.2f).SetEase(Ease.InSine);
				_page1.DOLocalMoveY(0f, 0.2f).SetEase(Ease.InSine);
			}
			if (Input.GetMouseButtonDown(0))
			{
				_isMouseDown = true;
				transform.DOLocalMoveZ(-250f, 0.1f).SetEase(Ease.InSine);
			}
			if (Input.GetMouseButtonUp(0))
			{
				_isMouseDown = false;
				transform.DOLocalMove(new Vector3(0f, -190f, 0f), 0.1f).SetEase(Ease.InSine);
			}
		}
	}
}
