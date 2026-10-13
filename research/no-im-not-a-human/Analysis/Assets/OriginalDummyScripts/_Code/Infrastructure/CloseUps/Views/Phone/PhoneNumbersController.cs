using System.Collections.Generic;
using _Code.Infrastructure.OtherGameData;
using _Code.Infrastructure._NINAH__CloseUps;
using _Code.Utils.Logger;

namespace _Code.Infrastructure.CloseUps.Views.Phone
{
	public sealed class PhoneNumbersController
	{
		private enum ENumberGeneratingType
		{
			aaAaaA = 0,
			AaaAaa = 1,
			AaAOoO = 2,
			AaaaaA = 3,
			aAaaaA = 4
		}

		private ConditionalLocalLogger _logger;

		private CloseUpSaveData _saveData;

		private OtherGameSOData _otherGameSOData;

		public void Init(CloseUpSaveData saveData, OtherGameSOData otherGameSOData)
		{
		}

		public void GenerateNumbers()
		{
		}

		private string GeneratePhoneNumber(IDictionary<EPhoneSubscriber, string> phoneNumbers)
		{
			return null;
		}

		public bool TryGetSubscriberByPhone(string number, out EPhoneSubscriber phoneSubscriber)
		{
			phoneSubscriber = default;
			return false;
		}

		public string GetNumberBySubscriber(EPhoneSubscriber phoneSubscriber)
		{
			return null;
		}

		public void ReinitSaveData(CloseUpSaveData saveData)
		{
		}
	}
}
