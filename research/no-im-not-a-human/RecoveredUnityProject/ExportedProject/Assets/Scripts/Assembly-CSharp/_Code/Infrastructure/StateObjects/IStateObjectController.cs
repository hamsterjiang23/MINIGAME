using System;

namespace _Code.Infrastructure.StateObjects
{
	public interface IStateObjectController
	{
		event Action<EStateObjectType> ReachedLastState;

		event Action<EStateObjectType, int> StateChanged;

		void SetState(EStateObjectType stateObjectType, int index);

		void IncrementState(EStateObjectType stateObjectType);

		int GetState(EStateObjectType ground);

		void SetStateDelayed(EStateObjectType state, int index, int day);

		void CheckChangesForDay(int day);
	}
}
