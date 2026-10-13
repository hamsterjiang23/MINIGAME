using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using _Code.Infrastructure.Cursor;
using _Code.Player;
using _Code.Rooms;

namespace _Code.Infrastructure.Rooms
{
	public sealed class RoomDisplayer : MonoBehaviour
	{
		[SerializeField]
		private GraphicRaycaster _raycaster;

		[SerializeField]
		private EventSystem _eventSystem;

		[SerializeField]
		private Camera _camera;

		[SerializeField]
		private GameObject _backImage;

		private const float FADE_DURATION = 0.5f;

		public const float ANGLE_TO_USE_GLOBAL_OVERLAY = 40f;

		private bool _wasOpenedWithGlobalOverlay;

		private bool _isOpened;

		private bool _isHoveredObject;

		private ICursorController _cursorController;

		private bool _isBlocked;

		private bool _areClicksLocked;

		private InputHandling _inputHandler;

		private UIButton _selectedButton;

		[field: SerializeField]
		public Canvas Canvas { get; private set; }

		[field: SerializeField]
		public Canvas DialogCanvas { get; private set; }

		[field: SerializeField]
		public Image LocalOverlay { get; private set; }

		[field: SerializeField]
		public Image GlobalOverlay { get; private set; }

		public void InitModules(ICursorController cursorController, IInputHandlerProvider inputHandlerProvider)
		{
			_cursorController = cursorController;
			_inputHandler = inputHandlerProvider.InputHandler;
		}

		public async UniTask OpenFadeLocal(float fadeDuration = 0.5f)
		{
			_wasOpenedWithGlobalOverlay = false;
			LocalOverlay.gameObject.SetActive(value: true);
			LocalOverlay.color = Color.black;
			_backImage.gameObject.SetActive(value: true);
			await (Tween)LocalOverlay.DOColor(Color.clear, fadeDuration).SetEase(Ease.InCubic);
			_isOpened = true;
		}

		public async UniTask OpenFadeGlobal(float fadeDuration = 0.5f)
		{
			_wasOpenedWithGlobalOverlay = true;
			await (Tween)GlobalOverlay.DOColor(Color.black, fadeDuration * 0.1f).SetEase(Ease.InCubic);
			_backImage.gameObject.SetActive(value: true);
			await (Tween)GlobalOverlay.DOColor(Color.clear, fadeDuration * 0.9f).SetEase(Ease.InCubic);
			_isOpened = true;
		}

		public async UniTask CloseFade(float fadeDuration = 0.5f)
		{
			_isOpened = false;
			_backImage.gameObject.SetActive(value: false);
			await (Tween)LocalOverlay.DOColor(Color.black, fadeDuration / 2f).SetEase(Ease.OutCubic);
			LocalOverlay.gameObject.SetActive(value: false);
		}

		public void SetData(Camera roomLinkedCamera, float roomCameraDistance, float dialogCanvasDistance)
		{
			Canvas.worldCamera = roomLinkedCamera;
			Canvas.planeDistance = roomCameraDistance;
			DialogCanvas.planeDistance = dialogCanvasDistance;
		}

		private void Update()
		{
			if (!_isOpened)
			{
				return;
			}
			if (_isBlocked)
			{
				if (_isHoveredObject)
				{
					Unhover();
				}
			}
			else
			{
				if (_areClicksLocked)
				{
					return;
				}
				PointerEventData eventData = new PointerEventData(_eventSystem)
				{
					position = Input.mousePosition
				};
				List<RaycastResult> list = new List<RaycastResult>();
				_raycaster.Raycast(eventData, list);
				List<UIButton> list2 = new List<UIButton>(list.Count);
				foreach (RaycastResult item in list)
				{
					if (item.gameObject.TryGetComponent<UIButton>(out var component))
					{
						if (component.IsMousePosInSpriteArea(item.screenPosition, _camera))
						{
							list2.Add(component);
						}
					}
					else if (list2.Count == 0)
					{
						break;
					}
				}
				if (list2.Count > 0)
				{
					UIButton uIButton = list2[0];
					if (uIButton != _selectedButton)
					{
						if (_selectedButton != null)
						{
							_selectedButton.OnUnhover();
						}
						_cursorController.SetType(ECursorType.Hover);
						uIButton.OnHover();
						_selectedButton = uIButton;
					}
					if (_inputHandler.UISubmitClicked || _inputHandler.LMBClicked)
					{
						uIButton.Click();
						_selectedButton = null;
					}
				}
				else
				{
					_cursorController.SetType(ECursorType.Normal);
					if (_selectedButton != null)
					{
						_selectedButton.OnUnhover();
						_selectedButton = null;
					}
				}
			}
		}

		private void Unhover()
		{
			if (_isHoveredObject)
			{
				_cursorController.SetType(ECursorType.Normal);
				_isHoveredObject = false;
			}
		}

		private void Hover()
		{
			if (!_isHoveredObject)
			{
				_cursorController.SetType(ECursorType.Hover);
				_isHoveredObject = true;
			}
		}

		public void SetBlockerActive(bool isActive)
		{
			_isBlocked = isActive;
		}

		public void LockClicks()
		{
			_areClicksLocked = true;
		}

		public void UnlockClicks()
		{
			_areClicksLocked = false;
		}
	}
}
