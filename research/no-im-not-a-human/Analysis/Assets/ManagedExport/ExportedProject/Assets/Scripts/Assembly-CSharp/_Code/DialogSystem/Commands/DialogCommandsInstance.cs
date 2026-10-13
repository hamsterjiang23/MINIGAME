using System;
using System.Linq;
using UnityEngine;
using Yarn.Unity;
using _Code.Characters;
using _Code.Infrastructure;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Cutscenes;
using _Code.Infrastructure.GameEvents;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure.StateObjects;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Menues.HUD.Animations;
using _Code.Rooms;
using _Code.Utils.Extensions;

namespace _Code.DialogSystem.Commands
{
	public sealed class DialogCommandsInstance : MonoBehaviour
	{
		private ICharactersManager _charactersManager;

		private ICharactersSODataProvider _charactersSODataProvider;

		private IGameEventsManager _gameEventsManager;

		private static DialogCommandsInstance _instance;

		public event Action<EDialogEmotionState> EmotionChanged;

		public event Action<CharacterSOData> CharacterExiled;

		public event Action<CharacterSOData, ECharacterSign> SignShowed;

		public event Action<ECharacterSign> PlayerSignShowed;

		public event Action StoppedShowingSign;

		public event Action<bool> GunSetUp;

		public event Action GunShot;

		public event Action FakedShot;

		public event Action Dead;

		public event Action EndingTriggered;

		public event Action GotCat;

		public event Action CultistsBegun;

		public event Action CatPet;

		public event Action CatTaken;

		public event Action CalledFema;

		public event Action PlayerRevealedByVigilante;

		public event Action CultistsSaved;

		public event Action<float> FadedIn;

		public event Action<float> FadedOut;

		public event Action<ECutscene> CutsceneTriggered;

		public event Action<EConsumable, int> GotItem;

		public event Action<EStateObjectType, int> StateChanged;

		public event Action<ELocation> WentToLocation;

		public event Action<int> CharacterExiledByFEMA;

		public event Action<bool> SetFridgeActivity;

		public event Action<ECharacterType, ERoomPeopleState> PoseChanged;

		public event Action<ECharacterType, ERoomPeopleState> PoseChangedTomorrow;

		public event Action<ECharacterType, ERoomPeopleState> PoseChangedAfterTomorrow;

		public event Action<EConsumable, int> OrderedCourier;

		public event Action<EPhoneSubscriber> UnlockedPhoneNumber;

		public event Action<ESound> StartedWindowNoise;

		public event Action<ESound> Sounded;

		public event Action<EHUDAnimation> PlayedAnimation;

		public event Action ArmpitsWashed;

		public event Action DeficitDayUpdated;

		public event Action UnlockedDeathEnding;

		public event Action UnlockedMushroomEnding;

		public event Action UnlockedKillerEnding;

		public event Action IntroSkipped;

		public event Action SmokeEnabled;

		public event Action<ECharacterType> RoomKilled;

		public event Func<EConsumable, int, ECharacterType, bool> TryGiveItem;

		public event Func<EConsumable, int> GotItemCount;

		public event Func<bool> HasCompletedMushroom;

		public event Func<EPhoneSubscriber, string> GotPhoneNumber;

		public event Func<bool> CouldOrderCourier;

		public event Func<DialogCourierOrderData> GotCourierOrder;

		public event Func<int> GotEnergy;

		public event Func<bool> NeededToGetDeficitItem;

		public event Func<bool> WasEverCompletedGame;

		public event Func<EDream, bool> HadSeenDream;

		public event Func<string> PeekedDeficitItem;

		public event Func<ECharacterType, bool> BeenAlive;

		public void Init(ICharactersManager charactersManager, ICharactersSODataProvider charactersSoDataProvider, IGameEventsManager gameEventsManager)
		{
			_instance = this;
			_charactersManager = charactersManager;
			_charactersSODataProvider = charactersSoDataProvider;
			_gameEventsManager = gameEventsManager;
		}

