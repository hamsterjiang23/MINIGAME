using System.Collections.Generic;

namespace _Code.Utils.CustomYarnReading
{
	public interface INodeNameGetterSaveData
	{
		Dictionary<int, int> NodeNamesIndexes { get; }

		List<int> TodayDialogInteractables { get; }
	}
}
