using System;
using RoboRyanTron.SearchableEnum;
using UnityEngine;
using _Code.Events;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.Windows;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Rooms;
using _Scripts.Services.Sound;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingBaby : AGameplayEnding
	{
		private IWindowsManager _windowsManager;

		private IDayNightController _dayNightController;

		private IStateObjectController _stateObjectController;

		private IInteractablesManager _interactablesManager;

		private IEffectsController _effectsController;

		private INotAHumanSoundService _soundService;

		private GameplayEndingManagerSaveData _saveData;

		[SerializeField]
		[SearchableEnum]
		private ESound[] _babyFoundSounds;

		public override int Priority => 2;

		public override EEnding Ending => EEnding.Baby;

		public override bool AreConditionsMet
		{
			get
			{
				if (_saveData.HasFoundBedroomBaby && _saveData.HasFoundOfficeBaby && _saveData.HasFoundBathroomBaby)
				{
					return _dayNightController.CurrentTimeOfDay == ETimeOfDay.Night;
				}
				return false;
			}
		}

		public bool IsBathroomBabyFound => _saveData.HasFoundBathroomBaby;

		public bool IsOfficeBabyFound => _saveData.HasFoundOfficeBaby;

		public bool IsBedroomBabyFound => _saveData.HasFoundBedroomBaby;

		public event Action<int> FoundBaby;

		protected override void TriggerInner()
		{
			_effectsController.EnableFog(1.5f, 9f, new Color(0.6f, 0.6f, 0.6f));
			_dayNightController.SetDayForEnding(EEnding.Baby);
			_stateObjectController.SetState(EStateObjectType.PeepholeEndingTrigger, 1);
		}

		public void InitModules(IDayNightController dayNightController, IWindowsManager windowsManager, IStateObjectController stateObjectController, GameplayEndingManagerSaveData saveData, IEffectsController effectsController, INotAHumanSoundService soundService)
		{
			_dayNightController = dayNightController;
			_windowsManager = windowsManager;
			_stateObjectController = stateObjectController;
			_saveData = saveData;
			_effectsController = effectsController;
			_soundService = soundService;
		}

		public void FindBaby(ERoom room)
		{
			ESound clip = UnityEngine.Random.Range(0, 4) switch
			{
				0 => ESound.Char_event_child_found_01, 
				1 => ESound.Char_event_child_found_02, 
				2 => ESound.Char_event_child_found_03, 
				_ => ESound.Char_event_child_found_04, 
			};
			_soundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, clip);
			switch (room)
			{
			case ERoom.Bedroom:
				_saveData.HasFoundBedroomBaby = true;
				FoundBaby?.Invoke(0);
				break;
			case ERoom.Office:
				_saveData.HasFoundOfficeBaby = true;
				FoundBaby?.Invoke(1);
				break;
			case ERoom.Bathroom:
				_saveData.HasFoundBathroomBaby = true;
				FoundBaby?.Invoke(2);
				break;
			}
		}

		public void ReinitSaveData(GameplayEndingManagerSaveData saveData)
		{
			_saveData = saveData;
		}
	}
}
