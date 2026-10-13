using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Rooms;

namespace _Code.Rooms
{
	public abstract class ARoomObjectView<T> : MonoBehaviour where T : Enum
	{
		[SerializeField]
		protected UIButton _button;

		[SerializeField]
		protected Image _image;

		public Func<ARoom> GetRoom;

		protected IDayNightController DayNightController;

		protected IDialogManager DialogManager;

		protected ICloseUpsController CloseUpsController;

		private bool _isInited;

		protected abstract RoomObjectState<T>[] States { get; }

		protected abstract T StartState { get; }

		public T SelectedState { get; private set; }

		public void Init(IDayNightController dayNightController, IDialogManager dialogManager, ICloseUpsController closeUpsController)
		{
			DayNightController = dayNightController;
			DialogManager = dialogManager;
			CloseUpsController = closeUpsController;
		}

		protected virtual void Awake()
		{
			if (!_isInited)
			{
				_isInited = true;
				SelectState(StartState);
			}
		}

		public virtual void Activate()
		{
			gameObject.SetActive(value: true);
		}

		public virtual void Deactivate()
		{
			gameObject.SetActive(value: false);
		}

		public void SetAction(Action action)
		{
			if (action != null)
			{
				_button.SetAction(action);
			}
		}

		public void SelectState(T state)
		{
			_isInited = true;
			SelectedState = state;
			RoomObjectState<T> roomObjectState = States.FirstOrDefault((RoomObjectState<T> x) => x.Name.ToString().ToLower().Equals(state.ToString().ToLower()));
			if (roomObjectState != null)
			{
				_image.sprite = roomObjectState.Sprite;
				transform.localPosition = roomObjectState.PositionShift;
				transform.localScale = roomObjectState.ScaleShift;
			}
		}

		protected void SetClickability(bool isClickable)
		{
			_image.raycastTarget = isClickable;
		}

		private bool IsOutlineConditionMet()
		{
			return false;
		}
	}
}
