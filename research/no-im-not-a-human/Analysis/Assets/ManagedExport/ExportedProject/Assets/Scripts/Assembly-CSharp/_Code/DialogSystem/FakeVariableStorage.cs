using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Yarn.Unity;

namespace _Code.DialogSystem
{
	public sealed class FakeVariableStorage : VariableStorageBehaviour
	{
		private DialogSaveData _saveData = new DialogSaveData();

		public void Init(DialogSaveData saveData)
		{
			_saveData = saveData;
		}

		public override void Clear()
		{
			_saveData.YarnBoolVariables.Clear();
			_saveData.YarnFloatVariables.Clear();
			_saveData.YarnStringVariables.Clear();
		}

		public override bool Contains(string variableName)
		{
			if (!_saveData.YarnBoolVariables.ContainsKey(variableName) && !_saveData.YarnFloatVariables.ContainsKey(variableName))
			{
				return _saveData.YarnStringVariables.ContainsKey(variableName);
			}
			return true;
		}

		public override void SetAllVariables(Dictionary<string, float> floats, Dictionary<string, string> strings, Dictionary<string, bool> bools, bool clear = true)
		{
			if (clear)
			{
				_saveData.YarnFloatVariables.Clear();
				_saveData.YarnStringVariables.Clear();
				_saveData.YarnBoolVariables.Clear();
			}
			foreach (KeyValuePair<string, float> @float in floats)
			{
				_saveData.YarnFloatVariables[@float.Key] = @float.Value;
			}
			foreach (KeyValuePair<string, string> @string in strings)
			{
				_saveData.YarnStringVariables[@string.Key] = @string.Value;
			}
			foreach (KeyValuePair<string, bool> @bool in bools)
			{
				_saveData.YarnBoolVariables[@bool.Key] = @bool.Value;
			}
		}

		public override (Dictionary<string, float>, Dictionary<string, string>, Dictionary<string, bool>) GetAllVariables()
		{
			return (_saveData.YarnFloatVariables, _saveData.YarnStringVariables, _saveData.YarnBoolVariables);
		}

		public override bool TryGetValue<T>(string variableName, out T result)
		{
			if (typeof(T) == typeof(string))
			{
				if (_saveData.YarnStringVariables.TryGetValue(variableName, out var value))
				{
					result = (T)(object)value;
					return true;
				}
				if (_saveData.YarnBoolVariables.TryGetValue(variableName, out var value2))
				{
					string text = value2.ToString();
					result = (T)(object)text;
					return true;
				}
				if (_saveData.YarnFloatVariables.TryGetValue(variableName, out var value3))
				{
					string text2 = value3.ToString(CultureInfo.InvariantCulture);
					result = (T)(object)text2;
					return true;
				}
				result = default;
				return false;
			}
			if (typeof(T) == typeof(bool))
			{
				if (_saveData.YarnBoolVariables.TryGetValue(variableName, out var value4))
				{
					result = (T)(object)value4;
					return true;
				}
				if (_saveData.YarnStringVariables.TryGetValue(variableName, out var value5) && bool.TryParse(value5, out var result2))
				{
					result = (T)(object)result2;
					return true;
				}
				if (_saveData.YarnFloatVariables.TryGetValue(variableName, out var value6))
				{
					bool flag = !Mathf.Approximately(value6, 0f);
					result = (T)(object)flag;
					return true;
				}
				result = default;
				return false;
			}
			if (typeof(T) == typeof(float))
			{
				if (_saveData.YarnFloatVariables.TryGetValue(variableName, out var value7))
				{
					result = (T)(object)value7;
					return true;
				}
				if (_saveData.YarnStringVariables.TryGetValue(variableName, out var value8) && float.TryParse(value8, NumberStyles.Float, CultureInfo.InvariantCulture, out var result3))
				{
					result = (T)(object)result3;
					return true;
				}
				if (_saveData.YarnBoolVariables.TryGetValue(variableName, out var value9))
				{
					result = (T)(object)(value9 ? 1f : 0f);
					return true;
				}
				result = default;
				return false;
			}
			if (typeof(T) == typeof(int))
			{
				if (_saveData.YarnFloatVariables.TryGetValue(variableName, out var value10))
				{
					result = (T)(object)(int)value10;
					return true;
				}
				if (_saveData.YarnStringVariables.TryGetValue(variableName, out var value11))
				{
					if (int.TryParse(value11, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result4))
					{
						result = (T)(object)result4;
						return true;
					}
					if (float.TryParse(value11, NumberStyles.Float, CultureInfo.InvariantCulture, out var result5))
					{
						result = (T)(object)(int)result5;
						return true;
					}
				}
				if (_saveData.YarnBoolVariables.TryGetValue(variableName, out var value12))
				{
					result = (T)(object)(value12 ? 1 : 0);
					return true;
				}
				result = default;
				return false;
			}
			if (typeof(T) == typeof(double))
			{
				if (_saveData.YarnFloatVariables.TryGetValue(variableName, out var value13))
				{
					result = (T)(object)(double)value13;
					return true;
				}
				if (_saveData.YarnStringVariables.TryGetValue(variableName, out var value14) && double.TryParse(value14, NumberStyles.Float, CultureInfo.InvariantCulture, out var result6))
				{
					result = (T)(object)result6;
					return true;
				}
				if (_saveData.YarnBoolVariables.TryGetValue(variableName, out var value15))
				{
					result = (T)(object)(value15 ? 1.0 : 0.0);
					return true;
				}
				result = default;
				return false;
			}
			if (_saveData.YarnStringVariables.TryGetValue(variableName, out var value16))
			{
				try
				{
					result = (T)(object)value16;
					return true;
				}
				catch
				{
				}
			}
			if (_saveData.YarnFloatVariables.TryGetValue(variableName, out var value17))
			{
				try
				{
					result = (T)(object)value17;
					return true;
				}
				catch
				{
				}
			}
			if (_saveData.YarnBoolVariables.TryGetValue(variableName, out var value18))
			{
				try
				{
					result = (T)(object)value18;
					return true;
				}
				catch
				{
				}
			}
			result = default;
			return false;
		}

		public override void SetValue(string variableName, string stringValue)
		{
			_saveData.YarnFloatVariables.Remove(variableName);
			_saveData.YarnBoolVariables.Remove(variableName);
			_saveData.YarnStringVariables[variableName] = stringValue;
		}

		public override void SetValue(string variableName, bool boolValue)
		{
			_saveData.YarnFloatVariables.Remove(variableName);
			_saveData.YarnStringVariables.Remove(variableName);
			_saveData.YarnBoolVariables[variableName] = boolValue;
		}

		public override void SetValue(string variableName, float floatValue)
		{
			_saveData.YarnBoolVariables.Remove(variableName);
			_saveData.YarnStringVariables.Remove(variableName);
			_saveData.YarnFloatVariables[variableName] = floatValue;
		}
	}
}
