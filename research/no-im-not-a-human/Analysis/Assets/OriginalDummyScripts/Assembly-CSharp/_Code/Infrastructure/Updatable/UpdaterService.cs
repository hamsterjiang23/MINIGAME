using System.Collections.Generic;

namespace _Code.Infrastructure.Updatable;

public sealed class UpdaterService : IUpdaterService
{
	private IUpdaterInstance _instance;

	private readonly List<IUpdateable> _updatables = new List<IUpdateable>();

	public UpdaterService(IUpdaterInstanceProvider instanceProvider)
	{
		_instance = instanceProvider.UpdaterInstance;
	}

	public void AddUpdatable(IUpdateable updateable)
	{
		_updatables.Add(updateable);
	}

	public void RemoveUpdatable(IUpdateable updateable)
	{
		_updatables.Remove(updateable);
	}

	public void Run()
	{
		_instance.Init(_updatables);
		_instance.SetActiveState(isActive: true);
	}

	public void Stop()
	{
		_instance.SetActiveState(isActive: false);
	}
}
