using System;
using _Code.Infrastructure.Updatable;

namespace _Code.Infrastructure.Showcase
{
	public interface IShowcaseManager
	{
		IUpdateable Updateable { get; }

		event Action Restarted;

		event Action ChangedLanguage;
	}
}
