using System.Linq;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using Yarn.Unity;

namespace _Code.Utils.CustomYarnReading;

public sealed class CustomYarnReader : MonoBehaviour
{
	[SerializeField]
	private YarnProject _project;

	public string[] GetNodeContent(string nodeName)
	{
		if (_project != null)
		{
			string[] source = _project.GetLineIDsForNodes(new string[1] { nodeName }).ToArray();
			StringTable table = LocalizationSettings.StringDatabase.GetTable("Dialogs");
			return source.Select((string x) =>
			{
				UnityEngine.Localization.Tables.StringTableEntry entry = table.GetEntry(x);
				if (entry == null)
				{
					return $"Can't find entry {x} in {table}";
				}
				string localizedValue = entry.LocalizedValue;
				return (!string.IsNullOrEmpty(localizedValue)) ? localizedValue : $"Can't find entry {x} in {table} for selected locale";
			}).ToArray();
		}
		Debug.LogWarning("YarnProject is not assigned.");
		return null;
	}

	public int GetNodesCountContainsString(string template)
	{
		return _project.NodeNames.Count((string x) => x.Contains(template));
	}
}
