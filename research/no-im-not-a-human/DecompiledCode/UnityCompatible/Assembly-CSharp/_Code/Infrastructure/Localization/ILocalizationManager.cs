using _Code.Language;

namespace _Code.Infrastructure.Localization
{
	public interface ILocalizationManager
	{
		ELanguage CurrentLanguage { get; }

		void NextLanguage();

		void PreviousLanguage();
	}
}
