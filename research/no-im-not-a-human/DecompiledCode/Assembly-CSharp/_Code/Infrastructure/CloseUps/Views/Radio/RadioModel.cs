using System;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Player;
using _Code.Utils.CustomYarnReading;

namespace _Code.Infrastructure.CloseUps.Views.Radio;

public class RadioModel
{
	private readonly float _delta;

	private readonly float _thresholdStart;

	private readonly float _wrongStateMultiplier;

	private string _message;

	private float[] _thresholdMap;

	private float _currentPosition = 0.5f;

	private CloseUpSaveData _saveData;

	private readonly CustomYarnReader _customYarnReader;

	private readonly InputHandling _inputHandler;

	private StringTableEntry _noiseCharsLocalization;

	private string[] _currentText;

	private int _currentTextIndex;

	private float _delayBeforeShowingNextText;

	private bool _isForceStopped;

	private bool _isPaused;

	private float _onFoundProgresss;

	private bool _isWaveFound;

	private float _speedUpStrength;

	private const float BaseDelay = 2f;

	private const float BaseDelayForEachCharacter = 0.1f;

	private const float FarFromCorrectWaveModifier = 0.01f;

	private const string PUNCTUATION_CHARS = "!\"#$%&'()*+, -./:;<=>?@[\\]^_`{|}~—“”“”«»¡—‘’“”„…、。々《》「」！（），：？ｈ\n";

	public float TargetPosition { get; private set; }

	private float PositionCorrectnessError
	{
		get
		{
			if (CurrentState != TargetState)
			{
				return 0.5f;
			}
			return Mathf.Clamp(Mathf.Abs(TargetPosition - _currentPosition), 0f, 0.5f);
		}
	}

	public float CurrentPosition
	{
		get
		{
			return _currentPosition;
		}
		set
		{
			_currentPosition = Mathf.Clamp01(value);
		}
	}

	public ERadioState CurrentState { get; private set; }

	public ERadioState TargetState { get; private set; }

	public float NormalisedDistance { get; private set; } = 1f;

	private float SpeedModifier
	{
		get
		{
			if (_inputHandler.CurrentDevice != EInputDevice.MouseAndKeyboard)
			{
				return 4f;
			}
			return 3f;
		}
	}

	public event Action<ERadioState> StateChanged;

	public event Action<bool> MessageFinished;

	public event Action MessageLineFinished;

	public event Action<float> TimeLeftChanged;

	public RadioModel(float delta, float thresholdStart, float wrongStateMultiplier, CustomYarnReader customYarnReader, CloseUpSaveData saveData, InputHandling inputHandler)
	{
		_inputHandler = inputHandler;
		_delta = delta;
		_thresholdStart = thresholdStart;
		_wrongStateMultiplier = wrongStateMultiplier;
		_saveData = saveData;
		_customYarnReader = customYarnReader;
	}

	public void InitNode(string nodeName)
	{
		_noiseCharsLocalization = LocalizationSettings.StringDatabase.GetTable("UI").GetEntry("Radio_NoiseCharacters");
		_currentTextIndex = 0;
		_currentText = _customYarnReader.GetNodeContent(nodeName);
		CurrentPosition = 0f;
		Setup();
		StartUpdatingMessage().Forget();
		_isForceStopped = false;
	}

	private async UniTaskVoid StartUpdatingMessage()
	{
		do
		{
			UpdateMessage(_currentText[_currentTextIndex]);
			MessageLineFinished?.Invoke();
			float delay = 2f + (float)_currentText[_currentTextIndex].Length * 0.1f;
			float startDelay = delay;
			while (delay > 0f)
			{
				float num = (_isPaused ? 0f : (Time.fixedDeltaTime * Mathf.Pow(1f - PositionCorrectnessError * 0.99f, (float)Math.E)));
				num += num * _speedUpStrength * SpeedModifier;
				delay -= num;
				TimeLeftChanged?.Invoke(delay / startDelay);
				await UniTask.WaitForFixedUpdate();
				if (_isForceStopped)
				{
					return;
				}
				if (NormalisedDistance == 1f && !_isWaveFound)
				{
					_onFoundProgresss += Time.fixedDeltaTime;
					if (_onFoundProgresss >= 1f)
					{
						_isWaveFound = true;
					}
				}
				else
				{
					_onFoundProgresss = 0f;
				}
			}
			_currentTextIndex++;
		}
		while (_currentTextIndex < _currentText.Length);
		MessageFinished?.Invoke(_isWaveFound);
	}

	public void ForceStop()
	{
		_isForceStopped = true;
	}

	private void Setup()
	{
		TargetPosition = UnityEngine.Random.Range(0f, 1f);
		TargetState = (ERadioState)UnityEngine.Random.Range(0, Enum.GetNames(typeof(ERadioState)).Length);
	}

	public void SwitchState(ERadioState newState)
	{
		if (CurrentState != newState)
		{
			CurrentState = newState;
		}
	}

	public void UpdateMessage(string newMessage)
	{
		_message = newMessage;
		CalculateThresholdMap();
	}

	private void CalculateThresholdMap()
	{
		_thresholdMap = new float[_message.Length];
		float num = 1f / (float)_message.Length;
		float num2 = num;
		for (int i = 0; i < _message.Length; i++)
		{
			_thresholdMap[i] = Mathf.Clamp01(num2);
			num2 += num;
		}
		ShuffleArray(_thresholdMap);
	}

	private void ShuffleArray(float[] array)
	{
		for (int num = array.Length - 1; num > 0; num--)
		{
			int num2 = UnityEngine.Random.Range(0, num + 1);
			ref float reference = ref array[num];
			ref float reference2 = ref array[num2];
			float num3 = array[num2];
			float num4 = array[num];
			reference = num3;
			reference2 = num4;
		}
	}

	public string GetDisplayedMessage()
	{
		if (string.IsNullOrEmpty(_message) || _thresholdMap == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		float num = TargetPosition - (_delta + _thresholdStart);
		float num2 = TargetPosition + (_delta + _thresholdStart);
		float num3 = TargetPosition - _delta;
		float num4 = TargetPosition + _delta;
		if (CurrentPosition < num || CurrentPosition > num2)
		{
			NormalisedDistance = 0f;
		}
		else if (CurrentPosition > num3 && CurrentPosition < num4)
		{
			NormalisedDistance = 1f;
		}
		else if (CurrentPosition < num3)
		{
			NormalisedDistance = (CurrentPosition - num) / (num3 - num);
		}
		else
		{
			NormalisedDistance = 1f - (CurrentPosition - num4) / (num2 - num4);
		}
		if (CurrentState != TargetState)
		{
			NormalisedDistance *= _wrongStateMultiplier;
		}
		string localizedValue = _noiseCharsLocalization.LocalizedValue;
		for (int i = 0; i < _message.Length; i++)
		{
			if (NormalisedDistance >= _thresholdMap[i] || "!\"#$%&'()*+, -./:;<=>?@[\\]^_`{|}~—“”“”«»¡—‘’“”„…、。々《》「」！（），：？ｈ\n".Contains(_message[i]))
			{
				stringBuilder.Append(_message[i]);
			}
			else
			{
				stringBuilder.Append(localizedValue[UnityEngine.Random.Range(0, localizedValue.Length)]);
			}
		}
		return stringBuilder.ToString();
	}

	public void SetPauseState(bool isPaused)
	{
		_isPaused = isPaused;
	}

	public void ResetDay()
	{
		_isWaveFound = false;
	}

	public void SetSpeedUpStrength(float strength)
	{
		_speedUpStrength = strength;
	}
}
