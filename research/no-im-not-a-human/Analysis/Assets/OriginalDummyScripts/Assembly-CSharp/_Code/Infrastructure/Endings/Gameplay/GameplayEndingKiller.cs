using _Code.Characters;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Endings.Data;

namespace _Code.Infrastructure.Endings.Gameplay;

public sealed class GameplayEndingKiller : AGameplayEnding
{
	private IDayNightController _dayNightController;

	private ICharactersManager _charactersManager;

	private IEndingSODataProvider _endingSoDataProvider;

	private bool _isReady;

	public override int Priority => 3;

	public override EEnding Ending => EEnding.Killer;

	public override bool AreConditionsMet => _charactersManager.InnocentsKilledCount >= _endingSoDataProvider.EndingSOData.KillsCount;

	protected override void TriggerInner()
	{
		_dayNightController.SetDayForEnding(EEnding.Killer);
	}

	public void InitModules(IDayNightController dayNightController, ICharactersManager charactersManager, IEndingSODataProvider endingSoDataProvider)
	{
		_dayNightController = dayNightController;
		_charactersManager = charactersManager;
		_endingSoDataProvider = endingSoDataProvider;
	}

	public void UnlockCartoon()
	{
		_isReady = true;
	}
}
