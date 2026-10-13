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
				return false;
			}
			set
			{
			}
		}

		public bool IsLocked => false;

		protected abstract void OnFocused();

		protected abstract void OnLostFocus();

		protected abstract void OnTargetedWrongConditions();

		protected abstract void OnTargetedCorrectConditions();

		public void Init(IHUDPresenter hudPresenter, IPauseController pauseController, Func<bool> hardConditions = null, AActionableObjectView linkedActionableObject = null)
		{
		}

		public void SetLockedState(bool isLocked)
		{
		}

		private void OnPauseStateChanged(bool isPaused)
		{
		}

		public void SetOnHoverCheck(Action action)
		{
		}
	}
}
