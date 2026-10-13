using System;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Pause;
using _Code.Infrastructure.Player;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.CustomYarnReading;

namespace _Code.Infrastructure
{
	public sealed class DialogInteractable : AInteractableObject
	{
		[SerializeField]
		private CharacterSOData _character;

		[SerializeField]
		private NodeNameGetter _nodeNames;

		[SerializeField]
		private Camera _camera;

		[SerializeField]
		private CinemachineCamera _cinemachineCamera;

		[SerializeField]
		private ETimeOfDay[] _prohibitedTimeOfDay;

		[SerializeField]
		private bool _isOneTime;

		private IDialogManager _dialogManager;

		private InteractablesSaveData _saveData;

		private int _objectIndex;

		private int _currentNodeIndex;

		private IPlayerService _playerService;

		private InputHandling _inputHandler;

		private IHUDPresenter _hudPresenter;

		private ICursorController _cursorController;

		private IDayNightController _dayNightController;

		private bool _everTalked;

		public override bool HardConditions => true;

		public override bool SoftConditions => true;

		public event Action DialogStarted;

		public event Action DialogEnded;

		public override void Interact()
		{
			if (!_isOneTime || !_everTalked)
			{
				_everTalked = true;
				if (!_prohibitedTimeOfDay.Contains(_dayNightController.CurrentTimeOfDay))
				{
					_hudPresenter.HideHint();
					_dialogManager.DialogEnded += OnDialogEnded;
					_playerService.MakeImmovable();
					_inputHandler.SetIsInUIState(isInUi: true);
					_raycastTarget.SetLockedState(isLocked: true);
					_cinemachineCamera?.gameObject.SetActive(value: true);
					_dialogManager.RunDialog(_character, _nodeNames.GetNodeName(), EDialogOverlayType.None, _camera);
					DialogStarted?.Invoke();
				}
			}
		}

		private void OnDialogEnded(bool isEndedDialog, bool isEndedSubtitle)
		{
			_dialogManager.DialogEnded -= OnDialogEnded;
			_playerService.MakeMovable();
			_inputHandler.SetIsInUIState(isInUi: false);
			_raycastTarget.SetLockedState(isLocked: false);
			_cinemachineCamera?.gameObject.SetActive(value: false);
			DialogEnded?.Invoke();
		}

		public void Init(int index, IHUDPresenter hudPresenter, IPauseController pauseController, IDialogManager dialogManager, InteractablesSaveData saveData, IPlayerService playerService, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController, IDayNightController dayNightController)
		{
			_raycastTarget.Init(hudPresenter, pauseController, () => HardConditions);
			_hudPresenter = hudPresenter;
			_dialogManager = dialogManager;
			_saveData = saveData;
			_playerService = playerService;
			_inputHandler = inputHandlerProvider.InputHandler;
			_cursorController = cursorController;
			_dayNightController = dayNightController;
			_nodeNames.Init(index, saveData);
			_dayNightController.DayChanged -= OnDayChanged;
			_dayNightController.DayChanged += OnDayChanged;
		}

		private void OnDayChanged(int day)
		{
			_nodeNames.ResetDay();
		}
	}
}
