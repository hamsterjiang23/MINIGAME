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

		public override int Priority => 1;

		public override EEnding Ending => EEnding.FEMA;

		public override bool AreConditionsMet
		{
			get
			{
				if (!_dialogManager.EverRudeToFema && _consumablesController.PovistaskUsedCount >= _endingSOData.PovistkasUsedCount)
				{
					return _getFemaCallsCount?.Invoke() >= _endingSOData.FemaCallsCount;
				}
				return false;
			}
		}

		protected override void TriggerInner()
		{
			_dayNightController.SetDayForEnding(EEnding.FEMA);
		}

		public void InitModules(IDialogManager dialogManager, IConsumablesController consumablesController, IEndingSODataProvider endingSODataProvider, IDayNightController dayNightController)
		{
			_dialogManager = dialogManager;
			_consumablesController = consumablesController;
			_endingSOData = endingSODataProvider.EndingSOData;
			_dayNightController = dayNightController;
		}

		public void InitFunc(Func<int> getFemaCallsCount)
		{
			_getFemaCallsCount = getFemaCallsCount;
		}
	}
}
