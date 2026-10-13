using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.ActionableObjects;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Rooms;
using _Code.Utils.Extensions;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Rooms;

public sealed class RoomsManager : ASavableClass<RoomsSaveData>, IRoomsManager, ITickable, IDisposable
{
	private RoomsSaveData _saveData = new RoomsSaveData();

	private readonly IRoomsViewProvider _roomsViewProvider;

	private readonly IDayNightController _dayNightController;

	private readonly ICharactersManager _charactersManager;

	private readonly IGameEventsManager _gameEventsManager;

	private readonly RoomDisplayer _roomDisplayer;

	private readonly IPlayerService _playerService;

	private readonly IHUDPresenter _hudPresenter;

	private readonly IDataModelService _dataModelService;

	private readonly CharacterSOData[] _charactersList;

	private readonly IGameplayEndingManager _gameplayEndingManager;

	public ARoom Kitchen { get; }

	public ARoom Office { get; }

	public ARoom Bedroom { get; }

	public ARoom BigRoom { get; }

	public ARoom Bathroom { get; }

	public ARoom Pantry { get; }

	public ARoom Entrance { get; }

	public bool IsInRoom { get; private set; }

	private ARoom[] AllRooms => new ARoom[7] { Kitchen, Office, Bedroom, BigRoom, Bathroom, Pantry, Entrance };

	public event Func<ECharacterType, bool> IsCharacterAlive;

	public RoomsManager(IRoomsViewProvider roomsViewProvider, IDayNightController dayNightController, IOtherGameSODataProvider otherGameSODataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICharactersManager charactersManager, ICloseUpsController closeUpsController, ICursorController cursorController, IRoomDisplayerViewProvider roomDisplayerViewProvider, IPlayerService playerService, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, IInputHandlerProvider inputHandlerProvider, IDataModelService dataModelService, ICharactersSODataProvider charactersSODataProvider, IStateObjectController stateObjectController)
	{
		_roomsViewProvider = roomsViewProvider;
		_dayNightController = dayNightController;
		_charactersManager = charactersManager;
		_playerService = playerService;
		_gameEventsManager = gameEventsManager;
		_hudPresenter = hudPresenter;
		_dataModelService = dataModelService;
		_roomDisplayer = roomDisplayerViewProvider.RoomDisplayer;
		_roomDisplayer.InitModules(cursorController, inputHandlerProvider);
		_charactersList = charactersSODataProvider.CharactersData;
		_gameplayEndingManager = gameplayEndingManager;
		Kitchen = new KitchenRoom(dayNightController, otherGameSODataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController);
		Office = new OfficeRoom(dayNightController, otherGameSODataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController);
		Bedroom = new Bedroom(dayNightController, otherGameSODataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController);
		BigRoom = new BigRoom(dayNightController, otherGameSODataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController);
		Bathroom = new Bathroom(dayNightController, otherGameSODataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController);
		Pantry = new PantryRoom(dayNightController, otherGameSODataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController);
		Entrance = new EntranceRoom(dayNightController, otherGameSODataProvider, dialogManager, gameEventsManager, closeUpsController, cursorController, soundService, gameplayEndingManager, hudPresenter, charactersManager, dataModelService, stateObjectController);
		dialogManager.PoseChanged -= ChangeCharacterPose;
		dialogManager.PoseChanged += ChangeCharacterPose;
		dialogManager.PoseChangedTomorrow -= ChangeCharacterPoseTomorrow;
		dialogManager.PoseChangedTomorrow += ChangeCharacterPoseTomorrow;
		dialogManager.SetFridgeActivity -= OnSetFridgeActivity;
		dialogManager.SetFridgeActivity += OnSetFridgeActivity;
		dialogManager.RoomKilled -= OnRoomKilled;
		dialogManager.RoomKilled += OnRoomKilled;
		_dataModelService.GameSaveDataHandler.SavedAll -= OnSave;
		_dataModelService.GameSaveDataHandler.SavedAll += OnSave;
		LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
		Init();
	}

	private void OnRoomKilled(ERoom obj)
	{
		if (obj == ERoom.BigRoom)
		{
			(_roomsViewProvider.BigRoom as BigRoomView).RevealBlood();
			_saveData.DayOfBloodClean = _dayNightController.Day + 1;
		}
	}

	private void OnSetFridgeActivity(bool isActive)
	{
		((KitchenRoom)Kitchen).SetFridgeActivity(isActive);
	}

