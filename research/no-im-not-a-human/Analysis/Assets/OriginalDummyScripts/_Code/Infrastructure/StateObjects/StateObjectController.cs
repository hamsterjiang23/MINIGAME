using System;
using System.Runtime.CompilerServices;
using Zenject;
using _Code.Characters;
using _Code.Infrastructure.DataModel.Models.GameSave;
using _Scripts.Services.DataModel;

namespace _Code.Infrastructure.StateObjects
{
	public sealed class StateObjectController : ASavableClass<StateObjectsSaveData>, IStateObjectController, IInitializable, IDisposable
	{
		private StateObjectsSaveData _saveData;

		private readonly StateObjet[] _stateObjets;

		private readonly IDataModelService _dataModelService;

		[CompilerGenerated]
		private Action<EStateObjectType> m_ReachedLastState;

		[CompilerGenerated]
		private Action<EStateObjectType, int> m_StateChanged;

		public event Action<EStateObjectType> ReachedLastState
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

		public event Action<EStateObjectType, int> StateChanged
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

		public StateObjectController(IStateObjectViewProvider viewProvider, IDataModelService dataModelService, ICharactersManager charactersManager)
		{
		}

		private void OnCharacterAdded(ECharacterType character, bool isFromSave)
		{
		}

		private void OnCharacterRemoved(ECharacterType character)
		{
		}

		public void SetState(EStateObjectType stateObjectType, int index)
		{
		}

		public void SetStateDelayed(EStateObjectType state, int index, int day)
		{
		}

		public void IncrementState(EStateObjectType stateObjectType)
		{
		}

		public int GetState(EStateObjectType ground)
		{
			return 0;
		}

		public void CheckChangesForDay(int day)
		{
		}

		protected override void OnSaveDataLoad(IGameSaveDataHandler saver)
		{
		}

		public void Initialize()
		{
		}
	}
}
