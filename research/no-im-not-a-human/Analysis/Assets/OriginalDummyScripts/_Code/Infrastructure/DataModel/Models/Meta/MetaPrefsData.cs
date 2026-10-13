using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using _Code.Characters;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Endings;
using _Code.Rooms;
using _Scripts.Services.DataModel.Models;

namespace _Code.Infrastructure.DataModel.Models.Meta
{
	[Serializable]
	public sealed class MetaPrefsData : BaseDataStorage
	{
		[JsonProperty]
		private List<ECharacterType> _killedImposters;

		[JsonProperty]
		private List<ECharacterType> _killedInnocents;

		private string[] _neededTV;

		private EEnding[] _allEndings;

		private ECharacterType[] _ignoreCharacters;

		private EConsumable[] _neededConsumables;

		private ECharacterSign[] _ignoredSigns;

		private EPhoneSubscriber[] _ignoredSubscribers;

		[JsonProperty]
		public List<int> UnseenWindows { get; private set; }

		[JsonProperty]
		public bool HasCompletedLookAllWindows { get; private set; }

		[JsonProperty]
		public Dictionary<ECharacterType, int> MetCharacters { get; private set; }

		[JsonProperty]
		public Dictionary<EEnding, int> EndingsReachedCount { get; set; }

		[JsonProperty]
		public List<EConsumable> UsedConsumables { get; }

		[JsonProperty]
		public List<ENarrativeObject> WatchedNarrativeObject { get; }

		[JsonProperty]
		public List<ECharacterSign> WatchedSelfSigns { get; }

		[JsonProperty]
		public List<EPhoneSubscriber> CalledSubscribers { get; }

		[JsonProperty]
		public List<int> RadioDecryptedDays { get; }

		[JsonProperty]
		public List<string> WatchedTV { get; private set; }

		public void LookAtWindow(int index, int day)
		{
		}

		public void WatchTV(string dialogName)
		{
		}

		public void KillInnocent(ECharacterType character)
		{
		}

		public void KillImposter(ECharacterType character)
		{
		}

		private void CheckShooterAchievement()
		{
		}

		private void CheckFrenemyAchievement(ECharacterType character)
		{
		}

		public void CheckAchievements(EEnding currentEnding)
		{
		}

		public void CheckAchievements(ECharacterType characterType)
		{
		}

		public void CheckAchievements(EConsumable currentEnding)
		{
		}

		public void CheckAchievements(ENarrativeObject narrativeObject)
		{
		}

		public void CheckAchievements(ECharacterSign sign)
		{
		}

		public void CheckAchievements(EPhoneSubscriber subscriber)
		{
		}

		public void CheckAchievementsRadio()
		{
		}

		public void ResetUnseenWindows()
		{
		}
	}
}
