using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RTLTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;
using _Code.Characters.DialogSystem;
using _Code.DialogSystem;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.ControlsViewer;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.Sound.Settings;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Infrastructure._NINAH__CloseUps.Views.Fridge;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Menues.HUD;
using _Code.Menues.HUD.Animations;
using _Code.Player;
using _Code.Utils;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound;

namespace _Code.Infrastructure.CloseUps.Views
{
	public sealed class FridgeCloseUpView : ACloseUpView, IUpdateable
	{
		[SerializeField]
		private RTLTextMeshPro _nameText;

		[SerializeField]
		private RTLTextMeshPro _narrativeDescriptionText;

		[SerializeField]
		private RTLTextMeshPro _gameplayDescriptionText;

		[SerializeField]
		private FridgeItemController[] _itemControllers;

		[SerializeField]
		private AnimatedImage _walkingCockroach;

		[SerializeField]
		private LocalizedString _baseDescription;

		[SerializeField]
		private GraphicRaycaster _raycaster;

		[SerializeField]
		private EventSystem _eventSystem;

		[SerializeField]
		private Cockroach _cockroach;

		private IHUDPresenter _hudPresenter;

		private IDayNightController _dayNightController;

		private IConsumablesController _consumablesController;

		private IStateObjectController _stateObjectController;

		private IGameplayEndingManager _gameplayEndingManager;

		private IDialogManager _dialogManager;

		private float _lastWalkingCockroachTime;

		private float _walkingCockroachDelay;

		private CloseUpSaveData _saveData;

		private IRoomDisplayerViewProvider _roomDisplayerViewProvider;

		private ICursorController _cursorController;

		private InputHandling _inputHandler;

		private FridgeItemView _selectedItem;

		private IDataModelService _dataModelService;

		private bool _canUseItems = true;

		private IPlayerService _playerService;

		private IDreamController _dreamController;

		private const float MIN_WALKING_COCKROACHE_DELAY = 10f;

		private const float MAX_WALKING_COCKROACHE_DELAY = 30f;

		private const float SHOW_WALKING_COCROACH_ON_OPEN_CHANCE = 0.333f;

		public override IUpdateable[] Updateables => new IUpdateable[1] { this };

		public event Action ShowStarted;

		public event Action HideEnded;

		public override void Init()
		{
			base.Init();
			FridgeItemController[] itemControllers = _itemControllers;
			foreach (FridgeItemController fridgeItemController in itemControllers)
			{
				fridgeItemController.Init(_consumablesController.Count(fridgeItemController.ItemType));
				fridgeItemController.Used -= OnItemUsed;
				fridgeItemController.Used += OnItemUsed;
			}
			_consumablesController.UpdatedCount -= OnResourceCountUpdated;
			_consumablesController.UpdatedCount += OnResourceCountUpdated;
		}

		public void InitModules(IHUDPresenter hudPresenter, IDayNightController dayNightController, IConsumablesController consumablesController, IStateObjectController stateObjectController, IGameplayEndingManager gameplayEndingManager, CloseUpSaveData saveData, IDialogManager dialogManager, IRoomDisplayerViewProvider roomDisplayerViewProvider, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, IPlayerService playerService, IDataModelService dataModelService, IDreamController dreamController)
		{
			_hudPresenter = hudPresenter;
			_dayNightController = dayNightController;
			_consumablesController = consumablesController;
			_stateObjectController = stateObjectController;
			_gameplayEndingManager = gameplayEndingManager;
			_dialogManager = dialogManager;
			_saveData = saveData;
			_roomDisplayerViewProvider = roomDisplayerViewProvider;
			_cursorController = cursorController;
			_inputHandler = inputHandlerProvider.InputHandler;
			_playerService = playerService;
			_dataModelService = dataModelService;
			_dreamController = dreamController;
		}

		private void OnResourceCountUpdated(EConsumable item, int count)
		{
			FridgeItemController fridgeItemController = _itemControllers.FirstOrDefault((FridgeItemController x) => x.ItemType == item);
			if (!(fridgeItemController == null))
			{
				fridgeItemController.Init(count);
				if (item == EConsumable.Bobeer)
				{
					CheckExtraBeer();
				}
			}
		}