		[YarnCommand("LetIn")]
		public static void LetCharacterIn(string characterName)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => x.CharacterType.ToString() == characterName);
			if (characterSOData == null)
			{
				Debug.LogError("There is no character with name " + characterName);
			}
			else
			{
				_instance._charactersManager.LetIn(characterSOData);
			}
		}

		[YarnCommand("AggressiveLetIn")]
		public static void AggressiveLetCharacterIn(string characterName)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => x.CharacterType.ToString() == characterName);
			if (characterSOData == null)
			{
				Debug.LogError("There is no character with name " + characterName);
			}
			else
			{
				_instance._charactersManager.LetIn(characterSOData, exileOldCharacterFromThisPlace: true);
			}
		}

		[YarnCommand("Refuse")]
		public static void RefuseCharacter(string characterName)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => x.CharacterType.ToString() == characterName);
			if (characterSOData == null)
			{
				Debug.LogError("There is no character with name " + characterName);
			}
			else
			{
				_instance._charactersManager.Refuse(characterSOData);
			}
		}

		[YarnCommand("SetEmotion")]
		public static void SetEmotion(string rawEmotion)
		{
			EDialogEmotionState obj = Enum.Parse<EDialogEmotionState>(rawEmotion, ignoreCase: true);
			_instance.EmotionChanged?.Invoke(obj);
		}

		[YarnCommand("PerformEvent")]
		public static void PerformEvent(string eventName)
		{
			_instance._gameEventsManager.CompleteEvent(eventName);
		}

		[YarnCommand("Kill")]
		public static void KillCharacter(string characterName)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => string.Equals(x.CharacterType.ToString(), characterName, StringComparison.InvariantCultureIgnoreCase));
			if (characterSOData == null)
			{
				throw new Exception("Can't kill " + characterName + ", it doesn't exist");
			}
			_instance.GunShot?.Invoke();
			_instance._charactersManager.Kill(characterSOData, isByPlayer: true);
		}

		[YarnCommand("FakeShot")]
		public static void FakeShot()
		{
			_instance.FakedShot?.Invoke();
		}

		[YarnCommand("KillNoGun")]
		public static void KillCharacterWithNoGun(string characterName)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => string.Equals(x.CharacterType.ToString(), characterName, StringComparison.InvariantCultureIgnoreCase));
			if (characterSOData == null)
			{
				throw new Exception("Can't kill " + characterName + ", it doesn't exist");
			}
			_instance._charactersManager.Kill(characterSOData);
		}

		[YarnCommand("KillRoom")]
		public static void KillRoom(string characterName)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => string.Equals(x.CharacterType.ToString(), characterName, StringComparison.InvariantCultureIgnoreCase));
			if (characterSOData == null)
			{
				throw new Exception("Can't kill room of " + characterName + ", they doesn't exist");
			}
			_instance._charactersManager.KillRoom(characterSOData);
			_instance.RoomKilled(characterSOData.CharacterType);
		}

		[YarnCommand("KillTomorrow")]
		public static void KillTomorrow(string characterName)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => string.Equals(x.CharacterType.ToString(), characterName, StringComparison.InvariantCultureIgnoreCase));
			if (characterSOData == null)
			{
				throw new Exception("Can't kill " + characterName + " tomorrow, it doesn't exist");
			}
			_instance._charactersManager.KillTomorrow(characterSOData);
		}

		[YarnCommand("ExileByFEMA")]
		public static void ExileByFEMA(int count)
		{
			_instance.CharacterExiledByFEMA?.Invoke(count);
		}

		[YarnCommand("ExileCharacter")]
		public static void ExileCharacter(string characterRaw, bool showInDialog = false)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => string.Equals(x.CharacterType.ToString(), characterRaw, StringComparison.InvariantCultureIgnoreCase));
			_instance._charactersManager.ExileCharacter(characterSOData.CharacterType);
			if (showInDialog)
			{
				_instance.CharacterExiled?.Invoke(characterSOData);
			}
		}

		[YarnCommand("ShowSign")]
		public static void ShowSign(string characterRaw, string signRaw)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => string.Equals(x.CharacterType.ToString(), characterRaw, StringComparison.InvariantCultureIgnoreCase));
			if (characterSOData == null)
			{
				throw new Exception("Can't check " + characterRaw + ", it doesn't exist");
			}
			ECharacterSign arg = Enum.Parse<ECharacterSign>(signRaw, ignoreCase: true);
			_instance.SignShowed?.Invoke(characterSOData, arg);
		}

		[YarnCommand("StopShowingSign")]
		public static void StopShowingSign()
		{
			_instance.StoppedShowingSign?.Invoke();
		}

		[YarnCommand("SetupGun")]
		public static void SetUpGun(bool isShowed)
		{
			if (isShowed)
			{
				_instance._charactersManager.OnGunStateSelected(b: true);
			}
			_instance.GunSetUp?.Invoke(isShowed);
		}

		[YarnCommand("DontSetupGun")]
		public static void DontSetUpGun()
		{
			_instance._charactersManager.OnGunStateSelected(b: false);
		}

		[YarnCommand("DeathBySuper")]
		public static void Death()
		{
			_instance.Dead?.Invoke();
		}

		[YarnCommand("Ending")]
		public static void Ending()
		{
			_instance.EndingTriggered?.Invoke();
		}

		[YarnCommand("Cutscene")]
		public static void PlayCutscene(string cutsceneRaw)
		{
			ECutscene obj = Enum.Parse<ECutscene>(cutsceneRaw, ignoreCase: true);
			_instance.CutsceneTriggered?.Invoke(obj);
		}

		[YarnCommand("GetItem")]
		public static void GetItem(string rawItem, int count)
		{
			EConsumable arg = Enum.Parse<EConsumable>(rawItem, ignoreCase: true);
			_instance.GotItem?.Invoke(arg, count);
		}

		[YarnCommand("ChangeState")]
		public static void ChangeState(string stateableRaw, int stateIndex)
		{
			EStateObjectType arg = Enum.Parse<EStateObjectType>(stateableRaw, ignoreCase: true);
			_instance.StateChanged?.Invoke(arg, stateIndex);
		}

		[YarnCommand("FadeIn")]
		public static void FadeIn(float duration)
		{
			_instance.FadedIn?.Invoke(duration);
		}

		[YarnCommand("FadeOut")]
		public static void FadeOut(float duration)
		{
			_instance.FadedOut?.Invoke(duration);
		}

		[YarnCommand("GetCat")]
		public static void GetCat()
		{
			_instance.GotCat?.Invoke();
		}

		[YarnCommand("GoToLocation")]
		public static void GoToLocation(string locationRaw)
		{
			ELocation obj = Enum.Parse<ELocation>(locationRaw, ignoreCase: true);
			_instance.WentToLocation?.Invoke(obj);
		}

		[YarnCommand("BeginCultists")]
		public static void BeginCultists()
		{
			_instance.CultistsBegun?.Invoke();
		}

		[YarnCommand("PetCat")]
		public static void PetCat()
		{
			_instance.CatPet?.Invoke();
		}

		[YarnCommand("TakeCat")]
		public static void TakeCat()
		{
			_instance.CatTaken?.Invoke();
		}

		[YarnCommand("UnlockPhoneNumber")]
		public static void UnlockPhoneNumber(string subRaw)
		{
			EPhoneSubscriber obj = Enum.Parse<EPhoneSubscriber>(subRaw, ignoreCase: true);
			_instance.UnlockedPhoneNumber?.Invoke(obj);
		}

		[YarnCommand("OrderCourier")]
		public static void OrderCourier(string rawConsumable, int count)
		{
			EConsumable arg = Enum.Parse<EConsumable>(rawConsumable, ignoreCase: true);
			_instance.OrderedCourier?.Invoke(arg, count);
		}

		[YarnCommand("StartWindowNoise")]
		public static void StartWindowNoise(string soundRaw)
		{
			ESound obj = Enum.Parse<ESound>(soundRaw, ignoreCase: true);
			_instance.StartedWindowNoise?.Invoke(obj);
		}

		[YarnCommand("RefuseOldLetInNew")]
		public static void RefuseOldLetInNew(string characterRaw)
		{
			ECharacterType character = Enum.Parse<ECharacterType>(characterRaw, ignoreCase: true);
			CharacterSOData characterData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => x.CharacterType == character);
			ExileCharacter(_instance._charactersManager.CharactersInside.FirstOrDefault((ECharacterType x) => _instance._charactersManager.GetPlace(x) == characterData?.Place).ToString(), showInDialog: true);
			LetCharacterIn(characterRaw);
		}

		[YarnCommand("CallFEMATonight")]
		public static void CallFEMATonight()
		{
			_instance.CalledFema?.Invoke();
		}

		[YarnCommand("ChangePose")]
		public static void ChangePose(string characterRaw, string poseRaw)
		{
			ECharacterType arg = Enum.Parse<ECharacterType>(characterRaw, ignoreCase: true);
			ERoomPeopleState arg2 = Enum.Parse<ERoomPeopleState>(poseRaw, ignoreCase: true);
			_instance.PoseChanged?.Invoke(arg, arg2);
		}

		[YarnCommand("ChangePoseTomorrow")]
		public static void ChangePoseTomorrow(string characterRaw, string poseRaw)
		{
			ECharacterType arg = Enum.Parse<ECharacterType>(characterRaw, ignoreCase: true);
			ERoomPeopleState arg2 = Enum.Parse<ERoomPeopleState>(poseRaw, ignoreCase: true);
			_instance.PoseChangedTomorrow?.Invoke(arg, arg2);
		}

		[YarnCommand("ExileAfterTomorrow")]
		public static void ExileAfterTomorrow(string characterRaw)
		{
			CharacterSOData characterSOData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => string.Equals(x.CharacterType.ToString(), characterRaw, StringComparison.InvariantCultureIgnoreCase));
			if (characterSOData == null)
			{
				throw new Exception("Can't kill " + characterRaw + " after tomorrow, it doesn't exist");
			}
			_instance._charactersManager.ExileAfterTomorrow(characterSOData);
		}

		[YarnCommand("SetFridgeActiveState")]
		public static void SetFridgeActiveState(bool isActive)
		{
			_instance.SetFridgeActivity?.Invoke(isActive);
		}

		[YarnCommand("GetAchievement")]
		public static void GetAchievement(string achievementRaw)
		{
			SteamAchievements.Unlock(achievementRaw);
		}

		[YarnCommand("ShowPlayerSign")]
		public static void ShowPlayerSign(string signRaw)
		{
			ECharacterSign obj = Enum.Parse<ECharacterSign>(signRaw, ignoreCase: true);
			_instance.PlayerSignShowed?.Invoke(obj);
		}

		[YarnCommand("WashArmpits")]
		public static void WashArmpits()
		{
			_instance.ArmpitsWashed?.Invoke();
		}

		[YarnCommand("UpdateDeficitDay")]
		public static void UpdateDeficitDay()
		{
			_instance.DeficitDayUpdated?.Invoke();
		}

		[YarnCommand("VigilanteRevealsPlayer")]
		public static void VigilanteRevealsPlayer()
		{
			_instance.PlayerRevealedByVigilante?.Invoke();
		}

		[YarnCommand("SaveCultists")]
		public static void SaveCultists()
		{
			_instance.CultistsSaved?.Invoke();
		}

		[YarnCommand("Sound")]
		public static void Sound(string rawSound)
		{
			ESound obj = Enum.Parse<ESound>(rawSound, ignoreCase: true);
			_instance.Sounded?.Invoke(obj);
		}

		[YarnCommand("UnlockDeathEnding")]
		public static void UnlockDeathEnding()
		{
			_instance.UnlockedDeathEnding?.Invoke();
		}

		[YarnCommand("UnlockMushroomEnding")]
		public static void UnlockMushroomEnding()
		{
			_instance.UnlockedMushroomEnding?.Invoke();
		}

		[YarnCommand("UnlockKillerEnding")]
		public static void UnlockKillerEnding()
		{
			_instance.UnlockedKillerEnding?.Invoke();
		}

		[YarnCommand("PlayAnimation")]
		public static void PlayAnimation(string rawAnimation)
		{
			EHUDAnimation obj = Enum.Parse<EHUDAnimation>(rawAnimation, ignoreCase: true);
			_instance.PlayedAnimation?.Invoke(obj);
		}

		[YarnCommand("SkipIntro")]
		public static void SkipIntro()
		{
			_instance.IntroSkipped?.Invoke();
		}

		[YarnCommand("EnableSmoke")]
		public static void EnableSmoke()
		{
			_instance.SmokeEnabled?.Invoke();
		}

		[YarnFunction("IsImposter")]
		public static bool IsImposter(string characterName)
		{
			return _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => x.CharacterType.ToString() == characterName)?.IsImposter ?? false;
		}

		[YarnFunction("TryGiveItemFor")]
		public static bool TryGiveResource(string rawItem, int count, string rawCharacter)
		{
			EConsumable arg = Enum.Parse<EConsumable>(rawItem, ignoreCase: true);
			ECharacterType arg2 = Enum.Parse<ECharacterType>(rawCharacter, ignoreCase: true);
			return _instance.TryGiveItem?.Invoke(arg, count, arg2) ?? false;
		}

		[YarnFunction("TryGiveItem")]
		public static bool TryGiveResource(string rawItem, int count)
		{
			EConsumable arg = Enum.Parse<EConsumable>(rawItem, ignoreCase: true);
			return _instance.TryGiveItem?.Invoke(arg, count, ECharacterType.Sanya) ?? false;
		}

		[YarnFunction("GetItemCount")]
		public static int GetItemCount(string rawItem)
		{
			EConsumable arg = Enum.Parse<EConsumable>(rawItem, ignoreCase: true);
			return _instance.GotItemCount?.Invoke(arg) ?? 0;
		}

		[YarnFunction("HasCompletedMushroomCondition")]
		public static bool HasCompletedMushroomCondition()
		{
			return _instance.HasCompletedMushroom?.Invoke() ?? false;
		}

		[YarnFunction("GetInnocentsCount")]
		public static int GetInnocentsCount()
		{
			return _instance._charactersManager.CharactersInside.Count((ECharacterType x) => !_instance._charactersManager.IsImposter(x));
		}

		[YarnFunction("GetPhoneNumber")]
		public static string GetPhoneNumber(string rawSub)
		{
			EPhoneSubscriber arg = Enum.Parse<EPhoneSubscriber>(rawSub, ignoreCase: true);
			Debug.Log("0 - " + _instance.GotPhoneNumber?.Invoke(arg));
			return _instance.GotPhoneNumber?.Invoke(arg);
		}

		[YarnFunction("CanOrderCourier")]
		public static bool CanOrderCourier()
		{
			return _instance.CouldOrderCourier?.Invoke() ?? false;
		}

		[YarnFunction("GetCourierOrder")]
		public static string GetCourierOrder()
		{
			DialogCourierOrderData dialogCourierOrderData = _instance.GotCourierOrder?.Invoke() ?? new DialogCourierOrderData(EConsumable.Bobeer, 1);
			return ("consumable_" + dialogCourierOrderData.Consumable.ToString().ToLower() + "_name").GetLocalization("UI") + $" (x{dialogCourierOrderData.Count})";
		}

		[YarnFunction("IsCharacterPlaceEmpty")]
		public static bool IsCharacterPlaceEmpty(string characterRaw)
		{
			ECharacterType character = Enum.Parse<ECharacterType>(characterRaw, ignoreCase: true);
			CharacterSOData characterData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => x.CharacterType == character);
			return _instance._charactersManager.CharactersInside.Count((ECharacterType x) => _instance._charactersManager.GetPlace(x) == characterData?.Place) == 0;
		}

		[YarnFunction("GetCharacterOnPlace")]
		public static string GetCharacterOnPlace(string characterRaw)
		{
			ECharacterType character = Enum.Parse<ECharacterType>(characterRaw, ignoreCase: true);
			CharacterSOData characterData = _instance._charactersSODataProvider.CharactersData.FirstOrDefault((CharacterSOData x) => x.CharacterType == character);
			return ("characterName_" + _instance._charactersManager.CharactersInside.FirstOrDefault((ECharacterType x) => _instance._charactersManager.GetPlace(x) == characterData?.Place)).GetLocalization("UI");
		}

		[YarnFunction("GetEnergy")]
		public static int GetEnergy()
		{
			return _instance.GotEnergy?.Invoke() ?? 0;
		}

		[YarnFunction("IsNeedToGetDeficitItem")]
		public static bool IsNeedToGetDeficitItem()
		{
			return _instance.NeededToGetDeficitItem?.Invoke() ?? false;
		}

		[YarnFunction("PeekDeficitItem")]
		public static string PeekDeficitItem()
		{
			return _instance.PeekedDeficitItem?.Invoke() ?? "";
		}

		[YarnFunction("LocalizeItem")]
		public static string LocalizeItem(string rawItem)
		{
			return ("consumable_" + rawItem.ToLower() + "_name").GetLocalization("UI");
		}

		[YarnFunction("IsAlive")]
		public static bool IsAlive(string characterRaw)
		{
			ECharacterType characterType = Enum.Parse<ECharacterType>(characterRaw, ignoreCase: true);
			return _instance._charactersManager.IsCharacterAliveInside(characterType);
		}

		[YarnFunction("IsNobodyHome")]
		public static bool IsNobodyHome()
		{
			return _instance._charactersManager.CharactersInside.Count == 0;
		}

		[YarnFunction("IsEverCompletedGame")]
		public static bool IsEverCompletedGame()
		{
			return _instance.WasEverCompletedGame?.Invoke() ?? false;
		}

		[YarnFunction("HasSeenDream")]
		public static bool HasSeenDream(string dreamRaw)
		{
			EDream arg = Enum.Parse<EDream>(dreamRaw, ignoreCase: true);
			return _instance.HadSeenDream?.Invoke(arg) ?? false;
		}
	}
}
