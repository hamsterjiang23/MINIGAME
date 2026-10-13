using System;
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
		}

		protected virtual void Awake()
		{
		}

		public virtual void Activate()
		{
		}

		public virtual void Deactivate()
		{
		}

		public void SetAction(Action action)
		{
		}

		public void SelectState(T state)
		{
		}

		protected void SetClickability(bool isClickable)
		{
		}

		private bool IsOutlineConditionMet()
		{
			return false;
		}
	}
}
