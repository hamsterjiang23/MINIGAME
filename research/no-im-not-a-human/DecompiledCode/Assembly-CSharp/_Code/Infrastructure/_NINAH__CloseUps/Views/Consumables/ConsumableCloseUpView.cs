using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RTLTMPro;
using UnityEngine;
using UnityEngine.UI;
using _Code.Infrastructure.CloseUps;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Updatable;
using _Code.Menues.HUD;
using _Code.Player;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure._NINAH__CloseUps.Views.Consumables;

public sealed class ConsumableCloseUpView : ACloseUpView
{
	[SerializeField]
	private RTLTextMeshPro _name;

	[SerializeField]
	private RTLTextMeshPro _gameplayDescription;

	[SerializeField]
	private RTLTextMeshPro _narrativeDescription;

	[SerializeField]
	private Button _yesButton;

	[SerializeField]
	private Button _noButton;

	[SerializeField]
	private ConsumableLocalizationsListSOData _localizationsList;

	private EConsumable _selectedConsumable;

	private IConsumablesController _consumablesController;

	private Action _onUseAction;

	private Action _onCloseAction;

	private IDataModelService _dataModelService;

	private WatcherManager _watcherManager;

	protected override bool IsUseFade => false;

	public override IUpdateable[] Updateables { get; }

	public bool IsAnimating { get; private set; }

	private void OnYesClicked()
	{
		if (_consumablesController.TryRemove(_selectedConsumable))
		{
			_onUseAction?.Invoke();
			_onUseAction = null;
		}
		Hide().Forget();
	}

	private void OnNoClicked()
	{
		Hide().Forget();
	}

	public void InitModules(IConsumablesController consumablesController, IHUDPresenter hudPresenter, IInputHandlerProvider inputHandlerProvider, ICursorController cursorController, IDataModelService dataModelService, WatcherManager watcherManager)
	{
		_consumablesController = consumablesController;
		HUDPresenter = hudPresenter;
		InputHandler = inputHandlerProvider.InputHandler;
		CursorController = cursorController;
		_dataModelService = dataModelService;
		_watcherManager = watcherManager;
		_yesButton.onClick.RemoveAllListeners();
		_noButton.onClick.RemoveAllListeners();
		_yesButton.onClick.AddListener(OnYesClicked);
		_noButton.onClick.AddListener(OnNoClicked);
	}

	public void SetOnUseAction(Action onUseAction)
	{
		_onUseAction = onUseAction;
	}

	public void SetupConsumable(EConsumable consumable)
	{
		_selectedConsumable = consumable;
		ConsumableLocalizationSOData consumableLocalizationSOData = _localizationsList.Consumables.FirstOrDefault((ConsumableLocalizationSOData x) => x.Consumable == consumable);
		if (!(consumableLocalizationSOData == null))
		{
			_name.text = consumableLocalizationSOData.Name.GetLocalizedString() + $" (x{_consumablesController.Count(consumable)})";
			if (_dataModelService.MetaPrefsDataHandler.IsEverUsedConsumable(consumable) || consumable == EConsumable.Kombucha)
			{
				_gameplayDescription.text = consumableLocalizationSOData.GameplayDescription.GetLocalizedString();
			}
			else
			{
				_gameplayDescription.text = "???";
			}
			_narrativeDescription.text = consumableLocalizationSOData.NarrativeDescription.GetLocalizedString();
		}
	}

	public override async UniTask Show()
	{
		IsAnimating = true;
		int num = _consumablesController.Count(_selectedConsumable);
		_yesButton.interactable = num > 0;
		await base.Show();
		_watcherManager.EnterState(EWatcherState.ConsumableController);
		await UniTask.Delay(TimeSpan.FromSeconds(0.33000001311302185));
		IsAnimating = false;
	}

	public override async UniTask Hide()
	{
		IsAnimating = true;
		_watcherManager.LeaveState(EWatcherState.ConsumableController);
		await base.Hide();
		_onCloseAction?.Invoke();
		await UniTask.Delay(TimeSpan.FromSeconds(0.33000001311302185));
		IsAnimating = false;
	}

	public void SetOnCloseAction(Action onClose)
	{
		_onCloseAction = onClose;
	}

	private void Update()
	{
		if (!IsAnimating && InputHandler.UIExitTriggered && IsEntered)
		{
			OnNoClicked();
		}
	}
}
