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

		public bool IsReady => base.Data != null;

		public MetaPrefsDataHandler(IDataStorage dataStorage)
			: base(dataStorage)
		{
		}

		protected override void LoadInner()
		{
			if (!base.Data.HasCompletedLookAllWindows && base.Data.UnseenWindows.Count == 0)
			{
				base.Data.ResetUnseenWindows();
			}
		}

		public void LookAtWindow(int index, int day)
		{
			base.Data.LookAtWindow(index, day);
			SaveData().Forget();
		}

		public void MeetCharacter(ECharacterType characterType)
		{
			if (characterType == ECharacterType.Twins)
			{
				if (!base.Data.MetCharacters.TryAdd(ECharacterType.Alkonost, 1))
				{
					base.Data.MetCharacters[ECharacterType.Alkonost]++;
				}
				if (!base.Data.MetCharacters.TryAdd(ECharacterType.Sirin, 1))
				{
					base.Data.MetCharacters[ECharacterType.Sirin]++;
				}
			}
			if (!base.Data.MetCharacters.TryAdd(characterType, 1))
			{
				base.Data.MetCharacters[characterType]++;
			}
			base.Data.CheckAchievements(characterType);
			SaveData().Forget();
		}

		public void KillImposter(ECharacterType character)
		{
			base.Data.KillImposter(character);
			SaveData().Forget();
		}

		public void KillInnocent(ECharacterType character)
		{
			base.Data.KillInnocent(character);
			SaveData().Forget();
		}

		public void ReachEnding(EEnding ending)
		{
			if (!base.Data.EndingsReachedCount.TryAdd(ending, 1))
			{
				base.Data.EndingsReachedCount[ending]++;
			}
			base.Data.CheckAchievements(ending);
			SaveData().Forget();
		}

		public int GetCompletedGameTimesCount(EGameCompleteType gameCompleteType)
		{
			return gameCompleteType switch
			{
				EGameCompleteType.AnyEnding => base.Data.EndingsReachedCount.Sum((KeyValuePair<EEnding, int> x) => x.Value), 
				EGameCompleteType.ExcludeEarlyEndings => base.Data.EndingsReachedCount.Where((KeyValuePair<EEnding, int> x) => !IsGameOverEnding(x.Key)).Sum((KeyValuePair<EEnding, int> x) => x.Value), 
			};
		}

		public List<EEnding> GetUnlockedEndings()
		{
			return (from x in base.Data.EndingsReachedCount
				where x.Value > 0
				select x.Key).Distinct().ToList();
		}

		public List<ECharacterType> GetUnlockedCharacters()
		{
			return (from x in base.Data.MetCharacters
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
			return base.Data.UsedConsumables.Contains(consumable);
		}

		public void UseConsumable(EConsumable consumable)
		{
			if (!base.Data.UsedConsumables.Contains(consumable))
			{
				base.Data.UsedConsumables.Add(consumable);
			}
			base.Data.CheckAchievements(consumable);
			SaveData().Forget();
		}

		public void WatchNarrativeObject(ENarrativeObject narrativeObject)
		{
			if (!base.Data.WatchedNarrativeObject.Contains(narrativeObject))
			{
				base.Data.WatchedNarrativeObject.Add(narrativeObject);
			}
			base.Data.CheckAchievements(narrativeObject);
			SaveData().Forget();
		}

		public void WatchSelfSign(ECharacterSign sign)
		{
			if (!base.Data.WatchedSelfSigns.Contains(sign))
			{
				base.Data.WatchedSelfSigns.Add(sign);
			}
			base.Data.CheckAchievements(sign);
			SaveData().Forget();
		}

		public void CallSubscriber(EPhoneSubscriber subscriber)
		{
			if (!base.Data.CalledSubscribers.Contains(subscriber))
			{
				base.Data.CalledSubscribers.Add(subscriber);
			}
			base.Data.CheckAchievements(subscriber);
			SaveData().Forget();
		}

		public void DecryptRadio(int day)
		{
			if (!base.Data.RadioDecryptedDays.Contains(day))
			{
				base.Data.RadioDecryptedDays.Add(day);
			}
			base.Data.CheckAchievementsRadio();
			SaveData().Forget();
		}

		public void WatchTV(string dialogName)
		{
			base.Data.WatchTV(dialogName);
			SaveData().Forget();
		}
	}
}
