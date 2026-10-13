using System;
using System.Collections.Generic;
using UnityEngine;
using _Other.SimpleBalancer.Settings;

namespace _Other.SimpleBalancer.Environments
{
	public abstract class BalanceTimeEnvironment<T> where T : Enum
	{
		private readonly Dictionary<T, BalanceRandomFloatSetting> _randomSettings;

		private readonly Dictionary<T, BalanceFloatSetting> _floatSettings;

		private readonly Dictionary<T, BalanceColorSetting> _colorSettings;

		private float _startTime;

		private float _maxTime;

		private float MaxTime => 0f;

		private float CurrentTime => 0f;

		public BalanceTimeEnvironment<T> SetMaxTime(float maxTime)
		{
			return null;
		}

		public BalanceTimeEnvironment<T> SetStartTime(float maxTime)
		{
			return null;
		}

		public BalanceTimeEnvironment<T> AddRandomSetting(T settingType, BalanceRandomFloatSetting randomFloatSetting)
		{
			return null;
		}

		public float GetRandomValue(T settingType)
		{
			return 0f;
		}

		public BalanceTimeEnvironment<T> AddFloatSetting(T settingType, BalanceFloatSetting floatSetting)
		{
			return null;
		}

		public float GetFloatValue(T settingType)
		{
			return 0f;
		}

		public BalanceTimeEnvironment<T> AddColorSetting(T settingType, BalanceColorSetting colorSetting)
		{
			return null;
		}

		public Color GetColorValue(T settingType)
		{
			return default;
		}
	}
}
