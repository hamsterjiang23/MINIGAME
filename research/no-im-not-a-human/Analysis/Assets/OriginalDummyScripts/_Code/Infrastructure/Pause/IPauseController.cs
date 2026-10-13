using System;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Pause
{
	public interface IPauseController
	{
		IUpdateable Updateable { get; }

		bool IsPaused { get; }

		event Action<bool> PauseStateChanged;
	}
}
