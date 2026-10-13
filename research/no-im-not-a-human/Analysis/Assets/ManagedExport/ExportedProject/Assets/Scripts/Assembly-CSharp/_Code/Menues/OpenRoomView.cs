using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RTLTMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using _Code.Infrastructure.ControlsViewer;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Menues
{
	public sealed class OpenRoomView : MonoBehaviour
	{
		[SerializeField]
		private Camera _camera;

		[SerializeField]
		private RTLTextMeshPro _subjectText;

		[SerializeField]
		private RTLTextMeshPro _actionText;

		[SerializeField]
		private AnimatedImage _animatedImage;

		[SerializeField]
		private Transform _crosshair;

		[SerializeField]
		private Transform _box;

		[SerializeField]
		private SerializedDictionary<ERaycastHintIcon, AnimationData> _icons;

		[SerializeField]
		private Image _fade;

		[SerializeField]
		private CanvasGroup _canvasGroup;

		[SerializeField]
		private ControlView _controlView;

		private LocalizedString _roomNameLocalizationKey;

		private ERaycastHintIcon _icon;

		private Transform _currentTarget;

		private bool _isShowing;

		private void Update()
		{
			_crosshair.eulerAngles = new Vector3(0f, 0f, Mathf.Sin(Time.time * 2f) * 15f);
			Vector3 vector = _camera.ScreenToWorldPoint(Input.mousePosition);
			vector.z = 0f;
			_box.position = Vector3.Lerp(_camera.WorldToScreenPoint(_currentTarget.position) + new Vector3(Mathf.Sin(Time.time * 2f) * 5f, Mathf.Cos(Time.time) * 5f), _crosshair.position, 0.5f);
		}

		public async UniTask Show()
		{
			_isShowing = true;
			base.gameObject.SetActive(value: true);
			await (Tween)base.transform.DOScale(new Vector3(1f, 1f, 1f), 0.2f).SetEase(Ease.InQuad);
		}

		public async UniTask Hide()
		{
			_isShowing = false;
			await (Tween)base.transform.DOScale(new Vector3(0f, 0f, 0f), 0.2f).SetEase(Ease.OutQuad);
			if (!_isShowing)
			{
				base.gameObject.SetActive(value: false);
			}
		}

		public OpenRoomView SetSubjectName(string subject)
		{
			_subjectText.text = subject;
			return this;
		}

		public OpenRoomView SetActionName(string action)
		{
			_actionText.text = action;
			return this;
		}

		public OpenRoomView SetTarget(Transform target)
		{
			_currentTarget = target;
			return this;
		}

		public OpenRoomView SetIcon(ERaycastHintIcon icon)
		{
			AnimationData animationData = _icons[icon];
			bool flag = animationData.Frames.Length != 0;
			_animatedImage.gameObject.SetActive(flag);
			if (flag)
			{
				_animatedImage.SetData(animationData);
			}
			return this;
		}

		public void SetFadedState(bool isFaded)
		{
			_fade.color = (isFaded ? new Color(0f, 0f, 0f, 0.7f) : new Color(0f, 0f, 0f, 0f));
			_canvasGroup.alpha = (isFaded ? 0.7f : 1f);
		}

		public void UpdateIcon(EInputDevice device, EGaypadType gaypadType)
		{
			if (device == EInputDevice.Gaypad)
			{
				_controlView.transform.localScale = Vector3.one * 0.8f;
				_controlView.InitGamepadKey("Button South", EControl.Leave, gaypadType);
			}
			else
			{
				_controlView.transform.localScale = Vector3.one * 0.5f;
				_controlView.InitKey("Space", EControl.Leave);
			}
		}
	}
}
