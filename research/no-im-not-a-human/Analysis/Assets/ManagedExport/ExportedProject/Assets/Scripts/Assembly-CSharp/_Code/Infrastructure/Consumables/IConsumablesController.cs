using System;

namespace _Code.Infrastructure.Consumables
{
	public interface IConsumablesController
	{
		int PovistaskUsedCount { get; }

		event Action<EConsumable, int> UpdatedCount;

		event Action<EConsumable, int> ReceivedWithHint;

		event Action<EConsumable, int> GivenAwayWithHint;

		void Add(EConsumable consumable, int count = 1, bool isShowHint = false);

		bool TryRemove(EConsumable consumable, int count = 1, bool isShowHint = false);

		int Count(EConsumable consumable);

		void InitializeGetDay(Func<int> func);

		bool IsNeedToGetDeficitItem();

		EConsumable GetDeficitItem();

		void UpdateDeficitDay();

		void BeginNewDay();

		bool EverUsedConsumableThisRun(EConsumable consumable);
	}
}
