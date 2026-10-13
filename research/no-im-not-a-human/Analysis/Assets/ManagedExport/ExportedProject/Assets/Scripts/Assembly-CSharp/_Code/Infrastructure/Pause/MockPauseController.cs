using System;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Pause
{
	public sealed class MockPauseController : IPauseController
	{
		public IUpdateable Updateable { get; }

		public bool IsPaused { get; }

		public event Action<bool> PauseStateChanged;
	}
}
