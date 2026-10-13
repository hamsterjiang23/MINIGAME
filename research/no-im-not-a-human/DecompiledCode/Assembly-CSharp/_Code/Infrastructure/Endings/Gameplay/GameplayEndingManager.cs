using System;
using System.Linq;
using Cysharp.Threading.Tasks;
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

namespace _Code.Infrastructure.Endings.Gameplay;

public sealed class GameplayEndingManager : ASavableClass<GameplayEndingManagerSaveData>, IGameplayEndingManager, IDisposable
{
	private GameplayEndingManagerSaveData _saveData = new GameplayEndingManagerSaveData();

	private readonly AGameplayEnding[] _endings;

	private readonly GameplayEndingStayAlive _stayAlive = new GameplayEndingStayAlive();

	private readonly GameplayEndingStayAliveWithThem _stayAliveWithThem = new GameplayEndingStayAliveWithThem();

	private readonly GameplayEndingStayAliveAlone _stayAliveAlone = new GameplayEndingStayAliveAlone();

	private readonly GameplayEndingBaby _baby = new GameplayEndingBaby();

	private readonly GameplayEndingKiller _killer = new GameplayEndingKiller();

	private readonly GameplayEndingMushroom _mushroom = new GameplayEndingMushroom();

	private readonly GameplayEndingCultists _cultists = new GameplayEndingCultists();

	private readonly GameplayEndingDeath _death = new GameplayEndingDeath();

	private readonly GameplayEndingFema _fema = new GameplayEndingFema();

	private readonly GameplayEndingVigilante _vigilante = new GameplayEndingVigilante();

	private IDataModelService _dataModelService;

	public AGameplayEnding ActualEnding => (from x in _endings
		where x.AreConditionsMet
		orderby x.Priority descending
		select x).FirstOrDefault();

	public event Action<EEnding> EndingTriggered;

	public AGameplayEnding GetEnding(EEnding ending)
	{
		return _endings.FirstOrDefault((AGameplayEnding x) => x.Ending == ending);
	}

	public GameplayEndingManager(IDayNightController dayNightController, ICharactersManager charactersManager, IStateObjectController stateObjectController, IEndingShower endingShower, IWindowsManager windowsManager, IEndingSODataProvider endingSoDataProvider, IDialogManager dialogManager, IDataModelService dataModelService, IEffectsController effectsController, IConsumablesController consumablesController, INotAHumanSoundService soundService)
	{
		_endings = new AGameplayEnding[10] { _stayAlive, _stayAliveWithThem, _stayAliveAlone, _baby, _killer, _mushroom, _cultists, _death, _fema, _vigilante };
		AGameplayEnding[] endings = _endings;
		foreach (AGameplayEnding ending in endings)
		{
			ending.PreInit(endingShower);
			ending.Triggered += () =>
			{
				EndingTriggered?.Invoke(ending.Ending);
			};
		}
		_stayAlive.InitModules(dayNightController, charactersManager, stateObjectController);
		_stayAliveWithThem.InitModules(dayNightController, charactersManager, stateObjectController);
		_stayAliveAlone.InitModules(dayNightController, charactersManager, stateObjectController);
		_baby.InitModules(dayNightController, windowsManager, stateObjectController, _saveData, effectsController, soundService);
		_killer.InitModules(dayNightController, charactersManager, endingSoDataProvider);
		_mushroom.InitModules(_saveData, stateObjectController, dayNightController);
		_cultists.InitModules(_saveData, stateObjectController, dialogManager);
		_fema.InitModules(dialogManager, consumablesController, endingSoDataProvider, dayNightController);
		_death.InitModules(_saveData);
		dayNightController.Changed -= OnTimeOfDayChanged;
		dayNightController.Changed += OnTimeOfDayChanged;
		dialogManager.EndingTriggered -= OnDialogEndingTriggered;
		dialogManager.EndingTriggered += OnDialogEndingTriggered;
		dialogManager.HasCompletedMushroomCheck -= OnHasCompletedMushroomCheck;
		dialogManager.HasCompletedMushroomCheck += OnHasCompletedMushroomCheck;
		dialogManager.CultistsBegun -= OnCultistsBegun;
		dialogManager.CultistsBegun += OnCultistsBegun;
		dialogManager.ProphetDontCheckConditionMet -= TryCompleteProphetSignChecksCondition;
		dialogManager.ProphetDontCheckConditionMet += TryCompleteProphetSignChecksCondition;
		dialogManager.PlayerRevealedByVigilante -= OnPlayerRevealedByVigilante;
		dialogManager.PlayerRevealedByVigilante += OnPlayerRevealedByVigilante;
		dialogManager.CultistsSaved -= OnCultistsSaved;
		dialogManager.CultistsSaved += OnCultistsSaved;
		dialogManager.UnlockedDeathEnding -= OnUnlockedDeathEnding;
		dialogManager.UnlockedDeathEnding += OnUnlockedDeathEnding;
		dialogManager.UnlockedMushroomEnding -= OnUnlockedMushroomEnding;
		dialogManager.UnlockedMushroomEnding += OnUnlockedMushroomEnding;
		dialogManager.UnlockedKillerEnding -= OnUnlockedKillerEnding;
		dialogManager.UnlockedKillerEnding += OnUnlockedKillerEnding;
		charactersManager.CharacterAdded -= OnCharacterLetIn;
		charactersManager.CharacterAdded += OnCharacterLetIn;
		stateObjectController.ReachedLastState -= OnReachedLastState;
		stateObjectController.ReachedLastState += OnReachedLastState;
		stateObjectController.StateChanged -= OnStateChanged;
		stateObjectController.StateChanged += OnStateChanged;
		_dataModelService = dataModelService;
		dialogManager.InitializeProphetCondition(_death.CheckCondition);
		dialogManager.InitializeMushroomeaterCondition(_mushroom.CheckCondition);
		dialogManager.InitializePriestCondition(_cultists.CheckCondition);
		LinkSaveDataKey(this, _saveData, dataModelService.GameSaveDataHandler);
	}

