using System.Collections.Generic;

namespace _Code.Infrastructure.Updatable
{
	public interface IUpdaterInstance
	{
		void Init(IReadOnlyList<IUpdateable> updatables);

		void SetActiveState(bool isActive);
	}
}
