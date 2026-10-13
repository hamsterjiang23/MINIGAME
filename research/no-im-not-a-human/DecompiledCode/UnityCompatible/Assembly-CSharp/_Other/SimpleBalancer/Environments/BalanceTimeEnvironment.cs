using System;
using System.Collections.Generic;
using UnityEngine;
using _Other.SimpleBalancer.Settings;

namespace _Other.SimpleBalancer.Environments
{
	public abstract class BalanceTimeEnvironment<T> where T : Enum
	{
		private readonly Dictionary<T, BalanceRandomFloatSetting> _randomSettings = new Dictionary<T, BalanceRandomFloatSetting>();

		private readonly Dictionary<T, BalanceFloatSetting> _floatSettings = new Dictionary<T, BalanceFloatSetting>();

		private readonly Dictionary<T, BalanceColorSetting> _colorSettings = new Dictionary<T, BalanceColorSetting>();

		private float _startTime;

		private float _maxTime;

		private float MaxTime => _startTime + _maxTime;

		private float CurrentTime => (Mathf.Clamp(Time.timeSinceLevelLoad, _startTime, MaxTime) - _startTime) / MaxTime;

		public BalanceTimeEnvironment<T> SetMaxTime(float maxTime)
		{
			_maxTime = maxTime;
			return this;
		}

		public BalanceTimeEnvironment<T> SetStartTime(float maxTime)
		{
			_startTime = maxTime;
			return this;
		}

		public BalanceTimeEnvironment<T> AddRandomSetting(T settingType, BalanceRandomFloatSetting randomFloatSetting)
		{
			if (_randomSettings.ContainsKey(settingType))
			{
				_randomSettings[settingType] = randomFloatSetting;
			}
			else
			{
				_randomSettings.Add(settingType, randomFloatSetting);
			}
			return this;
		}

		public float GetRandomValue(T settingType)
		{
			if (_randomSettings.TryGetValue(settingType, out var value))
			{
				return value.GetValue(CurrentTime);
			}
			throw new KeyNotFoundException($"No random settings found for type {settingType}");
		}

		public BalanceTimeEnvironment<T> AddFloatSetting(T settingType, BalanceFloatSetting floatSetting)
		{
			if (_floatSettings.ContainsKey(settingType))
			{
				_floatSettings[settingType] = floatSetting;
			}
			else
			{
				_floatSettings.Add(settingType, floatSetting);
			}
			return this;
		}

		public float GetFloatValue(T settingType)
		{
			if (_floatSettings.TryGetValue(settingType, out var value))
			{
				return value.GetValue(CurrentTime);
			}
			throw new KeyNotFoundException($"No float settings found for type {settingType}");
		}

		public BalanceTimeEnvironment<T> AddColorSetting(T settingType, BalanceColorSetting colorSetting)
		{
			if (_colorSettings.ContainsKey(settingType))
			{
				_colorSettings[settingType] = colorSetting;
			}
			else
			{
				_colorSettings.Add(settingType, colorSetting);
			}
			return this;
		}

		public Color GetColorValue(T settingType)
		{
			if (_colorSettings.TryGetValue(settingType, out var value))
			{
				return value.GetValue(CurrentTime);
			}
			throw new KeyNotFoundException($"No color settings found for type {settingType}");
		}
	}
}
