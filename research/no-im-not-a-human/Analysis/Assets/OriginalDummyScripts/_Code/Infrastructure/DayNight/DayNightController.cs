using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.Characters;
using _Code.DialogSystem;
using _Code.Events;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.Endings;
using _Code.Infrastructure.Endings.Data;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure.Windows;
using _Code.Infrastructure._NINAH__Cat;
using _Code.Infrastructure._NINAH__CommonView;
using _Code.Infrastructure._NINAH__DayNight;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Infrastructure._NINAH__Effects;
using _Code.Infrastructure._NINAH__Rooms;
using _Code.Menues.HUD;
using _Code.Player;
using _Code.Rooms;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.DayNight
{
	public sealed class DayNightController : ASavableClass<DayNightSaveData>, IDayNightController, IDisposable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CChangeDaytimeVisual_003Ed__77 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public DayNightController _003C_003E4__this;

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
		private struct _003CInitDevUtils_003Ed__67 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

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
		private struct _003CSetDayForEnding_003Ed__68 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public DayNightController _003C_003E4__this;

			public EEnding ending;

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
		private struct _003CWatchMorningTV_003Ed__81 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public DayNightController _003C_003E4__this;

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

		private DayNightSaveData _saveData;

		private readonly IGameEventsManager _gameEventsManager;

		private readonly ICharactersManager _charactersManager;

		private readonly IHUDPresenter _hudPresenter;

		private readonly IDayNightControllerViewProvider _viewProvider;

		private readonly IDialogManager _dialogManager;

		private readonly IWindowsViewProvider _windowsViewProvider;

		private readonly INotAHumanSoundService _soundService;

		private readonly IStateObjectController _stateObjectController;

		private readonly DayNightControllerSOData _data;

		[CompilerGenerated]
		private Action m_WatchedTV;

		[CompilerGenerated]
		private Action<ETimeOfDay> m_Changed;

		[CompilerGenerated]
		private Action<List<ChangePoseData>> m_PosesChanged;

		[CompilerGenerated]
		private Action<int> m_DayChanged;

		[CompilerGenerated]
		private Action m_BodyEaterAppeared;

		[CompilerGenerated]
		private Action m_BodyEaterDisappeared;

		[CompilerGenerated]
		private Action m_WentToBed;

		[CompilerGenerated]
		private Action m_WokeUp;

		private bool _wasSomebodyMurdered;

		private IDataModelService _dataModelService;

		private readonly InputHandling _inputHandler;

		private readonly ICursorController _cursorController;

		private readonly IConsumablesController _consumablesController;

		private readonly ICommonViewProvider _commonViewProvider;

		private readonly IDreamController _dreamController;

		private readonly IEffectsController _effectsController;

		private readonly EndingsSOData _endingSoData;

		private readonly ICatController _catController;

		public int MaxDayActions => 0;

		public int DayActions => 0;

		public int Day => 0;

		public bool IsEndingDay => false;

		public ETimeOfDay CurrentTimeOfDay => ETimeOfDay.Day;

		public float LastChange => 0f;

		public bool CanLeaveRooms { get; private set; }

		public event Action WatchedTV
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

		public event Action<ETimeOfDay> Changed
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

		public event Action<List<ChangePoseData>> PosesChanged
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

		public event Action<int> DayChanged
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

		public event Action BodyEaterAppeared
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

		public event Action BodyEaterDisappeared
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

		public event Action WentToBed
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

		public event Action WokeUp
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

		public DayNightController(IDayNightControllerSODataProvider dataProvider, IGameEventsManager gameEventsManager, ICharactersManager charactersManager, IHUDPresenter hudPresenter, IDayNightControllerViewProvider viewProvider, IDialogManager dialogManager, IWindowsViewProvider windowsViewProvider, INotAHumanSoundService soundService, IDataModelService dataModelService, IStateObjectController stateObjectController, ICursorController cursorController, IInputHandlerProvider inputHandlerProvider, IConsumablesController consumablesController, ICommonViewProvider commonViewProvider, IDreamController dreamController, IEffectsController effectsController, IEndingSODataProvider endingSoDataProvider, ICatController catController)
		{
		}

		private void Init()
		{
		}

		private void OnIntroSkipped()
		{
		}

		private int OnGotEnergy()
		{
			return 0;
		}

		private void OnOrderedCourier(EConsumable consumable, int count)
		{
		}

		private bool OnCouldOrderCourier()
		{
			return false;
		}

		private void OnFedCat()
		{
		}

		[AsyncStateMachine(typeof(_003CInitDevUtils_003Ed__67))]
		private UniTaskVoid InitDevUtils()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CSetDayForEnding_003Ed__68))]
		public UniTaskVoid SetDayForEnding(EEnding ending)
		{
			return default;
		}

		public void AddEnergySlot()
		{
		}

		public void AddEnergy()
		{
		}

		public void RefillEnergy()
		{
		}

		public void RemoveExtraEnergySlotsForTomorrow(int energyCount)
		{
		}

		public void Change()
		{
		}

		private void InitNewNight(bool isFromSaveData = false)
		{
		}

		private void InitNewDay(bool isFromSaveData = false)
		{
		}

		public bool HasCompletedDaytimeGoal()
		{
			return false;
		}

		[AsyncStateMachine(typeof(_003CChangeDaytimeVisual_003Ed__77))]
		private UniTaskVoid ChangeDaytimeVisual()
		{
			return default;
		}

		private void UpdateVisualThingForSelectedTimeOfDay()
		{
		}

		private void SelectMusic()
		{
		}

		private void ChangeLightBeamMaterials()
		{
		}

		[AsyncStateMachine(typeof(_003CWatchMorningTV_003Ed__81))]
		private UniTaskVoid WatchMorningTV()
		{
			return default;
		}

		public void Act()
		{
		}

		public void ActAll()
		{
		}

		public void UpdateTimeOfDayStartedOrDialogEndedTime()
		{
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}

		private void OnSave(bool isReserve)
		{
		}

		public void AddChangePosTomorrow(ECharacterType character, ERoomPeopleState pose)
		{
		}
	}
}
