using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.Localization;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.CloseUps.Views.Phone.Pins;

namespace _Code.Infrastructure._NINAH__CloseUps.Views.Phone.Pins
{
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
		}

		public void Create(EPhoneSubscriber phoneSubscriber, string number)
		{
		}

		public void CreateFromOld(EPhoneSubscriber phoneSubscriber, string number, int index)
		{
		}

		public void ReinitSaveData(CloseUpSaveData saveData)
		{
		}
	}
}
