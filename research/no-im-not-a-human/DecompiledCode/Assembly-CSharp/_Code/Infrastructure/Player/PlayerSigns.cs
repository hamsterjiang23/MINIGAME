using _Code.Infrastructure.DayNight;

namespace _Code.Infrastructure.Player;

public sealed class PlayerSigns
{
	private PlayerServiceSaveData _saveData;

	private IDayNightController _dayNightController;

	public bool IsTeethFake { get; private set; } = true;

	public bool IsHandsFake { get; private set; }

	public bool IsEyeFake { get; private set; }

	public bool IsArmpitFake { get; private set; }

	public bool IsPhotoFake { get; private set; } = true;

	public void InitModules(PlayerServiceSaveData saveData, IDayNightController dayNightController)
	{
		_saveData = saveData;
		_dayNightController = dayNightController;
	}

	public void Smoke()
	{
		IsTeethFake = false;
		_saveData.DayOfTeethReset = _dayNightController.Day + 4;
	}

	public void Dig()
	{
		IsHandsFake = true;
		_saveData.DayOfHandsReset = _dayNightController.Day + 1;
	}

	public void DrinkCaffeine()
	{
		IsEyeFake = true;
		_saveData.DayOfEyesReset = _dayNightController.Day + 3;
	}

	public void Wash()
	{
		IsArmpitFake = false;
		IsHandsFake = false;
		_saveData.DayOfArmpitReset = _dayNightController.Day + 5;
	}

	public void ResetTeeth()
	{
		IsTeethFake = true;
	}

	public void ResetEye()
	{
		IsEyeFake = false;
	}

	public void ResetHands()
	{
		IsHandsFake = false;
	}

	public void ResetArmpits()
	{
		IsArmpitFake = true;
	}
}