	private void OnUnlockedKillerEnding()
	{
		_killer.UnlockCartoon();
	}

	private void OnUnlockedMushroomEnding()
	{
		_mushroom.UnlockCartoon();
	}

	private void OnStateChanged(EStateObjectType state, int index)
	{
		if (state == EStateObjectType.HatchEnter && index == 1)
		{
			_mushroom.OpenHatch();
		}
	}

	private void OnUnlockedDeathEnding()
	{
		_death.EnableVideoEnd();
	}

	private void OnCultistsSaved()
	{
		_cultists.SaveCultists();
	}

	private void OnPlayerRevealedByVigilante()
	{
		_vigilante.Reveal();
	}

	public void InitGetFemaCallsFunc(Func<int> func)
	{
		_fema.InitFunc(func);
	}

	private void OnReachedLastState(EStateObjectType state)
	{
		if (state == EStateObjectType.Ground)
		{
			_death.CheckCondition(2);
		}
	}

	private void TryCompleteProphetSignChecksCondition()
	{
		_death.CompleteCondition(1);
	}

	private void OnCharacterLetIn(ECharacterType character, bool isFromSave)
	{
		if (character == ECharacterType.Ballerina && !isFromSave)
		{
			_death.CompleteCondition(0);
		}
	}

	private void OnCultistsBegun()
	{
		_cultists.BeginCultists();
	}

	private bool OnHasCompletedMushroomCheck()
	{
		if (_saveData.HasEatenMushroom && _saveData.HasFoundMushroomApple)
		{
			return _saveData.HasWatchedMushroomClock;
		}
		return false;
	}

	private void OnDialogEndingTriggered()
	{
		ActualEnding.ShowClip(0.5f).Forget();
	}

	private void OnTimeOfDayChanged(ETimeOfDay timeOfDay)
	{
		ActualEnding?.Trigger();
	}

	public bool TryNailUpWindowForBasement()
	{
		if (_stayAlive.AreConditionsMet)
		{
			return _stayAlive.NailUpWindow();
		}
		if (_stayAliveWithThem.AreConditionsMet)
		{
			return _stayAliveWithThem.NailUpWindow();
		}
		if (_stayAliveAlone.AreConditionsMet)
		{
			return _stayAliveAlone.NailUpWindow();
		}
		return false;
	}

	protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
	{
		_saveData = saver.LoadKey(this);
		_baby.ReinitSaveData(_saveData);
		_mushroom.ReinitSaveData(_saveData);
		_cultists.ReinitSaveData(_saveData);
		_death.ReinitSaveData(_saveData);
		LinkSaveDataKey(this, _saveData, _dataModelService.GameSaveDataHandler);
	}
}
