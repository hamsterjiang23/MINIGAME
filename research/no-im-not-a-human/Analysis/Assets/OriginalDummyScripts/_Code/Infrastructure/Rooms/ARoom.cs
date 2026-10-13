using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

		protected readonly List<ECharacterType> CharactersInside;

		protected readonly List<ECharacterType> AliveCharactersInside;

		protected readonly List<CharacterRoomObjectView> KilledCharacters;

		private AActionableObjectView _linkedActionableObject;

		private readonly ICharactersManager _charactersManager;

		private ECharacterType _todayWhisperSource;

		[CompilerGenerated]
		private Action<ARoom> m_Entered;

		[CompilerGenerated]
		private Action<ARoom> m_Left;

		[CompilerGenerated]
		private Action m_DialogStarted;

		[CompilerGenerated]
		private Action m_DialogFinished;

		public abstract ERoom RoomType { get; }

		public float CameraDistance => 0f;

		public bool SuspendedExit { get; protected set; }

		public float DialogCameraDistance => 0f;

		public bool HasDialogNow => false;

		public Camera LinkedCamera => null;

		public Vector3 LookDirection => default;

		public event Action<ARoom> Entered
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

		public event Action<ARoom> Left
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

		public event Action DialogFinished
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

		protected ARoom(IDayNightController dayNightController, IOtherGameSODataProvider otherGameSoDataProvider, IDialogManager dialogManager, IGameEventsManager gameEventsManager, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IGameplayEndingManager gameplayEndingManager, IHUDPresenter hudPresenter, ICharactersManager charactersManager, IDataModelService dataModelService, IStateObjectController stateObjectController)
		{
		}

		public void Init(ARoomView view)
		{
		}

		public void Enter()
		{
		}

		protected virtual void EnterInner()
		{
		}

		public void Leave()
		{
		}

		public void StopWhispering(float time)
		{
		}

		public void AddCharacter(ECharacterType character, bool isAddSilently = false)
		{
		}

		public void KillCharacter(ECharacterType character)
		{
		}

		public void ExileCharacter(ECharacterType character)
		{
		}

		public void StartWhispering(float time)
		{
		}

		public void ClearCorpses()
		{
		}

		protected string GetDialogName(CharacterSOData character)
		{
			return null;
		}

		private void StartDialog(CharacterRoomObjectView character, bool ignoreIsActiveCondition = false)
		{
		}

		private void OnDialogFinished(bool isEndedDialog, bool isEndedSubtitle)
		{
		}

		public void SetActionableObject(AActionableObjectView actionableObject)
		{
		}

		public void ShowView()
		{
		}

		protected void CallDialogStarted()
		{
		}

		public void ChangeCharacterPose(ECharacterType character, ERoomPeopleState pose)
		{
		}

		public void EnableObjects()
		{
		}

		public void DisableObjects()
		{
		}

		public void StartLeavingRoom()
		{
		}

		public void StopLeavingRoom()
		{
		}

		public void BreakDoor()
		{
		}

		public void RepairDoor()
		{
		}

		public Dictionary<string, int> GetUIButtonsStates()
		{
			return null;
		}
	}
}
