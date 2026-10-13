using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.StateObjects;

namespace _Code.Infrastructure.Endings.Gameplay;

public sealed class GameplayEndingMushroom : AGameplayEnding
{
	private GameplayEndingManagerSaveData _saveData;

	private IStateObjectController _stateObjectController;

	private IDayNightController _dayNightController;

	private bool _cartoonUnlocked;

	public override int Priority => 1;

	public override EEnding Ending => EEnding.Mushroom;

	public override bool AreConditionsMet
	{
		get
		{
			if (_saveData.HasWatchedMushroomClock)
			{
				return _cartoonUnlocked;
			}
			return false;
		}
	}

	protected override void TriggerInner()
	{
	}

	public void InitModules(GameplayEndingManagerSaveData saveData, IStateObjectController stateObjectController, IDayNightController dayNightController)
	{
		_saveData = saveData;
		_stateObjectController = stateObjectController;
		_dayNightController = dayNightController;
	}

	public void WatchClock()
	{
		if (!_saveData.HasWatchedMushroomClock)
		{
			_saveData.HasWatchedMushroomClock = true;
		}
	}

	public void FindApple()
	{
		_saveData.HasFoundMushroomApple = true;
		_stateObjectController.SetStateDelayed(EStateObjectType.Clock, 1, _dayNightController.Day + 1);
	}

	public void EatMushroom()
	{
		_saveData.HasEatenMushroom = true;
		_stateObjectController.SetStateDelayed(EStateObjectType.Apple, 1, _dayNightController.Day + 1);
	}

	public void OpenHatch()
	{
		_saveData.HatchOpened = true;
	}

	public bool CheckCondition(int visitIndex)
	{
		return visitIndex switch
		{
			0 => _saveData.HatchOpened, 
			1 => _saveData.HatchOpened, 
			2 => _saveData.HatchOpened && _saveData.HasWatchedMushroomClock, 
		};
	}

	public void ReinitSaveData(GameplayEndingManagerSaveData saveData)
	{
		_saveData = saveData;
	}

	public void UnlockCartoon()
	{
		_cartoonUnlocked = true;
	}
}
