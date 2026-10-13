using System;
using _Code.Events;

namespace _Code.Infrastructure._NINAH__Cat
{
	public interface ICatController
	{
		event Action DayStarted;

		event Action NightStarted;

		event Action Born;

		void ActivateCat();

		void ChangePosition(ETimeOfDay currentTimeOfDay);

		void PlayAnimation();
	}
}
