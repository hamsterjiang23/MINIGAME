using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Steamworks;
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
		private List<ECharacterType> _killedImposters = new List<ECharacterType>();

		[JsonProperty]
		private List<ECharacterType> _killedInnocents = new List<ECharacterType>();

		private string[] _neededTV = new string[29]
		{
			"tv_1_necessary", "tv_1_extra1", "tv_2_necessary", "tv_2_extra1", "tv_2_extra2", "tv_3_necessary", "tv_3_extra1", "tv_3_extra2", "tv_4_necessary", "tv_4_extra1",
			"tv_4_extra2", "tv_5_necessary", "tv_5_extra1", "tv_6_necessary", "tv_6_extra1", "tv_6_extra2", "tv_7_necessary", "tv_7_extra1", "tv_7_extra2", "tv_8_necessary",
			"tv_8_extra1", "tv_9_necessary", "tv_9_extra1", "tv_10_necessary", "tv_10_extra1", "tv_11_necessary", "tv_11_extra1", "tv_12_necessary", "tv_13_necessary"
		};

		private EEnding[] _allEndings = new EEnding[10]
		{
			EEnding.Basement,
			EEnding.BasementWithThem,
			EEnding.KilledBySuper,
			EEnding.Baby,
			EEnding.TheDeath,
			EEnding.Cult,
			EEnding.Killer,
			EEnding.FEMA,
			EEnding.Mushroom,
			EEnding.Rage
		};

		private ECharacterType[] _ignoreCharacters = new ECharacterType[7]
		{
			ECharacterType.TV,
			ECharacterType.Empty,
			ECharacterType.Player,
			ECharacterType.Twins,
			ECharacterType.Phone,
			ECharacterType.Sanya,
			ECharacterType.Miner
		};

		private EConsumable[] _neededConsumables = new EConsumable[6]
		{
			EConsumable.Bobeer,
			EConsumable.Cigarette,
			EConsumable.Coffee,
			EConsumable.Enerjeka,
			EConsumable.Mushroom,
			EConsumable.Kombucha
		};

		private ECharacterSign[] _ignoredSigns = new ECharacterSign[1] { ECharacterSign.AuraPhoto };

		private EPhoneSubscriber[] _ignoredSubscribers = new EPhoneSubscriber[3]
		{
			EPhoneSubscriber.None,
			EPhoneSubscriber.Daughter,
			EPhoneSubscriber.AURACAM
		};

		[JsonProperty]
		public List<int> UnseenWindows { get; private set; } = new List<int>();

		[JsonProperty]
		public bool HasCompletedLookAllWindows { get; private set; }

		[JsonProperty]
		public Dictionary<ECharacterType, int> MetCharacters { get; private set; } = new Dictionary<ECharacterType, int>();

		[JsonProperty]
		public Dictionary<EEnding, int> EndingsReachedCount { get; set; } = new Dictionary<EEnding, int>();

		[JsonProperty]
		public List<EConsumable> UsedConsumables { get; } = new List<EConsumable>();

		[JsonProperty]
		public List<ENarrativeObject> WatchedNarrativeObject { get; } = new List<ENarrativeObject>();

		[JsonProperty]
		public List<ECharacterSign> WatchedSelfSigns { get; } = new List<ECharacterSign>();

		[JsonProperty]
		public List<EPhoneSubscriber> CalledSubscribers { get; } = new List<EPhoneSubscriber>();

		[JsonProperty]
		public List<int> RadioDecryptedDays { get; } = new List<int>();

		[JsonProperty]
		public List<string> WatchedTV { get; private set; } = new List<string>();

		public void LookAtWindow(int index, int day)
		{
			int item = day * 10 + index;
			if (UnseenWindows.Contains(item))
			{
				UnseenWindows.Remove(item);
			}
			if (!HasCompletedLookAllWindows && UnseenWindows.Count == 0)
			{
				HasCompletedLookAllWindows = true;
				SteamAchievements.Unlock("WINDOWS");
			}
		}

		public void WatchTV(string dialogName)
		{
			if (!WatchedTV.Contains(dialogName))
			{
				WatchedTV.Add(dialogName);
			}
			if (_neededTV.All((string x) => WatchedTV.Contains(x)))
			{
				SteamAchievements.Unlock("ALL_TV");
			}
		}

		public void KillInnocent(ECharacterType character)
		{
			_killedInnocents.Add(character);
			CheckFrenemyAchievement(character);
			CheckShooterAchievement();
		}

		public void KillImposter(ECharacterType character)
		{
			_killedImposters.Add(character);
			CheckFrenemyAchievement(character);
			CheckShooterAchievement();
		}

		private void CheckShooterAchievement()
		{
			if (_killedImposters.Count + _killedInnocents.Count >= 30)
			{
				SteamAchievements.Unlock("SHOOTER");
			}
		}

		private void CheckFrenemyAchievement(ECharacterType character)
		{
			if (_killedImposters.Contains(character) && _killedInnocents.Contains(character))
			{
				SteamAchievements.Unlock("FRENEMY");
			}
		}

		public void CheckAchievements(EEnding currentEnding)
		{
			switch (currentEnding)
			{
			case EEnding.Basement:
				SteamAchievements.Unlock("END_LIVE_TILL_END");
				break;
			case EEnding.BasementWithThem:
				SteamAchievements.Unlock("END_IMPOSTERS");
				break;
			case EEnding.KilledBySuper:
				SteamAchievements.Unlock("END_SUPER");
				break;
			case EEnding.Baby:
				SteamAchievements.Unlock("END_FATAL_CHILD");
				break;
			case EEnding.TheDeath:
				SteamAchievements.Unlock("END_ENTRING_DEATH");
				break;
			case EEnding.Cult:
				SteamAchievements.Unlock("END_CULTISTS");
				break;
			case EEnding.Killer:
				SteamAchievements.Unlock("END_KILLED");
				break;
			case EEnding.FEMA:
				SteamAchievements.Unlock("END_FEMA");
				break;
			case EEnding.Mushroom:
				SteamAchievements.Unlock("END_MUSHROOM");
				break;
			case EEnding.Rage:
				SteamAchievements.Unlock("END_VIGILANTE");
				break;
			case EEnding.BasementAlone:
				SteamAchievements.Unlock("END_ALONE");
				break;
			}
			if (SteamManager.Initialized)
			{
				SteamUserStats.SetStat(currentEnding.ToString(), EndingsReachedCount[currentEnding]);
			}
			if (_allEndings.All((EEnding x) => EndingsReachedCount.ContainsKey(x)))
			{
				SteamAchievements.Unlock("ALL_ENDINGS");
			}
		}

		public void CheckAchievements(ECharacterType characterType)
		{
			List<ECharacterType> list = Enum.GetValues(typeof(ECharacterType)).Cast<ECharacterType>().ToList();
			list.RemoveAll((ECharacterType x) => _ignoreCharacters.Contains(x));
			if (list.All((ECharacterType x) => MetCharacters.ContainsKey(x)))
			{
				SteamAchievements.Unlock("ALL_CHARACTERS");
			}
			if (SteamManager.Initialized)
			{
				SteamUserStats.SetStat("CharactersMet", MetCharacters.Count);
			}
		}

		public void CheckAchievements(EConsumable currentEnding)
		{
			if (_neededConsumables.All((EConsumable x) => UsedConsumables.Contains(x)))
			{
				SteamAchievements.Unlock("CONSUM_TRY_EACH");
			}
		}

		public void CheckAchievements(ENarrativeObject narrativeObject)
		{
			if (Enum.GetValues(typeof(ENarrativeObject)).Cast<ENarrativeObject>().ToList()
				.All((ENarrativeObject x) => WatchedNarrativeObject.Contains(x)))
			{
				SteamAchievements.Unlock("HOUSE_INSPECTOR");
			}
		}

		public void CheckAchievements(ECharacterSign sign)
		{
			List<ECharacterSign> list = Enum.GetValues(typeof(ECharacterSign)).Cast<ECharacterSign>().ToList();
			list.RemoveAll((ECharacterSign x) => _ignoredSigns.Contains(x));
			if (list.All((ECharacterSign x) => WatchedSelfSigns.Contains(x)))
			{
				SteamAchievements.Unlock("MIRROR_SELF");
			}
		}

		public void CheckAchievements(EPhoneSubscriber subscriber)
		{
			List<EPhoneSubscriber> list = Enum.GetValues(typeof(EPhoneSubscriber)).Cast<EPhoneSubscriber>().ToList();
			list.RemoveAll((EPhoneSubscriber x) => _ignoredSubscribers.Contains(x));
			if (list.All((EPhoneSubscriber x) => CalledSubscribers.Contains(x)))
			{
				SteamAchievements.Unlock("PHONEBOOK");
			}
		}

		public void CheckAchievementsRadio()
		{
			for (int i = 1; i < 14; i++)
			{
				if (!RadioDecryptedDays.Contains(i))
				{
					return;
				}
			}
			SteamAchievements.Unlock("ALL_RADIO");
		}

		public void ResetUnseenWindows()
		{
			UnseenWindows = new List<int>
			{
				1, 2, 3, 11, 12, 13, 21, 22, 23, 31,
				32, 33, 41, 42, 43, 51, 52, 53, 61, 62,
				63, 71, 72, 73, 81, 82, 83, 91, 92, 93,
				101, 102, 103, 111, 112, 113, 121, 122, 123
			};
		}
	}
}
