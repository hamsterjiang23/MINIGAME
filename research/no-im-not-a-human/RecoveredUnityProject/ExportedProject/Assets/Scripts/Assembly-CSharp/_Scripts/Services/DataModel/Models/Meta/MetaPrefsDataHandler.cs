using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
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
		protected override bool UseSteamCloud => true;

		public bool IsReady => Data != null;

		public MetaPrefsDataHandler(IDataStorage dataStorage)
			: base(dataStorage)
		{
		}

		protected override void LoadInner()
		{
			if (!Data.HasCompletedLookAllWindows && Data.UnseenWindows.Count == 0)
			{
				Data.ResetUnseenWindows();
			}
		}

		public void LookAtWindow(int index, int day)
		{
			Data.LookAtWindow(index, day);
			SaveData().Forget();
		}

		public void MeetCharacter(ECharacterType characterType)
		{
			if (characterType == ECharacterType.Twins)
			{
				if (!Data.MetCharacters.TryAdd(ECharacterType.Alkonost, 1))
				{
					Data.MetCharacters[ECharacterType.Alkonost]++;
				}
				if (!Data.MetCharacters.TryAdd(ECharacterType.Sirin, 1))
				{
					Data.MetCharacters[ECharacterType.Sirin]++;
				}
			}
			if (!Data.MetCharacters.TryAdd(characterType, 1))
			{
				Data.MetCharacters[characterType]++;
			}
			Data.CheckAchievements(characterType);
			SaveData().Forget();
		}

		public void KillImposter(ECharacterType character)
		{
			Data.KillImposter(character);
			SaveData().Forget();
		}

		public void KillInnocent(ECharacterType character)
		{
			Data.KillInnocent(character);
			SaveData().Forget();
		}

		public void ReachEnding(EEnding ending)
		{
			if (!Data.EndingsReachedCount.TryAdd(ending, 1))
			{
				Data.EndingsReachedCount[ending]++;
			}
			Data.CheckAchievements(ending);
			SaveData().Forget();
		}

		public int GetCompletedGameTimesCount(EGameCompleteType gameCompleteType)
		{
			return gameCompleteType switch
			{
				EGameCompleteType.AnyEnding => Data.EndingsReachedCount.Sum((KeyValuePair<EEnding, int> x) => x.Value), 
				EGameCompleteType.ExcludeEarlyEndings => Data.EndingsReachedCount.Where((KeyValuePair<EEnding, int> x) => !IsGameOverEnding(x.Key)).Sum((KeyValuePair<EEnding, int> x) => x.Value), 
			};
		}

		public List<EEnding> GetUnlockedEndings()
		{
			return (from x in Data.EndingsReachedCount
				where x.Value > 0
				select x.Key).Distinct().ToList();
		}

		public List<ECharacterType> GetUnlockedCharacters()
		{
			return (from x in Data.MetCharacters
				where x.Value > 0
				select x.Key).Distinct().ToList();
		}

		public bool IsGameOverEnding(EEnding ending)
		{
			if (ending != EEnding.Killer && ending != EEnding.Rage && ending != EEnding.KilledBySuper)
			{
				return ending == EEnding.Intro;
			}
			return true;
		}

		public bool IsEverUsedConsumable(EConsumable consumable)
		{
			return Data.UsedConsumables.Contains(consumable);
		}

		public void UseConsumable(EConsumable consumable)
		{
			if (!Data.UsedConsumables.Contains(consumable))
			{
				Data.UsedConsumables.Add(consumable);
			}
			Data.CheckAchievements(consumable);
			SaveData().Forget();
		}

		public void WatchNarrativeObject(ENarrativeObject narrativeObject)
		{
			if (!Data.WatchedNarrativeObject.Contains(narrativeObject))
			{
				Data.WatchedNarrativeObject.Add(narrativeObject);
			}
			Data.CheckAchievements(narrativeObject);
			SaveData().Forget();
		}

		public void WatchSelfSign(ECharacterSign sign)
		{
			if (!Data.WatchedSelfSigns.Contains(sign))
			{
				Data.WatchedSelfSigns.Add(sign);
			}
			Data.CheckAchievements(sign);
			SaveData().Forget();
		}

		public void CallSubscriber(EPhoneSubscriber subscriber)
		{
			if (!Data.CalledSubscribers.Contains(subscriber))
			{
				Data.CalledSubscribers.Add(subscriber);
			}
			Data.CheckAchievements(subscriber);
			SaveData().Forget();
		}

		public void DecryptRadio(int day)
		{
			if (!Data.RadioDecryptedDays.Contains(day))
			{
				Data.RadioDecryptedDays.Add(day);
			}
			Data.CheckAchievementsRadio();
			SaveData().Forget();
		}

		public void WatchTV(string dialogName)
		{
			Data.WatchTV(dialogName);
			SaveData().Forget();
		}
	}
}
