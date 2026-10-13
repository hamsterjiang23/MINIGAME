using System;
using System.Runtime.CompilerServices;
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

namespace _Code.DialogSystem.Commands
{
	public sealed class DialogCommandsInstance : MonoBehaviour
	{
		private ICharactersManager _charactersManager;

		private ICharactersSODataProvider _charactersSODataProvider;

		private IGameEventsManager _gameEventsManager;

		private static DialogCommandsInstance _instance;

		[CompilerGenerated]
		private Action<EDialogEmotionState> m_EmotionChanged;

		[CompilerGenerated]
		private Action<CharacterSOData> m_CharacterExiled;

		[CompilerGenerated]
		private Action<CharacterSOData, ECharacterSign> m_SignShowed;

		[CompilerGenerated]
		private Action<ECharacterSign> m_PlayerSignShowed;

		[CompilerGenerated]
		private Action m_StoppedShowingSign;

		[CompilerGenerated]
		private Action<bool> m_GunSetUp;

		[CompilerGenerated]
		private Action m_GunShot;

		[CompilerGenerated]
		private Action m_FakedShot;

		[CompilerGenerated]
		private Action m_Dead;

		[CompilerGenerated]
		private Action m_EndingTriggered;

		[CompilerGenerated]
		private Action m_GotCat;

		[CompilerGenerated]
		private Action m_CultistsBegun;

		[CompilerGenerated]
		private Action m_CatPet;

		[CompilerGenerated]
		private Action m_CatTaken;

		[CompilerGenerated]
		private Action m_CalledFema;

		[CompilerGenerated]
		private Action m_PlayerRevealedByVigilante;

		[CompilerGenerated]
		private Action m_CultistsSaved;

		[CompilerGenerated]
		private Action<float> m_FadedIn;

		[CompilerGenerated]
		private Action<float> m_FadedOut;

		[CompilerGenerated]
		private Action<ECutscene> m_CutsceneTriggered;

		[CompilerGenerated]
		private Action<EConsumable, int> m_GotItem;

		[CompilerGenerated]
		private Action<EStateObjectType, int> m_StateChanged;

		[CompilerGenerated]
		private Action<ELocation> m_WentToLocation;

		[CompilerGenerated]
		private Action<int> m_CharacterExiledByFEMA;

		[CompilerGenerated]
		private Action<bool> m_SetFridgeActivity;

		[CompilerGenerated]
		private Action<ECharacterType, ERoomPeopleState> m_PoseChanged;

		[CompilerGenerated]
		private Action<ECharacterType, ERoomPeopleState> m_PoseChangedTomorrow;

		[CompilerGenerated]
		private Action<ECharacterType, ERoomPeopleState> m_PoseChangedAfterTomorrow;

		[CompilerGenerated]
		private Action<EConsumable, int> m_OrderedCourier;

		[CompilerGenerated]
		private Action<EPhoneSubscriber> m_UnlockedPhoneNumber;

		[CompilerGenerated]
		private Action<ESound> m_StartedWindowNoise;

		[CompilerGenerated]
		private Action<ESound> m_Sounded;

		[CompilerGenerated]
		private Action<EHUDAnimation> m_PlayedAnimation;

		[CompilerGenerated]
		private Action m_ArmpitsWashed;

		[CompilerGenerated]
		private Action m_DeficitDayUpdated;

		[CompilerGenerated]
		private Action m_UnlockedDeathEnding;

		[CompilerGenerated]
		private Action m_UnlockedMushroomEnding;

		[CompilerGenerated]
		private Action m_UnlockedKillerEnding;

		[CompilerGenerated]
		private Action m_IntroSkipped;

		[CompilerGenerated]
		private Action m_SmokeEnabled;

		[CompilerGenerated]
		private Action<ECharacterType> m_RoomKilled;

		[CompilerGenerated]
		private Func<EConsumable, int, ECharacterType, bool> m_TryGiveItem;

		[CompilerGenerated]
		private Func<EConsumable, int> m_GotItemCount;

		[CompilerGenerated]
		private Func<bool> m_HasCompletedMushroom;

		[CompilerGenerated]
		private Func<EPhoneSubscriber, string> m_GotPhoneNumber;

		[CompilerGenerated]
		private Func<bool> m_CouldOrderCourier;

		[CompilerGenerated]
		private Func<DialogCourierOrderData> m_GotCourierOrder;

		[CompilerGenerated]
		private Func<int> m_GotEnergy;

		[CompilerGenerated]
		private Func<bool> m_NeededToGetDeficitItem;

		[CompilerGenerated]
		private Func<bool> m_WasEverCompletedGame;

		[CompilerGenerated]
		private Func<EDream, bool> m_HadSeenDream;

		[CompilerGenerated]
		private Func<string> m_PeekedDeficitItem;

		[CompilerGenerated]
		private Func<ECharacterType, bool> m_BeenAlive;

