using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using RTLTMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.UI;
using _Code.DialogSystem;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Gameplay;
using _Code.Infrastructure.Player;
using _Code.Infrastructure.Rooms;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.Updatable;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Infrastructure._NINAH__CloseUps.Views.Fridge;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Utils.UI.ImageAnimating;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.CloseUps.Views
{
	public sealed class FridgeCloseUpView : ACloseUpView, IUpdateable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CAnimateDrinkVibration_003Ed__51 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CDrinkBeerAsync_003Ed__49 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public FridgeCloseUpView _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CDrinkCoffeeAsync_003Ed__47 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public FridgeCloseUpView _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CDrinkEnerjekaAsync_003Ed__48 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public FridgeCloseUpView _003C_003E4__this;

			private UniTask.Awaiter _003C_003Eu__1;

			private void MoveNext()
			{
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

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

		[CompilerGenerated]
		private Action m_ShowStarted;

		[CompilerGenerated]
		private Action m_HideEnded;

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

		private bool _canUseItems;

		private IPlayerService _playerService;

		private IDreamController _dreamController;

		private const float MIN_WALKING_COCKROACHE_DELAY = 10f;

		private const float MAX_WALKING_COCKROACHE_DELAY = 30f;

		private const float SHOW_WALKING_COCROACH_ON_OPEN_CHANCE = 0.333f;

		public override IUpdateable[] Updateables => null;

		public event Action ShowStarted
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

		public event Action HideEnded
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

		public override void Init()
		{
		}

		public void InitModules(IHUDPresenter hudPresenter, IDayNightController dayNightController, IConsumablesController consumablesController, IStateObjectController stateObjectController, IGameplayEndingManager gameplayEndingManager, CloseUpSaveData saveData, IDialogManager dialogManager, IRoomDisplayerViewProvider roomDisplayerViewProvider, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, IPlayerService playerService, IDataModelService dataModelService, IDreamController dreamController)
		{
		}

		private void OnResourceCountUpdated(EConsumable item, int count)
		{
		}

		protected override void OnStartShow()
		{
		}

		protected override void OnShown()
		{
		}

		protected override void OnStartHide()
		{
		}

		protected override void OnHidden()
		{
		}

		private void OnPointerExited()
		{
		}

		private void OnPointerEntered(string name, string narrativeDescription, string gameplayDescription, EConsumable consumable)
		{
		}

		private bool OnItemUsed(EConsumable objectType)
		{
			return false;
		}

		[AsyncStateMachine(typeof(_003CDrinkCoffeeAsync_003Ed__47))]
		private UniTaskVoid DrinkCoffeeAsync()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CDrinkEnerjekaAsync_003Ed__48))]
		private UniTaskVoid DrinkEnerjekaAsync()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CDrinkBeerAsync_003Ed__49))]
		private UniTaskVoid DrinkBeerAsync()
		{
			return default;
		}

		private void CheckExtraBeer()
		{
		}

		[AsyncStateMachine(typeof(_003CAnimateDrinkVibration_003Ed__51))]
		private UniTaskVoid AnimateDrinkVibration()
		{
			return default;
		}

		public void PutItem(EConsumable itemType)
		{
		}

		private void RegenerateWalkingCockroachSpawn()
		{
		}

		public override void OnUpdateAction()
		{
		}

		public void Refill()
		{
		}

		private void Update()
		{
		}

		public void ReinitSaveData(CloseUpSaveData saveData)
		{
		}
	}
}