	private void Init()
	{
		Kitchen.Init(_roomsViewProvider.Kitchen);
		Office.Init(_roomsViewProvider.Office);
		Bedroom.Init(_roomsViewProvider.Bedroom);
		BigRoom.Init(_roomsViewProvider.BigRoom);
		Bathroom.Init(_roomsViewProvider.Bathroom);
		Pantry.Init(_roomsViewProvider.Pantry);
		Entrance.Init(_roomsViewProvider.Entrance);
		((Bedroom)Bedroom).InitModules(_dataModelService, _charactersManager, _saveData);
		((EntranceRoom)Entrance).InitModules(_dataModelService, _charactersManager);
		ARoom[] allRooms = AllRooms;
		foreach (ARoom obj in allRooms)
		{
			obj.Entered -= OnRoomEntered;
			obj.Entered += OnRoomEntered;
			obj.Left -= OnRoomLeft;
			obj.Left += OnRoomLeft;
			obj.DialogStarted -= OnDialogStarted;
			obj.DialogStarted += OnDialogStarted;
			obj.DialogFinished -= OnDialogFinished;
			obj.DialogFinished += OnDialogFinished;
		}
		_dayNightController.WatchedTV -= WatchTV;
		_dayNightController.Changed -= OnDayNightChanged;
		_dayNightController.PosesChanged -= OnDelayedPosesChanged;
		_dayNightController.BodyEaterAppeared -= OnBodyEaterAppeared;
		_dayNightController.BodyEaterDisappeared -= OnBodyEaterDisappeared;
		_charactersManager.GetRoom -= GetRoom;
		_gameEventsManager.CharacterCame -= OnCharacterCame;
		_gameEventsManager.CharacterSoonCame -= OnCharacterCameSoon;
		_gameEventsManager.SpecifiedEventCompleted -= OnSpecifiedEventCompleted;
		((Bedroom)Bedroom).BabyActivated -= OnSexyExploded;
		((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).FoundBaby -= OnActivatedBaby;
		_dayNightController.WatchedTV += WatchTV;
		_dayNightController.Changed += OnDayNightChanged;
		_dayNightController.PosesChanged += OnDelayedPosesChanged;
		_dayNightController.BodyEaterAppeared += OnBodyEaterAppeared;
		_dayNightController.BodyEaterDisappeared += OnBodyEaterDisappeared;
		_charactersManager.GetRoom += GetRoom;
		_gameEventsManager.CharacterCame += OnCharacterCame;
		_gameEventsManager.CharacterSoonCame += OnCharacterCameSoon;
		_gameEventsManager.SpecifiedEventCompleted += OnSpecifiedEventCompleted;
		((Bedroom)Bedroom).BabyActivated += OnSexyExploded;
		((GameplayEndingBaby)_gameplayEndingManager.GetEnding(EEnding.Baby)).FoundBaby += OnActivatedBaby;
		if (_dayNightController.CurrentTimeOfDay == ETimeOfDay.Night)
		{
			((Bedroom)Bedroom).SetTVActive(isActive: false);
		}
	}

	private void OnBodyEaterAppeared()
	{
		Office.BreakDoor();
		BigRoom.BreakDoor();
		Bathroom.BreakDoor();
		Kitchen.BreakDoor();
		Pantry.BreakDoor();
	}

	private void OnBodyEaterDisappeared()
	{
		Office.RepairDoor();
		BigRoom.RepairDoor();
		Bathroom.RepairDoor();
		Kitchen.RepairDoor();
		Pantry.RepairDoor();
	}

	private void OnDialogStarted()
	{
		_roomDisplayer.SetBlockerActive(isActive: true);
	}

	private void OnDialogFinished()
	{
		_roomDisplayer.SetBlockerActive(isActive: false);
	}

	private void OnSexyExploded()
	{
		_charactersManager.Kill(ECharacterType.Sexy);
	}

	private void OnActivatedBaby(int index)
	{
		switch (index)
		{
		case 0:
			((OfficeRoom)Office).ActivateBaby();
			break;
		case 1:
			((Bathroom)Bathroom).ActivateBaby();
			break;
		}
	}

	private void OnSpecifiedEventCompleted(string eventName)
	{
		if (_charactersManager.IsCharacterAliveInside(ECharacterType.Sexy) && eventName.Contains("belly"))
		{
			((Bedroom)Bedroom).GrowUpBelly();
		}
	}

	private void OnRoomEntered(ARoom room)
	{
		IsInRoom = true;
		_roomDisplayer.SetData(room.LinkedCamera, room.CameraDistance, room.DialogCameraDistance);
		OnRoomEnteredAsync(room).Forget();
	}

	private async UniTask OnRoomEnteredAsync(ARoom room)
	{
		if (room.RoomType == ERoom.Entrance)
		{
			room.ShowView();
			return;
		}
		UniTask taskToAwaitResult;
		if (Mathf.Abs(room.LookDirection.NormalizeAngle(40f).y - _playerService.LookDirection.NormalizeAngle(40f).y) < 40f)
		{
			taskToAwaitResult = _roomDisplayer.OpenFadeLocal();
			room.ShowView();
		}
		else
		{
			taskToAwaitResult = _roomDisplayer.OpenFadeGlobal();
			await UniTask.Delay(TimeSpan.FromSeconds(0.10000000149011612));
			room.ShowView();
		}
		await taskToAwaitResult;
	}

	private void OnRoomLeft(ARoom room)
	{
		IsInRoom = false;
		OnRoomLeftAsync().Forget();
	}

	private async UniTask OnRoomLeftAsync()
	{
		await _roomDisplayer.CloseFade();
	}

	private void OnCharacterCame(CharacterDingDongEvent dingDingEvent)
	{
		CharacterSOData character = _charactersList.FirstOrDefault((CharacterSOData x) => x.CharacterType == dingDingEvent.Character);
		((EntranceRoom)Entrance).ShowFullSizeCharacter(character, dingDingEvent.Condition.ConditionName);
	}

	private void OnCharacterCameSoon()
	{
		((EntranceRoom)Entrance).CharacerWillCumSoon();
	}

	private void OnDayNightChanged(ETimeOfDay currentTimeOfDay)
	{
		ClearCorpses();
		ReinitWhispers(currentTimeOfDay);
		((Bedroom)Bedroom).SetTVActive(currentTimeOfDay == ETimeOfDay.Day);
		if (currentTimeOfDay == ETimeOfDay.Night)
		{
			((Bedroom)Bedroom).DisableTvIfExtra();
		}
		if (_dayNightController.Day >= _saveData.DayOfBloodClean)
		{
			(_roomsViewProvider.BigRoom as BigRoomView).CleanBlood();
		}
	}

	public void WatchTV()
	{
		((Bedroom)Bedroom).WatchMorningTV();
	}

	public void ClearCorpses()
	{
		ARoom[] allRooms = AllRooms;
		for (int i = 0; i < allRooms.Length; i++)
		{
			allRooms[i].ClearCorpses();
		}
	}

	public void ReinitWhispers(ETimeOfDay currentTimeOfDay)
	{
		ARoom[] allRooms = AllRooms;
		foreach (ARoom aRoom in allRooms)
		{
			if (currentTimeOfDay == ETimeOfDay.Night)
			{
				aRoom.StartWhispering(0f);
			}
			else
			{
				aRoom.StopWhispering(0f);
			}
		}
	}

	public ARoom GetRoom(ERoom roomType)
	{
		return AllRooms.FirstOrDefault((ARoom x) => x.RoomType == roomType);
	}

	public void LinkActionableToRoom(ERoom linkedRoom, AActionableObjectView actionableObject)
	{
		GetRoom(linkedRoom).SetActionableObject(actionableObject);
	}

	public void Tick()
	{
		((EntranceRoom)Entrance).Tick();
	}

	public void ChangeCharacterPose(ECharacterType character, ERoomPeopleState pose)
	{
		ERoom characterRoom = _charactersManager.GetCharacterRoom(character);
		GetRoom(characterRoom).ChangeCharacterPose(character, pose);
	}

	private void ChangeCharacterPoseTomorrow(ECharacterType character, ERoomPeopleState pose)
	{
		_dayNightController.AddChangePosTomorrow(character, pose);
	}

	private void OnDelayedPosesChanged(List<ChangePoseData> poses)
	{
		foreach (ChangePoseData pose in poses)
		{
			ChangeCharacterPose(pose.Character, pose.State);
		}
	}

	private void OnSave(bool isReserve)
	{
	}

	protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
	{
		_saveData = saver.LoadKey(this);
		(_roomsViewProvider.Bathroom as BathroomView).LoadSink();
		(_roomsViewProvider.Bedroom as BedroomView).OnLoad(_saveData);
		(_roomsViewProvider.Bathroom as BathroomView).OnLoad(_saveData);
		(_roomsViewProvider.Office as OfficeView).OnLoad(_saveData);
		((Bedroom)Bedroom).OnLoad(_saveData).Forget();
		((EntranceRoom)Entrance).OnLoad().Forget();
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}
}
