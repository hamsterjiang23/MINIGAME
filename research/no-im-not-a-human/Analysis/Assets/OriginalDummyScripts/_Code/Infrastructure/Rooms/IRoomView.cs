using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Menues.HUD;
using _Code.Rooms;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Rooms
{
	public interface IRoomView
	{
		CharacterRoomObjectView[] CharacterViews { get; }

		AudioSource WhispersAudioSource { get; }

		bool HasDialogNow { get; }

		bool IsDead { get; }

		Camera LinkedCamera { get; }

		float CameraDistance { get; }

		float DialogCameraDistance { get; }

		Vector3 LookDirection { get; }

		UniTaskVoid StopWhispering(float time);

		void StartWhispering(AudioClip sound, float time);

		void SetDialogCompanion(CharacterRoomObjectView character);

		void SetOnOpenAction(Action action);

		void Enter();

		void Leave();

		void Init(IDialogManager dialogManager, IDayNightController dayNightController, ICloseUpsController closeUpsController, ICursorController cursorController, INotAHumanSoundService soundService, IHUDPresenter hudPresenter, IDataModelService dataModelService);

		UniTaskVoid Blink(Action action);

		void ChangeCharacterPose(ECharacterType character, ERoomPeopleState pose);

		void EnableObjects();

		void DisableObjects();

		void BreakDoor();

		void RepairDoor();

		Dictionary<string, int> GetUIBUttonsStates();

		void SetUIButtonState(Dictionary<string, int> buttonStates);

		void StartLeavingRoom();

		void StopLeavingRoom();
	}
}
