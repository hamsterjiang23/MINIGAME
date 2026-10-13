using System.Collections.Generic;

namespace _Code.Infrastructure.Updatable
{
	public sealed class UpdaterService : IUpdaterService
	{
		private IUpdaterInstance _instance;

		private readonly List<IUpdateable> _updatables;

		public UpdaterService(IUpdaterInstanceProvider instanceProvider)
		{
		}

		public void AddUpdatable(IUpdateable updateable)
		{
		}

		public void RemoveUpdatable(IUpdateable updateable)
		{
		}

		public void Run()
		{
		}

		public void Stop()
		{
		}
	}
}
