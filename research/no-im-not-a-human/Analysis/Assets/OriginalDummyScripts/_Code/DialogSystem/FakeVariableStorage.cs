using System.Collections.Generic;
using Yarn.Unity;

namespace _Code.DialogSystem
{
	public sealed class FakeVariableStorage : VariableStorageBehaviour
	{
		private DialogSaveData _saveData;

		public void Init(DialogSaveData saveData)
		{
		}

		public override void Clear()
		{
		}

		public override bool Contains(string variableName)
		{
			return false;
		}

		public override void SetAllVariables(Dictionary<string, float> floats, Dictionary<string, string> strings, Dictionary<string, bool> bools, bool clear = true)
		{
		}

		public override (Dictionary<string, float>, Dictionary<string, string>, Dictionary<string, bool>) GetAllVariables()
		{
			return default;
		}

		public override bool TryGetValue<T>(string variableName, out T result)
		{
			result = default;
			return false;
		}

		public override void SetValue(string variableName, string stringValue)
		{
		}

		public override void SetValue(string variableName, bool boolValue)
		{
		}

		public override void SetValue(string variableName, float floatValue)
		{
		}
	}
}