		protected override void OnStartShow()
		{
			ShowStarted?.Invoke();
			SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Fridge_open);
			SoundService.Play(ESoundSource.s1_GameScene_2DSFXSourceExtra, ESound.Fridge_loop, new SoundSettingCycling());
			SoundService.FadeVolume(ESoundSource.s1_GameScene_2DSFXSourceExtra, 0.3f, 0.5f);
		}

		protected override void OnShown()
		{
			if (UnityEngine.Random.Range(0f, 1f) < 0.333f)
			{
				_walkingCockroach.StartAnimationOrSetFirstFrame();
			}
			RegenerateWalkingCockroachSpawn();
			_hudPresenter.SetupAndShowControlsView(EControlsList.InFridge);
			_roomDisplayerViewProvider.RoomDisplayer.LockClicks();
			_cursorController.SetType(ECursorType.Normal);
			Vibrator.VibrateForTime(0.2f, 0.1f, 0f).Forget();
		}

		protected override void OnStartHide()
		{
			SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Fridge_close);
			SoundService.FadeOut(ESoundSource.s1_GameScene_2DSFXSourceExtra, 0.5f);
		}

		protected override void OnHidden()
		{
			Vibrator.VibrateForTime(0.2f, 0.1f, 0f).Forget();
			_hudPresenter.SetupAndShowControlsView(EControlsList.InRoom);
			_roomDisplayerViewProvider.RoomDisplayer.UnlockClicks();
			HideEnded?.Invoke();
		}

		private void OnPointerExited()
		{
			_narrativeDescriptionText.text = "";
			_gameplayDescriptionText.text = "";
			_nameText.text = "";
		}

		private void OnPointerEntered(string name, string narrativeDescription, string gameplayDescription, EConsumable consumable)
		{
			_nameText.text = name;
			_narrativeDescriptionText.text = narrativeDescription;
			if (_dataModelService.MetaPrefsDataHandler.IsEverUsedConsumable(consumable))
			{
				_gameplayDescriptionText.text = gameplayDescription;
			}
			else
			{
				_gameplayDescriptionText.text = "???";
			}
		}

		private bool OnItemUsed(EConsumable objectType)
		{
			bool flag = false;
			switch (objectType)
			{
			case EConsumable.Bobeer:
				if (_dayNightController.DayActions == 0)
				{
					_dialogManager.ShowSubtitlePopup(EInfoMessageType.NoMoreBeer);
					break;
				}
				AnimateDrinkVibration().Forget();
				DrinkBeerAsync().Forget();
				flag = true;
				break;
			case EConsumable.Coffee:
				AnimateDrinkVibration().Forget();
				DrinkCoffeeAsync().Forget();
				_playerService.Signs.DrinkCaffeine();
				flag = true;
				break;
			case EConsumable.Enerjeka:
				AnimateDrinkVibration().Forget();
				DrinkEnerjekaAsync().Forget();
				_playerService.Signs.DrinkCaffeine();
				flag = true;
				break;
			case EConsumable.Mushroom:
				_hudPresenter.PlayAnimation(EHUDAnimation.EatMushroom);
				((GameplayEndingMushroom)_gameplayEndingManager.GetEnding(EEnding.Mushroom)).EatMushroom();
				flag = true;
				SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Consum_mushroom_02);
				_dreamController.WantToShowDream(EDream.Mushroom);
				break;
			case EConsumable.Cockroach:
				_cockroach.Kill();
				break;
			}
			if (flag)
			{
				_consumablesController.TryRemove(objectType);
			}
			return flag;
		}

		private async UniTaskVoid DrinkCoffeeAsync()
		{
			CanLeave = false;
			_canUseItems = false;
			SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Consum_beer);
			await _hudPresenter.PlayAnimation(EHUDAnimation.DrinkBeer);
			_dayNightController.AddEnergySlot();
			_canUseItems = true;
			CanLeave = true;
		}

		private async UniTaskVoid DrinkEnerjekaAsync()
		{
			CanLeave = false;
			_canUseItems = false;
			SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Consum_beer);
			await _hudPresenter.PlayAnimation(EHUDAnimation.DrinkBeer);
			_dayNightController.AddEnergy();
			_canUseItems = true;
			CanLeave = true;
		}

		private async UniTaskVoid DrinkBeerAsync()
		{
			CanLeave = false;
			_canUseItems = false;
			SoundService.PlayOneShot(ESoundSource.s1_GameScene_2DSFXSource, ESound.Consum_beer);
			await _hudPresenter.PlayAnimation(EHUDAnimation.DrinkBeer);
			_dayNightController.ActAll();
			_dialogManager.ShowSubtitlePopup(EInfoMessageType.Fridge);
			_saveData.DrunkBeerToday = true;
			_canUseItems = true;
			CanLeave = true;
		}

		private void CheckExtraBeer()
		{
			if (_consumablesController.Count(EConsumable.Bobeer) == 0)
			{
				_saveData.ExtraBeerUsed++;
				_consumablesController.Add(EConsumable.Bobeer, 8);
				if (_saveData.ExtraBeerUsed == 1)
				{
					_stateObjectController.SetState(EStateObjectType.Beer2, 0);
				}
				if (_saveData.ExtraBeerUsed == 2)
				{
					_stateObjectController.SetState(EStateObjectType.Beer1, 0);
				}
			}
		}

		private async UniTaskVoid AnimateDrinkVibration()
		{
			await UniTask.Delay(TimeSpan.FromSeconds(0.44999998807907104));
			Vibrator.VibrateForTime(0.1f, 0f, 0.3f).Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(0.30000001192092896));
			Vibrator.VibrateForTime(0.2f, 0.2f, 0f).Forget();
			await UniTask.Delay(TimeSpan.FromSeconds(0.3499999940395355));
			Vibrator.VibrateForTime(0.2f, 0.2f, 0f).Forget();
		}

		public void PutItem(EConsumable itemType)
		{
			FridgeItemController fridgeItemController = _itemControllers.FirstOrDefault((FridgeItemController x) => x.ItemType == itemType);
			if (!(fridgeItemController == null))
			{
				fridgeItemController.PutItem();
			}
		}

		private void RegenerateWalkingCockroachSpawn()
		{
			_lastWalkingCockroachTime = Time.time;
			_walkingCockroachDelay = UnityEngine.Random.Range(10f, 30f);
		}

		public override void OnUpdateAction()
		{
			base.OnUpdateAction();
			if (Time.time > _lastWalkingCockroachTime + _walkingCockroachDelay)
			{
				RegenerateWalkingCockroachSpawn();
				_walkingCockroach.StartAnimationOrSetFirstFrame();
			}
		}

		public void Refill()
		{
			FridgeItemController[] itemControllers = _itemControllers;
			foreach (FridgeItemController fridgeItemController in itemControllers)
			{
				fridgeItemController.Init(_consumablesController.Count(fridgeItemController.ItemType));
			}
		}

		private void Update()
		{
			PointerEventData eventData = new PointerEventData(_eventSystem)
			{
				position = Input.mousePosition
			};
			List<RaycastResult> list = new List<RaycastResult>();
			_raycaster.Raycast(eventData, list);
			List<FridgeItemView> list2 = new List<FridgeItemView>(list.Count);
			foreach (RaycastResult item in list)
			{
				if (item.gameObject.TryGetComponent<FridgeItemView>(out var component))
				{
					list2.Add(component);
				}
				else if (list2.Count == 0)
				{
					break;
				}
			}
			if (list2.Count > 0)
			{
				FridgeItemView fridgeItemView = list2[0];
				if (!fridgeItemView)
				{
					return;
				}
				if (fridgeItemView != _selectedItem)
				{
					if ((bool)_selectedItem)
					{
						_selectedItem?.OnUnhover();
					}
					if (list2[0].GotTexts != null)
					{
						(string, string, string) tuple = list2[0].GotTexts();
						OnPointerEntered(tuple.Item1, tuple.Item2, tuple.Item3, list2[0].ItemType);
					}
					_cursorController.SetType(ECursorType.Hover);
					fridgeItemView.OnHover();
					_selectedItem = fridgeItemView;
				}
				if ((_inputHandler.UISubmitClicked || _inputHandler.LMBClicked) && _canUseItems)
				{
					fridgeItemView.Use();
					_selectedItem = null;
				}
			}
			else
			{
				OnPointerExited();
				_cursorController.SetType(ECursorType.Normal);
				if (_selectedItem != null)
				{
					_selectedItem.OnUnhover();
					_selectedItem = null;
				}
			}
		}

		public void ReinitSaveData(CloseUpSaveData saveData)
		{
			_saveData = saveData;
		}
	}
}
