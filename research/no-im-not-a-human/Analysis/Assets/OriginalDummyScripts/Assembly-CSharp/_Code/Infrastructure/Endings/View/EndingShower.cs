using System.Linq;
using Cysharp.Threading.Tasks;
using Zenject;
using _Code.Characters;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure.Scenes;
using _Code.Infrastructure._NINAH__Endings.View;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.CustomYarnReading;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.Endings.View;

public sealed class EndingShower : IEndingShower, IInitializable
{
	private readonly EndingView _view;

	private readonly EndingViewSOData[] _endings;

	private InputHandling _inputHandling;

	private IHUDPresenter _hudPresenter;

	private readonly IDataModelService _dataModelService;

	private readonly INotAHumanSoundService _soundService;

	private readonly ICharactersManager _charactersManager;

	private readonly IConsumablesController _consumablesController;

	private EEnding _currentEnding;

	public EndingShower(IEndingViewProvider endingViewProvider, IEndingDataProvider endingDataProvider, INotAHumanSoundService soundService, ICustomYarnReaderProvider customYarnReaderProvider, IInputHandlerProvider inputHandlerProvider, IHUDPresenter hudPresenter, ICutscenesManager cutscenesManager, IDataModelService dataModelService, ICharactersManager charactersManager, IConsumablesController consumablesController, WatcherManager watcherManager)
	{
		_view = endingViewProvider.EndingView;
		_endings = endingDataProvider.EndingsData;
		_inputHandling = inputHandlerProvider.InputHandler;
		_hudPresenter = hudPresenter;
		_dataModelService = dataModelService;
		_soundService = soundService;
		_charactersManager = charactersManager;
		_consumablesController = consumablesController;
		_view.Init(soundService, customYarnReaderProvider.CustomYarnReader, _inputHandling, _dataModelService, watcherManager);
		cutscenesManager.CutsceneEnded -= OnCutsceneEnded;
		cutscenesManager.CutsceneEnded += OnCutsceneEnded;
		_view.EndingUnlocked -= OnEndingUnlocked;
		_view.EndingUnlocked += OnEndingUnlocked;
	}

	public void Initialize()
	{
	}

	private void OnEndingUnlocked(bool isGameOver)
	{
		if (!_charactersManager.EverRefused())
		{
			SteamAchievements.Unlock("LET_ALL_IN");
		}
		if (!isGameOver && !_consumablesController.EverUsedConsumableThisRun(EConsumable.Kombucha))
		{
			SteamAchievements.Unlock("NO_SAVE");
		}
	}

	public async UniTaskVoid ShowEnding(EEnding endingType)
	{
		_currentEnding = endingType;
		await _hudPresenter.FadeIn(0.5f);
		_hudPresenter.FadeOut(0.5f).Forget();
		EndingViewSOData endingViewSOData = _endings.FirstOrDefault((EndingViewSOData x) => x.EndingType == endingType);
		if (!(endingViewSOData == null))
		{
			_inputHandling.SetIsInUIState(isInUi: true);
			_view.Ended += OnEndingEnded;
			_view.ShowEnding(endingViewSOData, isUnlock: true);
			_soundService.DisableDayTheme(1f);
			_soundService.DisableNightTheme(1f);
			_dataModelService.GameSaveDataHandler.ClearSavedData();
			_dataModelService.GameSaveDataHandler.ResetEvents();
		}
	}

	private void OnEndingEnded()
	{
		_view.Ended -= OnEndingEnded;
		_hudPresenter.FadeIn(0f);
		if (!_dataModelService.MetaPrefsDataHandler.IsGameOverEnding(_currentEnding))
		{
			ScenesChanger.OpenScene(5).Forget();
		}
		else
		{
			ScenesChanger.OpenScene(0).Forget();
		}
	}

	private void OnCutsceneEnded(ECutscene cutscene)
	{
		if (cutscene == ECutscene.Fek3d)
		{
			ShowEnding(EEnding.KilledBySuper).Forget();
		}
	}
}