		public event Action<EDialogEmotionState> EmotionChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<CharacterSOData> CharacterExiled
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<CharacterSOData, ECharacterSign> SignShowed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ECharacterSign> PlayerSignShowed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action StoppedShowingSign
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<bool> GunSetUp
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action GunShot
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action FakedShot
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action Dead
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action EndingTriggered
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action GotCat
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CultistsBegun
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CatPet
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CatTaken
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CalledFema
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action PlayerRevealedByVigilante
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action CultistsSaved
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<float> FadedIn
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<float> FadedOut
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ECutscene> CutsceneTriggered
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EConsumable, int> GotItem
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EStateObjectType, int> StateChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ELocation> WentToLocation
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<int> CharacterExiledByFEMA
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<bool> SetFridgeActivity
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ECharacterType, ERoomPeopleState> PoseChanged
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ECharacterType, ERoomPeopleState> PoseChangedTomorrow
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ECharacterType, ERoomPeopleState> PoseChangedAfterTomorrow
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EConsumable, int> OrderedCourier
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EPhoneSubscriber> UnlockedPhoneNumber
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ESound> StartedWindowNoise
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ESound> Sounded
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<EHUDAnimation> PlayedAnimation
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action ArmpitsWashed
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action DeficitDayUpdated
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action UnlockedDeathEnding
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action UnlockedMushroomEnding
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action UnlockedKillerEnding
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action IntroSkipped
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action SmokeEnabled
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Action<ECharacterType> RoomKilled
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<EConsumable, int, ECharacterType, bool> TryGiveItem
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<EConsumable, int> GotItemCount
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<bool> HasCompletedMushroom
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<EPhoneSubscriber, string> GotPhoneNumber
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<bool> CouldOrderCourier
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<DialogCourierOrderData> GotCourierOrder
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<int> GotEnergy
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<bool> NeededToGetDeficitItem
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<bool> WasEverCompletedGame
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<EDream, bool> HadSeenDream
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<string> PeekedDeficitItem
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public event Func<ECharacterType, bool> BeenAlive
		{
			[CompilerGenerated]
			add
			{
			}
			[CompilerGenerated]
			remove
			{
			}
		}

		public void Init(ICharactersManager charactersManager, ICharactersSODataProvider charactersSoDataProvider, IGameEventsManager gameEventsManager)
		{
		}

		[YarnCommand("LetIn")]
		public static void LetCharacterIn(string characterName)
		{
		}

		[YarnCommand("AggressiveLetIn")]
		public static void AggressiveLetCharacterIn(string characterName)
		{
		}

		[YarnCommand("Refuse")]
		public static void RefuseCharacter(string characterName)
		{
		}

		[YarnCommand("SetEmotion")]
		public static void SetEmotion(string rawEmotion)
		{
		}

		[YarnCommand("PerformEvent")]
		public static void PerformEvent(string eventName)
		{
		}

		[YarnCommand("Kill")]
		public static void KillCharacter(string characterName)
		{
		}

		[YarnCommand("FakeShot")]
		public static void FakeShot()
		{
		}

		[YarnCommand("KillNoGun")]
		public static void KillCharacterWithNoGun(string characterName)
		{
		}

		[YarnCommand("KillRoom")]
		public static void KillRoom(string characterName)
		{
		}

		[YarnCommand("KillTomorrow")]
		public static void KillTomorrow(string characterName)
		{
		}

		[YarnCommand("ExileByFEMA")]
		public static void ExileByFEMA(int count)
		{
		}

		[YarnCommand("ExileCharacter")]
		public static void ExileCharacter(string characterRaw, bool showInDialog = false)
		{
		}

		[YarnCommand("ShowSign")]
		public static void ShowSign(string characterRaw, string signRaw)
		{
		}

		[YarnCommand("StopShowingSign")]
		public static void StopShowingSign()
		{
		}

		[YarnCommand("SetupGun")]
		public static void SetUpGun(bool isShowed)
		{
		}

		[YarnCommand("DontSetupGun")]
		public static void DontSetUpGun()
		{
		}

		[YarnCommand("DeathBySuper")]
		public static void Death()
		{
		}

		[YarnCommand("Ending")]
		public static void Ending()
		{
		}

		[YarnCommand("Cutscene")]
		public static void PlayCutscene(string cutsceneRaw)
		{
		}

		[YarnCommand("GetItem")]
		public static void GetItem(string rawItem, int count)
		{
		}

		[YarnCommand("ChangeState")]
		public static void ChangeState(string stateableRaw, int stateIndex)
		{
		}

		[YarnCommand("FadeIn")]
		public static void FadeIn(float duration)
		{
		}

		[YarnCommand("FadeOut")]
		public static void FadeOut(float duration)
		{
		}

		[YarnCommand("GetCat")]
		public static void GetCat()
		{
		}

		[YarnCommand("GoToLocation")]
		public static void GoToLocation(string locationRaw)
		{
		}

		[YarnCommand("BeginCultists")]
		public static void BeginCultists()
		{
		}

		[YarnCommand("PetCat")]
		public static void PetCat()
		{
		}

