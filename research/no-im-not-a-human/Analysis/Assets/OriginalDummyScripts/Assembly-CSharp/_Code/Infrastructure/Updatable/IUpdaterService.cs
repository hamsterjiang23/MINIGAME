namespace _Code.Infrastructure.Updatable;

public interface IUpdaterService
{
	void AddUpdatable(IUpdateable updateable);

	void RemoveUpdatable(IUpdateable updateable);

	void Run();

	void Stop();
}
