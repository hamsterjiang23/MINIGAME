using System;
using System.Runtime.CompilerServices;
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

		[CompilerGenerated]
		private Action m_DialogStarted;

		[CompilerGenerated]
		private Action m_DialogEnded;

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

		public override bool HardConditions => false;

		public override bool SoftConditions => false;

		public event Action DialogStarted
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action DialogEnded
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public override void Interact()
		{
		}

		private void OnDialogEnded(bool isEndedDialog, bool isEndedSubtitle)
		{
		}

		public void Init(int index, IHUDPresenter hudPresenter, IPauseController pauseController, IDialogManager dialogManager, InteractablesSaveData saveData, IPlayerService playerService, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController, IDayNightController dayNightController)
		{
		}

		private void OnDayChanged(int day)
		{
		}
	}
}
