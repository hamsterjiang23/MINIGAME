using System;
using System.Runtime.CompilerServices;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Pause
{
	public sealed class MockPauseController : IPauseController
	{
		[CompilerGenerated]
		private Action<bool> m_PauseStateChanged;

		public IUpdateable Updateable { get; }

		public bool IsPaused { get; }

		public event Action<bool> PauseStateChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}
	}
}
