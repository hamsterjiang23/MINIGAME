using System.Linq;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.CloseUps.Views.Phone.Pins;

namespace _Code.Infrastructure._NINAH__CloseUps.Views.Phone.Pins;

public sealed class PhonePinController : MonoBehaviour
{
	[SerializeField]
	private PhonePinView[] _pins;

	[SerializeField]
	private Material[] _materials;

	[SerializeField]
	private SerializedDictionary<EPhoneSubscriber, LocalizedString> _subscribersNames;

	private CloseUpSaveData _saveData;

	public void Init(CloseUpSaveData saveData)
	{
		_saveData = saveData;
		if (_saveData.UnlockedPhoneSubscribers.Count == 0)
		{
			gameObject.SetActive(value: false);
		}
	}

	public void Create(EPhoneSubscriber phoneSubscriber, string number)
	{
		if (!_pins.Select((PhonePinView x) => x.PhoneSubscriber).Contains(phoneSubscriber))
		{
			int count = _saveData.UnlockedPhoneSubscribers.Count;
			_pins[count - 1].Create(_subscribersNames[phoneSubscriber], number, _materials.GetRandomElement(), phoneSubscriber);
			if (count > 0 && !gameObject.activeSelf)
			{
				gameObject.SetActive(value: true);
			}
		}
	}

	public void CreateFromOld(EPhoneSubscriber phoneSubscriber, string number, int index)
	{
		_pins[index].Create(_subscribersNames[phoneSubscriber], number, _materials.GetRandomElement(), phoneSubscriber);
		if (index + 1 > 0 && !gameObject.activeSelf)
		{
			gameObject.SetActive(value: true);
		}
	}

	public void ReinitSaveData(CloseUpSaveData saveData)
	{
		_saveData = saveData;
	}
}
