using System;
using System.Collections.Generic;
using System.Linq;
using Code.Utils;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.StateObjects;
using _Code.Menues.HUD;
using _Code.Rooms;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Rooms
{
	public abstract class ARoom : IRoom
	{
		protected IRoomView View;

		protected readonly IDayNightController DayNightController;

		protected readonly IOtherGameSODataProvider OtherGameSoDataProvider;

		protected readonly IDialogManager DialogManager;

		protected readonly IGameEventsManager GameEventsManager;

		protected readonly ICloseUpsController CloseUpsController;

		protected readonly ICursorController CursorController;

		protected readonly INotAHumanSoundService SoundService;

		protected readonly IGameplayEndingManager GameplayEndingManager;

		protected readonly IStateObjectController StateObjectController;

		protected readonly IHUDPresenter HUDPresenter;

		protected readonly IDataModelService DataModelService;

		public const int MAXIMUM_CHARACTERS_IN_ROOM = 10;

		protected readonly List<ECharacterType> CharactersInside = new List<ECharacterType>(10);

		protected readonly List<ECharacterType> AliveCharactersInside = new List<ECharacterType>(10);

		protected readonly List<CharacterRoomObjectView> KilledCharacters = new List<CharacterRoomObjectView>(10);

		private AActionableObjectView _linkedActionableObject;

		private readonly ICharactersManager _charactersManager;

		private ECharacterType _todayWhisperSource;

		public abstract ERoom RoomType { get; }

		public float CameraDistance => View.CameraDistance;

		public bool SuspendedExit { get; protected set; }

		public float DialogCameraDistance => View.DialogCameraDistance;

		public bool HasDialogNow
		{
			get
			{
				if (!View.HasDialogNow)
				{
					return View.IsDead;
				}
				return true;
			}
		}

		public Camera LinkedCamera => View.LinkedCamera;

		public Vector3 LookDirection => new Vector3(0f, _linkedActionableObject.AwaitedPlayerLookAngle, 0f);

		public event Action<ARoom> Entered;

		public event Action<ARoom> Left;

		public event Action DialogStarted;

		public event Action DialogFinished;

		protected ARoom(IDayNightController dayNightController, IOtherGameSODataProvider otherGameSoDataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, ICharactersManager charactersManager, IDataModelService dataModelService, IStateObjectController stateObjectController)
		{
			DayNightController = dayNightController;
			OtherGameSoDataProvider = otherGameSoDataProvider;
			DialogManager = dialogManager;
			GameEventsManager = gameEventsManager;
			CloseUpsController = closeUpsController;
			CursorController = cursorController;
			SoundService = soundService;
			HUDPresenter = hudPresenter;
			GameplayEndingManager = gameplayEndingManager;
			_charactersManager = charactersManager;
			DataModelService = dataModelService;
			StateObjectController = stateObjectController;
		}

		public void Init(ARoomView view)
		{
			View = view;
			View.Init(DialogManager, DayNightController, CloseUpsController, CursorController, SoundService, HUDPresenter, DataModelService);
		}

		public void Enter()
		{
			Entered?.Invoke(this);
			EnterInner();
		}

		protected virtual void EnterInner()
		{
		}

		public void Leave()
		{
			Left?.Invoke(this);
			View.Leave();
		}

		public void StopWhispering(float time)
		{
			View.StopWhispering(time);
		}

		public void AddCharacter(ECharacterType character, bool isAddSilently = false)
		{
			CharactersInside.Add(character);
			AliveCharactersInside.Add(character);
			CharacterRoomObjectView characterRoomObjectView = View.CharacterViews.FirstOrDefault((CharacterRoomObjectView x) => x.Data.CharacterType == character);
			if (characterRoomObjectView == null)
			{
				throw new Exception($"НЕТ ПЕРСА {character} В КОМНАТЕ {RoomType} ({View.CharacterViews.Select((CharacterRoomObjectView x) => x.Data.CharacterType).ElementsToString()})");
			}
			characterRoomObjectView.Activate();
			characterRoomObjectView.DialogStarted -= StartDialog;
			characterRoomObjectView.DialogStarted += StartDialog;
			if (!isAddSilently)
			{
				((DoorTrigger)_linkedActionableObject).SlightlyOpenDoor();
			}
		}

		public void KillCharacter(ECharacterType character)
		{
			CharacterRoomObjectView characterRoomObjectView = View.CharacterViews.FirstOrDefault((CharacterRoomObjectView x) => x.Data.CharacterType == character);
			if (characterRoomObjectView == null)
			{
				throw new Exception($"НЕТ ПЕРСА {character} В КОМНАТЕ {RoomType} ({View.CharacterViews.Select((CharacterRoomObjectView x) => x.Data.CharacterType).ElementsToString()})");
			}
			AliveCharactersInside.Remove(characterRoomObjectView.Data.CharacterType);
			KilledCharacters.Add(characterRoomObjectView);
			characterRoomObjectView.MakeCorpse();
		}

		public void ExileCharacter(ECharacterType character)
		{
			CharacterRoomObjectView characterRoomObjectView = View.CharacterViews.FirstOrDefault((CharacterRoomObjectView x) => x.Data.CharacterType == character);
			if (characterRoomObjectView == null)
			{
				throw new Exception($"НЕТ ПЕРСА {character} ДЛЯ ИЗГНАНИЯ");
			}
			CharactersInside.Remove(character);
			AliveCharactersInside.Remove(character);
			if (_todayWhisperSource == character)
			{
				StopWhispering(0f);
			}
			characterRoomObjectView.GigaDeactivate();
		}

		public void StartWhispering(float time)
		{
			if (AliveCharactersInside.Count > 0)
			{
				ECharacterType characterType = (_todayWhisperSource = AliveCharactersInside.GetRandomElement());
				CharacterSOData character = _charactersManager.GetCharacter(characterType);
				if (!(character.NightRoomSound == null))
				{
					View.StartWhispering(character.NightRoomSound, time);
				}
			}
		}

		public void ClearCorpses()
		{
			foreach (CharacterRoomObjectView killedCharacter in KilledCharacters)
			{
				killedCharacter.Deactivate();
			}
			KilledCharacters.Clear();
		}

		protected string GetDialogName(CharacterSOData character)
		{
			int num = DialogManager.GetCurrentDialogIndexForCharacter(character) + 1;
			return "talk_" + character.CharacterType.ToString().ToLower() + "_" + num;
		}

		private void StartDialog(CharacterRoomObjectView character, bool ignoreIsActiveCondition = false)
		{
			if (!KilledCharacters.Contains(character))
			{
				View.SetDialogCompanion(character);
				string dialogName = GetDialogName(character.Data);
				DialogManager.RunDialog(character.Data, dialogName, EDialogOverlayType.None, LinkedCamera);
				DialogManager.DialogEnded += OnDialogFinished;
				DialogStarted?.Invoke();
			}
		}

		private void OnDialogFinished(bool isEndedDialog, bool isEndedSubtitle)
		{
			DialogManager.DialogEnded -= OnDialogFinished;
			DialogFinished?.Invoke();
		}

		public void SetActionableObject(AActionableObjectView actionableObject)
		{
			_linkedActionableObject = actionableObject;
		}

		public void ShowView()
		{
			View.Enter();
		}

		protected void CallDialogStarted()
		{
			DialogStarted?.Invoke();
			DialogManager.DialogEnded += OnDialogFinished;
		}

		public void ChangeCharacterPose(ECharacterType character, ERoomPeopleState pose)
		{
			View.ChangeCharacterPose(character, pose);
		}

		public void EnableObjects()
		{
			View.EnableObjects();
		}

		public void DisableObjects()
		{
			View.DisableObjects();
		}

		public void StartLeavingRoom()
		{
			View.StartLeavingRoom();
		}

		public void StopLeavingRoom()
		{
			View.StopLeavingRoom();
		}

		public void BreakDoor()
		{
			_linkedActionableObject.Disable();
			View.BreakDoor();
		}

		public void RepairDoor()
		{
			_linkedActionableObject.Enable();
			View.RepairDoor();
		}

		public Dictionary<string, int> GetUIButtonsStates()
		{
			return View.GetUIBUttonsStates();
		}
	}
}
