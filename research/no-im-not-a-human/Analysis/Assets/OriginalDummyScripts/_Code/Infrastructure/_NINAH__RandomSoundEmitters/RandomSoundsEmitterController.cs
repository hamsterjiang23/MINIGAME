using Zenject;
using _Code.Events;
using _Code.Infrastructure.DayNight;
using _Code.Infrastructure.Player;
using _Scripts.Services.Sound.Service;

namespace _Code.Infrastructure._NINAH__RandomSoundEmitters
{
	public sealed class RandomSoundsEmitterController : IRandomSoundsEmitterController, ITickable
	{
		private readonly IDayNightController _dayNightController;

		private ETimeOfDay _currentTimeOfDay;

		private float _emitterDelay;

		private float _lastEmitterTime;

		private readonly RandomSoundsEmitterSOData _data;

		private readonly INotAHumanSoundService _soundService;

		private readonly IPlayerService _playerService;

		public RandomSoundsEmitterController(IDayNightController dayNightController, IRandomSoundsEmitterSODataViewProvider soundsEmitterSoDataViewProvider, INotAHumanSoundService soundService, IPlayerService playerService)
		{
		}

		private void OnCurrentTimeOfDayChanged(ETimeOfDay timeOfDay)
		{
		}

		public void Tick()
		{
		}
	}
}
