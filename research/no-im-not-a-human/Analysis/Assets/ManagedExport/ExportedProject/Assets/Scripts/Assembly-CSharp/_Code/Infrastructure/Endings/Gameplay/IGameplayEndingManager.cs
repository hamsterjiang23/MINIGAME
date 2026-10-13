using System;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public interface IGameplayEndingManager
	{
		AGameplayEnding ActualEnding { get; }

		event Action<EEnding> EndingTriggered;

		AGameplayEnding GetEnding(EEnding ending);

		bool TryNailUpWindowForBasement();

		void InitGetFemaCallsFunc(Func<int> getPovistkasCount);
	}
}
