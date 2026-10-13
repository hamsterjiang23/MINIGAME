using System;

namespace _Code.Infrastructure.Endings.Gameplay;

public sealed class GameplayEndingSuper : AGameplayEnding
{
	public override int Priority => 1;

	public override EEnding Ending => EEnding.KilledBySuper;

	public override bool AreConditionsMet => true;

	protected override void TriggerInner()
	{
		throw new NotImplementedException();
	}
}
