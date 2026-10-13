using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Utils.Logger;

namespace _Code.Infrastructure.CloseUps.Views.Phone;

public sealed class PhoneNumbersController
{
	private enum ENumberGeneratingType
	{
		aaAaaA,
		AaaAaa,
		AaAOoO,
		AaaaaA,
		aAaaaA
	}

	private ConditionalLocalLogger _logger = new ConditionalLocalLogger();

	private CloseUpSaveData _saveData;

	private OtherGameSOData _otherGameSOData;

	public void Init(CloseUpSaveData saveData, OtherGameSOData otherGameSOData)
	{
		_saveData = saveData;
		_otherGameSOData = otherGameSOData;
	}

	public void GenerateNumbers()
	{
		EPhoneSubscriber[] array = (EPhoneSubscriber[])Enum.GetValues(typeof(EPhoneSubscriber));
		for (int i = 0; i < array.Length; i++)
		{
			EPhoneSubscriber ePhoneSubscriber = array[i];
			if (ePhoneSubscriber != EPhoneSubscriber.None)
			{
				if (_otherGameSOData.PhoneNumbers.TryGetValue(ePhoneSubscriber, out var value))
				{
					_saveData.PhoneNumbers.Add(ePhoneSubscriber, value.ToString());
					_logger.Log(ePhoneSubscriber.ToString() + " - " + _saveData.PhoneNumbers[ePhoneSubscriber]);
				}
				else
				{
					_saveData.PhoneNumbers.Add(ePhoneSubscriber, GeneratePhoneNumber(_saveData.PhoneNumbers));
					_logger.Log(ePhoneSubscriber.ToString() + " - " + _saveData.PhoneNumbers[ePhoneSubscriber]);
				}
			}
		}
	}

	private string GeneratePhoneNumber(IDictionary<EPhoneSubscriber, string> phoneNumbers)
	{
		string tempNumber;
		do
		{
			tempNumber = "";
			ENumberGeneratingType randomElement = ((ENumberGeneratingType[])Enum.GetValues(typeof(ENumberGeneratingType))).GetRandomElement();
			for (int i = 0; i < 6; i++)
			{
				if (randomElement == ENumberGeneratingType.aaAaaA && i == 5)
				{
					tempNumber += tempNumber[2];
				}
				else if (randomElement == ENumberGeneratingType.AaaAaa && i == 3)
				{
					tempNumber += tempNumber[0];
				}
				else if (randomElement == ENumberGeneratingType.AaAOoO && i == 2)
				{
					tempNumber += tempNumber[0];
				}
				else if (randomElement == ENumberGeneratingType.AaAOoO && i == 5)
				{
					tempNumber += tempNumber[3];
				}
				else if (randomElement == ENumberGeneratingType.AaaaaA && i == 5)
				{
					tempNumber += tempNumber[0];
				}
				else if (randomElement == ENumberGeneratingType.aAaaaA && i == 5)
				{
					tempNumber += tempNumber[1];
				}
				else if (i == 4 && randomElement != ENumberGeneratingType.aAaaaA && UnityEngine.Random.Range(0f, 1f) < 0.66f)
				{
					int num = int.Parse(char.ToString(tempNumber[1])) + ((UnityEngine.Random.Range(0, 2) != 1) ? 1 : (-1));
					if (num < 0)
					{
						num = 9;
					}
					if (num > 9)
					{
						num = 0;
					}
					tempNumber += num;
				}
				else
				{
					tempNumber += UnityEngine.Random.Range(0, 10);
				}
			}
		}
		while (phoneNumbers.Count((KeyValuePair<EPhoneSubscriber, string> x) => x.Value == tempNumber) > 0 || tempNumber.Substring(0, 3) == tempNumber.Substring(3, 3));
		return tempNumber;
	}

	public bool TryGetSubscriberByPhone(string number, out EPhoneSubscriber phoneSubscriber)
	{
		if (_saveData.PhoneNumbers.Count((KeyValuePair<EPhoneSubscriber, string> x) => x.Value == number) > 0)
		{
			phoneSubscriber = _saveData.PhoneNumbers.FirstOrDefault((KeyValuePair<EPhoneSubscriber, string> x) => x.Value == number).Key;
			return true;
		}
		phoneSubscriber = EPhoneSubscriber.None;
		return false;
	}

	public string GetNumberBySubscriber(EPhoneSubscriber phoneSubscriber)
	{
		if (_saveData.PhoneNumbers.ContainsKey(phoneSubscriber))
		{
			return _saveData.PhoneNumbers[phoneSubscriber];
		}
		return "000000";
	}

	public void ReinitSaveData(CloseUpSaveData saveData)
	{
		_saveData = saveData;
	}
}
