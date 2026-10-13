using System;
using System.Runtime.CompilerServices;
using _Code.Infrastructure.Cursor;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Showcase
{
	public sealed class ShowcaseManager : IShowcaseManager, IUpdateable
	{
		private const float TIME_TO_RESTART = 600f;

		private float _restartTimer;

		[CompilerGenerated]
		private Action m_Restarted;

		[CompilerGenerated]
		private Action m_ChangedLanguage;

		private ICursorController _cursorController;

		public IUpdateable Updateable => null;

		public event Action Restarted
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

		public event Action ChangedLanguage
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

		public ShowcaseManager(ICursorController cursorController)
		{
		}

		private void Init()
		{
		}

		private void Restart()
		{
		}

		public void OnUpdateAction()
		{
		}

		private void ProcessRestartTimer()
		{
		}
	}
}
