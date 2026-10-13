using System;
using _Code.DialogSystem;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Data;

namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingFema : AGameplayEnding
	{
		private IDialogManager _dialogManager;

		private IConsumablesController _consumablesController;

		private EndingsSOData _endingSOData;

		private IDayNightController _dayNightController;

		private Func<int> _getFemaCallsCount;

		public override int Priority => 0;

		public override EEnding Ending => EEnding.TestEnding;

		public override bool AreConditionsMet => false;

		protected override void TriggerInner()
		{
		}

		public void InitModules(IDialogManager dialogManager, IConsumablesController consumablesController, IEndingSODataProvider endingSODataProvider, IDayNightController dayNightController)
		{
		}

		public void InitFunc(Func<int> getFemaCallsCount)
		{
		}
	}
}
