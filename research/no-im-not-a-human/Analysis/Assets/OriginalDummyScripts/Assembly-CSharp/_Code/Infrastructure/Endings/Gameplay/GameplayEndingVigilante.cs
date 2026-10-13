namespace _Code.Infrastructure.Endings.Gameplay;

public sealed class GameplayEndingVigilante : AGameplayEnding
{
	private bool _isRevealed;

	public override int Priority => 2;

	public override EEnding Ending => EEnding.Rage;

	public override bool AreConditionsMet => _isRevealed;

	public void Reveal()
	{
		_isRevealed = true;
	}

	protected override void TriggerInner()
	{
	}
}
