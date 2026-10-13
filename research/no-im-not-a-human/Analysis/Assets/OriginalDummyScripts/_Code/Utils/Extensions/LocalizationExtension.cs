using UnityEngine.Localization.Components;

namespace _Code.Utils.Extensions
{
	public static class LocalizationExtension
	{
		public const string UI_TABLE = "UI";

		public const string DIALOGS_TABLE = "Dialogs";

		public static void SetupLocalization(this LocalizeStringEvent localizeStringEvent, string table, string key)
		{
		}

		public static string GetLocalization(this string key, string table = "UI", params object[] args)
		{
			return null;
		}
	}
}
