namespace _Code.Infrastructure.Endings.Gameplay
{
	public sealed class GameplayEndingSuper : AGameplayEnding
	{
		public override int Priority => 0;

		public override EEnding Ending => EEnding.TestEnding;

		public override bool AreConditionsMet => false;

		protected override void TriggerInner()
		{
		}
	}
}
