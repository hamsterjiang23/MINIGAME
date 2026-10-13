using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure
{
	public interface IInteractablesManager
	{
		IUpdateable[] Updateables { get; }
	}
}
