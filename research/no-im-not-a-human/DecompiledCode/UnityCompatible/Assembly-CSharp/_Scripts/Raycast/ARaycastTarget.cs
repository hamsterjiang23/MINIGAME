using System;
using UnityEngine;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.Pause;
using _Code.Menues.HUD;

namespace _Scripts.Raycast
{
	public abstract class ARaycastTarget : MonoBehaviour
	{
		protected IHUDPresenter HUDPresenter;

		protected IPauseController PauseController;

		protected AActionableObjectView LinkedActionableObject;

		protected Func<bool> HardConditions;

		private bool _isTargeted;

		protected int LockedCount;

		private Action _onHoverCheck;

		public bool IsTargeted
		{
			get
			{
				return _isTargeted;
			}
			set
			{
				if (value)
				{
					if (!HardConditions())
					{
						OnTargetedWrongConditions();
					}
					else
					{
						OnTargetedCorrectConditions();
					}
				}
				if (value)
				{
					OnFocused();
					_onHoverCheck?.Invoke();
				}
				else
				{
					OnLostFocus();
				}
				_isTargeted = value;
			}
		}

		public bool IsLocked => LockedCount >= 1;

		protected abstract void OnFocused();

		protected abstract void OnLostFocus();

		protected abstract void OnTargetedWrongConditions();

		protected abstract void OnTargetedCorrectConditions();

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, Func<bool> hardConditions = null, AActionableObjectView linkedActionableObject = null)
		{
			HUDPresenter = hudPresenter;
			LinkedActionableObject = linkedActionableObject;
			PauseController = pauseController;
			if (hardConditions == null)
			{
				hardConditions = () => true;
			}
			HardConditions = hardConditions;
			pauseController.PauseStateChanged -= OnPauseStateChanged;
			pauseController.PauseStateChanged += OnPauseStateChanged;
		}

		public void SetLockedState(bool isLocked)
		{
			LockedCount += (isLocked ? 1 : (-1));
			if (LockedCount < 0)
			{
				LockedCount = 0;
			}
			if (LockedCount > 1 && IsTargeted && HardConditions())
			{
				OnLostFocus();
			}
			if (LockedCount == 0 && IsTargeted && HardConditions())
			{
				OnFocused();
			}
		}

		private void OnPauseStateChanged(bool isPaused)
		{
			if (_isTargeted)
			{
				if (isPaused)
				{
					OnLostFocus();
				}
				else
				{
					OnFocused();
				}
			}
		}

		public void SetOnHoverCheck(Action action)
		{
			_onHoverCheck = action;
		}
	}
}
