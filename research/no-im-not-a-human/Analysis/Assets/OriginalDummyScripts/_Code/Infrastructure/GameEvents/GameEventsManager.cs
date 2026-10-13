using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.Characters;
using _Code.Events;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.Randomization;
using _Code.Infrastructure.Updatable;
using _Scripts.Services.DataModel;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure.GameEvents
{
	public sealed class GameEventsManager : ASavableClass<GameEventsManagerSaveData>, IGameEventsManager, IUpdateable, IDisposable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CGenerateDingDongEventsForDayAsync_003Ed__66 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public GameEventsManager _003C_003E4__this;

			public int day;

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
		private struct _003CGenerateDingDongEventsForPresetAsync_003Ed__63 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskVoidMethodBuilder _003C_003Et__builder;

			public GameEventsManager _003C_003E4__this;

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

		private GameEventsManagerSaveData _saveData;

		private readonly GameEventsManagerSOData _data;

		private readonly ICharactersSODataProvider _charactersSODataProvider;

		private readonly INotAHumanSoundService _soundService;

		private readonly IGameplayRandomizer _gameplayRandomizer;

		private readonly IDataModelService _dataModelService;

		private Dictionary<int, List<(CharacterDingDongEvent dingDong, int order)>> _preDingDongEvents;

		private const string DING_DONG_EVENT_PREFIX = "entrance";

		private const string BELLY_EVENT_PREFIX = "belly";

		[CompilerGenerated]
		private Action m_AnyEventCompleted;

		[CompilerGenerated]
		private Action<string> m_SpecifiedEventCompleted;

		[CompilerGenerated]
		private Func<(int day, ETimeOfDay timeOfDay, float timeFromLastChange)> m_GetBaseDayNightData;

		[CompilerGenerated]
		private Func<(int, int)> m_GetCharactersAndImpostersCount;

		[CompilerGenerated]
		private Action<CharacterDingDongEvent> m_CharacterCame;

		[CompilerGenerated]
		private Action m_CharacterSoonCame;

		private float _lastTimeUpdated;

		private readonly float _updateInterval;

		private bool _arePresetReady;

		private List<string> CompletedEvents => null;

		public IUpdateable Updateable => null;

		private Func<int, bool> CheckProphetCondition { get; set; }

		private Func<int, bool> CheckMushrommeaterCondition { get; set; }

		private Func<int, bool> CheckPriestCondition { get; set; }

		private Func<int> GetDay { get; set; }

		public event Action AnyEventCompleted
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

		public event Action<string> SpecifiedEventCompleted
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

		public event Func<(int day, ETimeOfDay timeOfDay, float timeFromLastChange)> GetBaseDayNightData
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

		public event Func<(int, int)> GetCharactersAndImpostersCount
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

		public event Action<CharacterDingDongEvent> CharacterCame
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

		public event Action CharacterSoonCame
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

		public GameEventsManager(IGameEventsManagerSODataProvider dataProvider, ICharactersSODataProvider charactersSODataProvider, IGameplayRandomizer gameplayRandomizer, INotAHumanSoundService soundService, IDataModelService dataModelService)
		{
		}

		public void InitializeProphetCondition(Func<int, bool> condition)
		{
		}

		public void InitializeMushroomeaterCondition(Func<int, bool> condition)
		{
		}

		public void InitializePriestCondition(Func<int, bool> condition)
		{
		}

		public void InitializeGetDay(Func<int> func)
		{
		}

		public void CallFema()
		{
		}

		public void BeginNewDay()
		{
		}

		public void CompleteEvent(string eventName)
		{
		}

		public bool AreAllDingDongsCompletedThisNight(int day)
		{
			return false;
		}

		public void CheckOtherEvents()
		{
		}

		private void CheckDingDongEvents()
		{
		}

		public void OnUpdateAction()
		{
		}

		public void GenerateBabyEvents(int day)
		{
		}

		[AsyncStateMachine(typeof(_003CGenerateDingDongEventsForPresetAsync_003Ed__63))]
		private UniTaskVoid GenerateDingDongEventsForPresetAsync()
		{
			return default;
		}

		private void GenerateDingDongEventsForPreset()
		{
		}

		public void DisableSuperForDay(int day)
		{
		}

		[AsyncStateMachine(typeof(_003CGenerateDingDongEventsForDayAsync_003Ed__66))]
		public UniTaskVoid GenerateDingDongEventsForDayAsync(int day)
		{
			return default;
		}

		public void GenerateDingDongEventsForSkip()
		{
		}

		private bool CheckProphetToRemove(int day)
		{
			return false;
		}

		private bool CheckMushroomeaterToRemove(int day)
		{
			return false;
		}

		private bool CheckPriestToRemove(int day)
		{
			return false;
		}

		public void GenerateCharacterDingDongEventsForDay(int day, ECharacterType characterType)
		{
		}

		private void GenerateDingDongEventsOrder(int day)
		{
		}

		private float GetDingDongDelay(int dayNmber, int guestNumber)
		{
			return 0f;
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}

		private void OnSave(bool isReserve)
		{
		}
	}
}
