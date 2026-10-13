using System;
using System.Runtime.CompilerServices;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Data;
using _Code.Infrastructure.Endings.View;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.Windows;
using _Code.Infrastructure._NINAH__Effects;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingManager : ASavableClass<GameplayEndingManagerSaveData>, IGameplayEndingManager, IDisposable
	{
		private GameplayEndingManagerSaveData _saveData;

		private readonly AGameplayEnding[] _endings;

		private readonly GameplayEndingStayAlive _stayAlive;

		private readonly GameplayEndingStayAliveWithThem _stayAliveWithThem;

		private readonly GameplayEndingStayAliveAlone _stayAliveAlone;

		private readonly GameplayEndingBaby _baby;

		private readonly GameplayEndingKiller _killer;

		private readonly GameplayEndingMushroom _mushroom;

		private readonly GameplayEndingCultists _cultists;

		private readonly GameplayEndingDeath _death;

		private readonly GameplayEndingFema _fema;

		private readonly GameplayEndingVigilante _vigilante;

		[CompilerGenerated]
		private Action<EEnding> m_EndingTriggered;

		private IDataModelService _dataModelService;

		public AGameplayEnding ActualEnding => null;

		public event Action<EEnding> EndingTriggered
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

		public AGameplayEnding GetEnding(EEnding ending)
		{
			return null;
		}

		public GameplayEndingManager(IDayNightController dayNightController, ICharactersManager charactersManager, IStateObjectController stateObjectController, IEndingShower endingShower, IWindowsManager windowsManager, IEndingSODataProvider endingSoDataProvider, IDialogManager dialogManager, IDataModelService dataModelService, IEffectsController effectsController, IConsumablesController consumablesController, INotAHumanSoundService soundService)
		{
		}

		private void OnUnlockedKillerEnding()
		{
		}

		private void OnUnlockedMushroomEnding()
		{
		}

		private void OnStateChanged(EStateObjectType state, int index)
		{
		}

		private void OnUnlockedDeathEnding()
		{
		}

		private void OnCultistsSaved()
		{
		}

		private void OnPlayerRevealedByVigilante()
		{
		}

		public void InitGetFemaCallsFunc(Func<int> func)
		{
		}

		private void OnReachedLastState(EStateObjectType state)
		{
		}

		private void TryCompleteProphetSignChecksCondition()
		{
		}

		private void OnCharacterLetIn(ECharacterType character, bool isFromSave)
		{
		}

		private void OnCultistsBegun()
		{
		}

		private bool OnHasCompletedMushroomCheck()
		{
			return false;
		}

		private void OnDialogEndingTriggered()
		{
		}

		private void OnTimeOfDayChanged(ETimeOfDay timeOfDay)
		{
		}

		public bool TryNailUpWindowForBasement()
		{
			return false;
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}
	}
}
