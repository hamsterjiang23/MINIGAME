using System.Collections.Generic;
using _Code.Characters;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.DataModel.Models.Meta;
using _Code.Infrastructure.Endings;
using _Code.Rooms;
using _Scripts.Services.DataModel.DataStorages;

namespace _Scripts.Services.DataModel.Models.Meta
{
	public sealed class MetaPrefsDataHandler : ABaseDataHandler<MetaPrefsData>, IMetaPrefsDataHandler
	{
		protected override bool UseSteamCloud => false;

		public bool IsReady => false;

		public MetaPrefsDataHandler(IDataStorage dataStorage)
			: base((IDataStorage)null)
		{
		}

		protected override void LoadInner()
		{
		}

		public void LookAtWindow(int index, int day)
		{
		}

		public void MeetCharacter(ECharacterType characterType)
		{
		}

		public void KillImposter(ECharacterType character)
		{
		}

		public void KillInnocent(ECharacterType character)
		{
		}

		public void ReachEnding(EEnding ending)
		{
		}

		public int GetCompletedGameTimesCount(EGameCompleteType gameCompleteType)
		{
			return 0;
		}

		public List<EEnding> GetUnlockedEndings()
		{
			return null;
		}

		public List<ECharacterType> GetUnlockedCharacters()
		{
			return null;
		}

		public bool IsGameOverEnding(EEnding ending)
		{
			return false;
		}

		public bool IsEverUsedConsumable(EConsumable consumable)
		{
			return false;
		}

		public void UseConsumable(EConsumable consumable)
		{
		}

		public void WatchNarrativeObject(ENarrativeObject narrativeObject)
		{
		}

		public void WatchSelfSign(ECharacterSign sign)
		{
		}

		public void CallSubscriber(EPhoneSubscriber subscriber)
		{
		}

		public void DecryptRadio(int day)
		{
		}

		public void WatchTV(string dialogName)
		{
		}
	}
}
