using Cysharp.Threading.Tasks;
using _Code.Characters;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.StateObjects;

namespace _Code.Infrastructure.Endings.Gameplay;

public sealed class GameplayEndingStayAliveWithThem : AGameplayEnding
{
	private IDayNightController _dayNightController;

	private ICharactersManager _charactersManager;

	private IStateObjectController _stateObjectController;

	private int _windowsNailedUp;

	public override int Priority => 1;

	public override EEnding Ending => EEnding.BasementWithThem;

	public override bool AreConditionsMet
	{
		get
		{
			if (_dayNightController.IsEndingDay)
			{
				return _charactersManager.ImpostersCount > 0;
			}
			return false;
		}
	}

	public int NailedUpWindowsCount => _windowsNailedUp;

	public void InitModules(IDayNightController dayNightController, ICharactersManager charactersManager, IStateObjectController stateObjectController)
	{
		_dayNightController = dayNightController;
		_charactersManager = charactersManager;
		_stateObjectController = stateObjectController;
	}

	protected override void TriggerInner()
	{
		_stateObjectController.SetState(EStateObjectType.WindowBoardsTriggers, 1);
	}

	public bool NailUpWindow()
	{
		_windowsNailedUp++;
		if (_windowsNailedUp != 3)
		{
			return false;
		}
		ShowClip(1f).Forget();
		return true;
	}
}
