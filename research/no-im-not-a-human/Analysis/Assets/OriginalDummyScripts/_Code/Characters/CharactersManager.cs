using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.CompilerServices;
using _Code.Events;
using _Code.Infrastructure;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure.Randomization;
using _Code.Infrastructure.Rooms;
using _Code.Rooms;
using _Scripts.Services.DataModel;

namespace _Code.Characters
{
	public sealed class CharactersManager : ASavableClass<CharactersManagerSaveData>, ICharactersManager, IDisposable
	{
		[StructLayout((LayoutKind)3)]
		[CompilerGenerated]
		private struct _003CInitDevUtils_003Ed__42 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncUniTaskMethodBuilder _003C_003Et__builder;

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

		private CharactersManagerSaveData _saveData;

		private readonly CharacterSOData[] _characterData;

		private readonly IGameEventsManager _gameEventsManager;

		[CompilerGenerated]
		private Func<ERoom, ARoom> m_GetRoom;

		[CompilerGenerated]
		private Action m_ScreamerShowed;

		[CompilerGenerated]
		private Action<ECharacterType> m_CharacterRemoved;

		[CompilerGenerated]
		private Action<ECharacterType, bool> m_CharacterAdded;

		[CompilerGenerated]
		private Func<(int Day, ETimeOfDay CurrentTimeOfDay, float LastChange)> m_GetBaseDayNightData;

		public Func<float> GetImpostersPercent;

		private readonly OtherGameSOData _otherData;

		private readonly IDataModelService _dataModelService;

		private readonly RandomGenerationSettingsSOData _randomGenerationSettings;

		private ECharacterType _currentDialogCharacter;

		public int ImpostersCount => 0;

		public int InnocentsCount => 0;

		public int Count => 0;

		public int InnocentsKilledCount => 0;

		public int ImpostersKilledCount => 0;

		public int EverImposterInHouse => 0;

		public IReadOnlyList<ECharacterType> CharactersInside => null;

		public int KilledByImpostersCount => 0;

		public event Func<ERoom, ARoom> GetRoom
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

		public event Action ScreamerShowed
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

		public event Action<ECharacterType> CharacterRemoved
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

		public event Action<ECharacterType, bool> CharacterAdded
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

		public event Func<(int Day, ETimeOfDay CurrentTimeOfDay, float LastChange)> GetBaseDayNightData
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

		public CharacterSOData GetCharacter(ECharacterType characterType)
		{
			return null;
		}

		public CharactersManager(ICharactersSODataProvider dataProvider, IGameEventsManager gameEventsManager, IDataModelService dataModelService, IOtherGameSODataProvider otherGameSoDataProvider, IRandomGenerationSettingsSODataProvider randomGenerationSettingsSoDataProvider)
		{
		}

		private (int, int) OnGotCharactersAndImpostersCount()
		{
			return default;
		}

		[AsyncStateMachine(typeof(_003CInitDevUtils_003Ed__42))]
		private UniTask InitDevUtils()
		{
			return default;
		}

		public void LetIn(CharacterSOData character, bool exileOldCharacterFromThisPlace = false)
		{
		}

		private void OnLoadLetIn(CharacterSOData character)
		{
		}

		private void OnLoadLetInAndKill(CharacterSOData character)
		{
		}

		private void GenerateStatus(CharacterSOData character)
		{
		}

		public void Refuse(CharacterSOData character)
		{
		}

		public void Kill(CharacterSOData character, bool isByPlayer = false)
		{
		}

		public void Kill(ECharacterType character)
		{
		}

		public void KillTomorrow(CharacterSOData character)
		{
		}

		public void ExileAfterTomorrow(CharacterSOData character)
		{
		}

		public bool EverRefused()
		{
			return false;
		}

		public void RunMorningOfSuicides()
		{
		}

		public CharacterSOData[] ExilyByFEMA(int count)
		{
			return null;
		}

		private List<ECharacterType> GetFavoritesList()
		{
			return null;
		}

		public CharacterSOData GetRandomCharacterToExile()
		{
			return null;
		}

		public void ExileCharacter(ECharacterType character)
		{
		}

		private CharacterSOData[] GetPossibleCharactersToKill()
		{
			return null;
		}

		public int GetPossibleCharactersToKillCount()
		{
			return 0;
		}

		public bool IsCharacterAliveInside(ECharacterType characterType)
		{
			return false;
		}

		public void KillRandom()
		{
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}

		public void GivePovistka(ECharacterType character)
		{
		}

		public void KillRoom(CharacterSOData characterOfRoom)
		{
		}

		public void KillEveryone()
		{
		}

		public void ClearPovistkas()
		{
		}

		public ERoom GetCharacterRoom(ECharacterType character)
		{
			return ERoom.Kitchen;
		}

		public bool IsImposter(ECharacterType character)
		{
			return false;
		}

		public ECharacterPlace GetPlace(ECharacterType character)
		{
			return ECharacterPlace.None;
		}

		public void BeginNewDay(int day)
		{
		}

		public void OnGunStateSelected(bool isTaken)
		{
		}

		public void OnDialogStarted(ECharacterType character)
		{
		}

		public void OnDayDialogTalked(bool hasAlreadyTalkedToday)
		{
		}
	}
}