		[YarnCommand("TakeCat")]
		public static void TakeCat()
		{
		}

		[YarnCommand("UnlockPhoneNumber")]
		public static void UnlockPhoneNumber(string subRaw)
		{
		}

		[YarnCommand("OrderCourier")]
		public static void OrderCourier(string rawConsumable, int count)
		{
		}

		[YarnCommand("StartWindowNoise")]
		public static void StartWindowNoise(string soundRaw)
		{
		}

		[YarnCommand("RefuseOldLetInNew")]
		public static void RefuseOldLetInNew(string characterRaw)
		{
		}

		[YarnCommand("CallFEMATonight")]
		public static void CallFEMATonight()
		{
		}

		[YarnCommand("ChangePose")]
		public static void ChangePose(string characterRaw, string poseRaw)
		{
		}

		[YarnCommand("ChangePoseTomorrow")]
		public static void ChangePoseTomorrow(string characterRaw, string poseRaw)
		{
		}

		[YarnCommand("ExileAfterTomorrow")]
		public static void ExileAfterTomorrow(string characterRaw)
		{
		}

		[YarnCommand("SetFridgeActiveState")]
		public static void SetFridgeActiveState(bool isActive)
		{
		}

		[YarnCommand("GetAchievement")]
		public static void GetAchievement(string achievementRaw)
		{
		}

		[YarnCommand("ShowPlayerSign")]
		public static void ShowPlayerSign(string signRaw)
		{
		}

		[YarnCommand("WashArmpits")]
		public static void WashArmpits()
		{
		}

		[YarnCommand("UpdateDeficitDay")]
		public static void UpdateDeficitDay()
		{
		}

		[YarnCommand("VigilanteRevealsPlayer")]
		public static void VigilanteRevealsPlayer()
		{
		}

		[YarnCommand("SaveCultists")]
		public static void SaveCultists()
		{
		}

		[YarnCommand("Sound")]
		public static void Sound(string rawSound)
		{
		}

		[YarnCommand("UnlockDeathEnding")]
		public static void UnlockDeathEnding()
		{
		}

		[YarnCommand("UnlockMushroomEnding")]
		public static void UnlockMushroomEnding()
		{
		}

		[YarnCommand("UnlockKillerEnding")]
		public static void UnlockKillerEnding()
		{
		}

		[YarnCommand("PlayAnimation")]
		public static void PlayAnimation(string rawAnimation)
		{
		}

		[YarnCommand("SkipIntro")]
		public static void SkipIntro()
		{
		}

		[YarnCommand("EnableSmoke")]
		public static void EnableSmoke()
		{
		}

		[YarnFunction("IsImposter")]
		public static bool IsImposter(string characterName)
		{
			return false;
		}

		[YarnFunction("TryGiveItemFor")]
		public static bool TryGiveResource(string rawItem, int count, string rawCharacter)
		{
			return false;
		}

		[YarnFunction("TryGiveItem")]
		public static bool TryGiveResource(string rawItem, int count)
		{
			return false;
		}

		[YarnFunction("GetItemCount")]
		public static int GetItemCount(string rawItem)
		{
			return 0;
		}

		[YarnFunction("HasCompletedMushroomCondition")]
		public static bool HasCompletedMushroomCondition()
		{
			return false;
		}

		[YarnFunction("GetInnocentsCount")]
		public static int GetInnocentsCount()
		{
			return 0;
		}

		[YarnFunction("GetPhoneNumber")]
		public static string GetPhoneNumber(string rawSub)
		{
			return null;
		}

		[YarnFunction("CanOrderCourier")]
		public static bool CanOrderCourier()
		{
			return false;
		}

		[YarnFunction("GetCourierOrder")]
		public static string GetCourierOrder()
		{
			return null;
		}

		[YarnFunction("IsCharacterPlaceEmpty")]
		public static bool IsCharacterPlaceEmpty(string characterRaw)
		{
			return false;
		}

		[YarnFunction("GetCharacterOnPlace")]
		public static string GetCharacterOnPlace(string characterRaw)
		{
			return null;
		}

		[YarnFunction("GetEnergy")]
		public static int GetEnergy()
		{
			return 0;
		}

		[YarnFunction("IsNeedToGetDeficitItem")]
		public static bool IsNeedToGetDeficitItem()
		{
			return false;
		}

		[YarnFunction("PeekDeficitItem")]
		public static string PeekDeficitItem()
		{
			return null;
		}

		[YarnFunction("LocalizeItem")]
		public static string LocalizeItem(string rawItem)
		{
			return null;
		}

		[YarnFunction("IsAlive")]
		public static bool IsAlive(string characterRaw)
		{
			return false;
		}

		[YarnFunction("IsNobodyHome")]
		public static bool IsNobodyHome()
		{
			return false;
		}

		[YarnFunction("IsEverCompletedGame")]
		public static bool IsEverCompletedGame()
		{
			return false;
		}

		[YarnFunction("HasSeenDream")]
		public static bool HasSeenDream(string dreamRaw)
		{
			return false;
		}
	}
}
