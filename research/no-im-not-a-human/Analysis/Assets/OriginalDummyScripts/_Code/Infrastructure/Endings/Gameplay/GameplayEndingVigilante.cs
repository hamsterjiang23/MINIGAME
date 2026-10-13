namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingVigilante : AGameplayEnding
	{
		private bool _isRevealed;

		public override int Priority => 0;

		public override EEnding Ending => EEnding.TestEnding;

		public override bool AreConditionsMet => false;

		public void Reveal()
		{
		}

		protected override void TriggerInner()
		{
		}
	}
}
