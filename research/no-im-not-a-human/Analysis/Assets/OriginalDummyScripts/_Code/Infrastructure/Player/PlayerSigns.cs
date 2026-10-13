using _Code.Infrastructure.DayNight;

namespace _Code.Infrastructure.Player
{
	public sealed class PlayerSigns
	{
		private PlayerServiceSaveData _saveData;

		private IDayNightController _dayNightController;

		public bool IsTeethFake { get; private set; }

		public bool IsHandsFake { get; private set; }

		public bool IsEyeFake { get; private set; }

		public bool IsArmpitFake { get; private set; }

		public bool IsPhotoFake { get; private set; }

		public void InitModules(PlayerServiceSaveData saveData, IDayNightController dayNightController)
		{
		}

		public void Smoke()
		{
		}

		public void Dig()
		{
		}

		public void DrinkCaffeine()
		{
		}

		public void Wash()
		{
		}

		public void ResetTeeth()
		{
		}

		public void ResetEye()
		{
		}

		public void ResetHands()
		{
		}

		public void ResetArmpits()
		{
		}
	}
}
