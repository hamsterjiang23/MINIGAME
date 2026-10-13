using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace _Code.Utils.Extensions
{
	public static class LocalizationExtension
	{
		public const string UI_TABLE = "UI";

		public const string DIALOGS_TABLE = "Dialogs";

		public static void SetupLocalization(this LocalizeStringEvent localizeStringEvent, string table, string key)
		{
			localizeStringEvent.SetTable(table);
			localizeStringEvent.SetEntry(key);
		}

		public static string GetLocalization(this string key, string table = "UI", params object[] args)
		{
			StringTableEntry entry = LocalizationSettings.StringDatabase.GetTable(table).GetEntry(key);
			if (entry == null)
			{
				Debug.LogError("Can't find " + key + " in " + table);
				return "-";
			}
			if (!entry.IsSmart)
			{
				return entry.GetLocalizedString(args);
			}
			if (args == null || args.Length == 0)
			{
				return entry.GetLocalizedString();
			}
			return entry.GetLocalizedString(args);
		}
	}
}
