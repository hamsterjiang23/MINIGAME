using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using UnityEngine.Localization;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure.Windows;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.EditorWindows;
using _Scripts.Raycast;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.ActionableObjects
{
	[TabsNames(new string[] { "GameObjects", "Float settings", "Other settings", "Drift", "Wrong open" })]
	public abstract class AActionableObjectView : MonoBehaviour, IActionableObjectView, IUpdateable
	{
		[TabIndex(0)]
		[SerializeField]
		protected Transform _lookAtPos;

		[TabIndex(0)]
		[SerializeField]
		private Transform _standingPos;

		[TabIndex(0)]
		[SerializeField]
		private ARaycastTarget _raycastTarget;

		[TabIndex(0)]
		[SerializeField]
		private float _awaitedPlayerLookAngle;

		[TabIndex(1)]
		[SerializeField]
		protected float _transitionTime;

		[TabIndex(1)]
		[SerializeField]
		private float _fov;

		[TabIndex(1)]
		[SerializeField]
		[Range(0.1f, 5f)]
		private float _fovChangeSlowerCoefIn = 1f;

		[TabIndex(1)]
		[SerializeField]
		[Range(0.1f, 5f)]
		private float _fovChangeSlowerCoefOut = 1f;

		[TabIndex(1)]
		[SerializeField]
		private float _moveToStandingPosSpeed = 0.1f;

		[TabIndex(2)]
		[SerializeField]
		private bool _isWaitingActionBeforeChangingFOV = true;

		[TabIndex(2)]
		[SerializeField]
		private ETimeOfDay[] ProhibitedToUseDaytime;

		[TabIndex(2)]
		[SerializeField]
		private int[] ProhibitedDays;

		[TabIndex(2)]
		[SerializeField]
		private bool _isAwaitingMoveToStandingPos = true;

		[TabIndex(3)]
		[SerializeField]
		private bool _canBeOpenedByEKey;

		[TabIndex(3)]
		[SerializeField]
		[Range(0f, 1f)]
		private float _eKeyChance;

		[TabIndex(4)]
		[SerializeField]
		[SearchableEnum]
		protected ESoundSource _soundSource;

		[TabIndex(4)]
		[SerializeField]
		[SearchableEnum]
		private ESound _cantOpenSound;

		[TabIndex(4)]
		[SerializeField]
		private LocalizedString _cantOpenCauseOfDay;

		[TabIndex(4)]
		[SerializeField]
		private LocalizedString _cantOpenCauseOfTimeOfDay;

		private bool _canLeave = true;

		protected IHUDPresenter HUDPresenter;

		protected IDayNightController DayNightController;

		protected IRoomsManager RoomsManager;

		protected IGameEventsManager GameEventsManager;

		protected ICloseUpsController CloseUpsController;

		protected IPlayerService PlayerService;

		protected ICursorController CursorController;

		protected IWindowsManager WindowsManager;

		protected IPauseController PauseController;

		protected INotAHumanSoundService SoundService;

		protected InputHandling InputHandler;

		protected bool IsLooking;

		private bool _isAnimating;

		private bool _isEKeyActivated;

		private bool _isLocked;

		protected IDialogManager _dialogManager;

		private ESound[] _tryOpenSounds = new ESound[4]
		{
			ESound.House_Doors_Try_Open_01,
			ESound.House_Doors_Try_Open_02,
			ESound.House_Doors_Try_Open_03,
			ESound.House_Doors_Try_Open_04
		};

		protected abstract Func<UniTask> ExtraActionIn { get; }

		protected abstract Func<UniTask> ExtraActionOut { get; }

		protected abstract Func<UniTask> ExtraActionInE { get; }

		protected abstract Func<UniTask> ExtraActionOutE { get; }

		protected abstract bool CanLeave { get; }

		public bool CanShowHint => !PauseController.IsPaused;

		public float AwaitedPlayerLookAngle => _awaitedPlayerLookAngle;

		public void InitModules(IDialogManager dialogManager)
		{
			_dialogManager = dialogManager;
		}

		public virtual void Init(IHUDPresenter hudPresenter, IDayNightController dayNightController, IRoomsManager roomsManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, IPlayerService playerService, ICursorController cursorController, IPauseController pauseController, INotAHumanSoundService soundService, IInputHandlerProvider inputHandlerProvider, WatcherManager watcherManager)
		{
			HUDPresenter = hudPresenter;
			DayNightController = dayNightController;
			RoomsManager = roomsManager;
			GameEventsManager = gameEventsManager;
			CloseUpsController = closeUpsController;
			PlayerService = playerService;
			CursorController = cursorController;
			PauseController = pauseController;
			SoundService = soundService;
			InputHandler = inputHandlerProvider.InputHandler;
			_raycastTarget.Init(hudPresenter, pauseController, null, this);
			_raycastTarget.SetOnHoverCheck(() =>
			{
				HUDPresenter.SetHintFadedState(ProhibitedToUseDaytime.Contains(DayNightController.CurrentTimeOfDay) || ProhibitedDays.Contains(DayNightController.Day));
			});
		}

		public void TryLeave()
		{
			if (IsLooking)
			{
				StopLooking().Forget();
			}
		}

		public void SetLockedState(bool isLocked)
		{
			_isLocked = isLocked;
			_raycastTarget.SetLockedState(isLocked);
		}

		private async UniTaskVoid StartLooking()
		{
			if (_isAnimating || _isLocked || PauseController.IsPaused)
			{
				return;
			}
			ExtraActionBeforeIn();
			_raycastTarget.SetLockedState(isLocked: true);
			HUDPresenter.SetupAndShowControlsView(EControlsList.InRoom);
			HUDPresenter.SetControlsAvailability(EControl.ExitRoom, CanLeave);
			HUDPresenter.HideHint();
			_isAnimating = true;
			PlayerService.MakeImmovable();
			if (_standingPos != null)
			{
				if (_isAwaitingMoveToStandingPos)
				{
					await PlayerService.MoveXZ(_standingPos.position, _moveToStandingPosSpeed);
				}
				else
				{
					PlayerService.MoveXZ(_standingPos.position, _moveToStandingPosSpeed).Forget();
				}
			}
			PlayerService.LookAtWithZoom(_lookAtPos.transform, _fov, _transitionTime / _fovChangeSlowerCoefIn);
			if (_isWaitingActionBeforeChangingFOV)
			{
				await (_isEKeyActivated ? ExtraActionInE() : ExtraActionIn());
			}
			else if (_isEKeyActivated)
			{
				ExtraActionInE().Forget();
			}
			else
			{
				ExtraActionIn().Forget();
			}
			if (!_isWaitingActionBeforeChangingFOV)
			{
				await UniTask.Delay(TimeSpan.FromSeconds(_transitionTime / _fovChangeSlowerCoefOut));
			}
			_isAnimating = false;
			IsLooking = true;
			InputHandler.SetIsInUIState(isInUi: true);
		}

		private async UniTaskVoid StopLooking()
		{
			await UniTask.DelayFrame(1);
			if (!_isAnimating && CanLeave && !PauseController.IsPaused)
			{
				ExtraActionBeforeOut();
				HUDPresenter.HideControlsView();
				_isAnimating = true;
				if (_isWaitingActionBeforeChangingFOV)
				{
					await (_isEKeyActivated ? ExtraActionOutE() : ExtraActionOut());
				}
				else if (_isEKeyActivated)
				{
					ExtraActionOutE().Forget();
				}
				else
				{
					ExtraActionOut().Forget();
				}
				PlayerService.ResetFov(_transitionTime / _fovChangeSlowerCoefOut);
				await UniTask.Delay(TimeSpan.FromSeconds(_transitionTime / _fovChangeSlowerCoefOut));
				PlayerService.MakeMovable();
				_isAnimating = false;
				IsLooking = false;
				InputHandler.SetIsInUIState(isInUi: false);
				_raycastTarget.SetLockedState(isLocked: false);
			}
		}

		protected virtual void ExtraActionBeforeIn()
		{
		}

		protected virtual void ExtraActionBeforeOut()
		{
		}

		public void OnUpdateAction()
		{
		}

		public void Update()
		{
			KeyAvailabilityCheck();
			ARaycastTarget raycastTarget = _raycastTarget;
			if ((object)raycastTarget != null && raycastTarget.IsTargeted && ((InputHandler.InteractTriggered && !IsLooking) || (InputHandler.UIExitTriggered && IsLooking)))
			{
				Act();
			}
		}

		private void KeyAvailabilityCheck()
		{
			if (IsLooking && CanLeave != _canLeave)
			{
				_canLeave = CanLeave;
				HUDPresenter.SetControlsAvailability(EControl.ExitRoom, _canLeave);
			}
		}

		private void Act()
		{
			if (!_isAnimating && (!_raycastTarget.IsLocked || IsLooking))
			{
				bool flag = false;
				if (!flag && ProhibitedToUseDaytime.Contains(DayNightController.CurrentTimeOfDay) && !IsLooking)
				{
					TryToOpenInWrongTimeOfDay();
				}
				else if (!flag && ProhibitedDays.Contains(DayNightController.Day) && !IsLooking)
				{
					TryToOpenInWrongDay();
				}
				else if (IsLooking)
				{
					StopLooking().Forget();
				}
				else
				{
					StartLooking().Forget();
				}
			}
		}

		private void TryToOpenInWrongTimeOfDay()
		{
			SoundService.PlayOneShot(_soundSource, _tryOpenSounds.GetRandomElementWithoutRepeating(), new SoundSettingRandomPitch(0.05f));
			SoundService.FadeVolume(_soundSource, 0.5f, 0f);
			_dialogManager.ShowSubtitlePopup(_cantOpenCauseOfTimeOfDay.GetLocalizedString());
		}

		private void TryToOpenInWrongDay()
		{
			SoundService.PlayOneShot(_soundSource, _tryOpenSounds.GetRandomElementWithoutRepeating(), new SoundSettingRandomPitch(0.05f));
			SoundService.FadeVolume(_soundSource, 0.5f, 0f);
			_dialogManager.ShowSubtitlePopup(_cantOpenCauseOfDay.GetLocalizedString());
		}

		private void OnDrawGizmos()
		{
			if (_standingPos != null)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawLine(_standingPos.position + Vector3.down, _standingPos.position + Vector3.up);
			}
			if (_lookAtPos != null)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawSphere(_lookAtPos.position, 0.02f);
			}
			Gizmos.color = Color.green;
			Vector3 vector = new Vector3(Mathf.Cos((90f - _awaitedPlayerLookAngle) * ((float)Math.PI / 180f)), 0f, Mathf.Sin((90f - _awaitedPlayerLookAngle) * ((float)Math.PI / 180f))) * 0.2f;
			Gizmos.DrawRay(_standingPos.position, vector);
			Gizmos.DrawSphere(_standingPos.position + vector, 0.02f);
		}

		public void Disable()
		{
			_raycastTarget.SetLockedState(isLocked: true);
		}

		public void Enable()
		{
			_raycastTarget.SetLockedState(isLocked: false);
		}
	}
}
