using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.ActionableObjects
{
	public interface IActionableObjectsManager
	{
		IUpdateable[] ActionableObjectsUpdates { get; }

		void ForceLeave();

		void SetLockedStateForAll(bool isLocked);
	}
}
