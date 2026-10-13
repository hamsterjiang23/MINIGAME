using System;
using UnityEngine;
using _Code.Characters;
using _Code.Characters.DialogSystem;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Locations;
using _Code.Infrastructure.Sound;
using _Code.Infrastructure._NINAH__Dream;
using _Code.Menues.HUD.Animations;
using _Code.Rooms;

namespace _Code.DialogSystem;

public interface IDialogManager
{
	bool IsOpened { get; }

	bool IsOpenedSubtitle { get; }

	bool EverRudeToFema { get; }

	event Action DialogStarted;

	event Action SubtitleStarted;

	event Action<bool, bool> DialogEnded;

	event Action GunShowed;

	event Action GunHidden;

	event Action GunShot;

	event Action FakedShot;

	event Action EnergyConsumed;

	event Action<string, Camera> Dead;

	event Action<ERoom> RoomKilled;

	event Action EndingTriggered;

	event Action CutsceneTriggered;

	event Action ShowedAura;

	event Action GivenPovistka;

	event Action FedCat;

	event Action<float> FadedIn;

	event Action<float> FadedOut;

	event Func<bool> HasCompletedMushroomCheck;

	event Action<ELocation> WentToLocation;

	event Action CultistsBegun;

	event Action CatPet;

	event Action CatTaken;

	event Action<EPhoneSubscriber> UnlockedPhoneSubscriber;

	event Func<EPhoneSubscriber, string> GotPhoneNumber;

	event Func<bool> CouldOrderCourier;

	event Action<EConsumable, int> OrderedCourier;

	event Action<ESound> StartedWindowNoise;

	event Action ProphetDontCheckConditionMet;

	event Action<ECharacterType, ERoomPeopleState> PoseChanged;

	event Action<ECharacterType, ERoomPeopleState> PoseChangedTomorrow;

	event Func<int> GotEnergy;

	event Action<bool> SetFridgeActivity;

	event Action BaseDialogLineShowed;

	event Action ButtonsDialogLineShowed;

	event Func<ECharacterSign, bool> PlayerSignShowed;

	event Action ArmpitsWashed;

	event Action PlayerRevealedByVigilante;

	event Action CultistsSaved;

	event Action UnlockedDeathEnding;

	event Action<EHUDAnimation> PlayedAnimation;

	event Action UnlockedMushroomEnding;

	event Action UnlockedKillerEnding;

	event Action IntroSkipped;

	event Func<EDream, bool> HadSeenDream;

	void AddOnDialogEndedAction(Action action);

	void RunDialog(CharacterSOData character, string nodeName, EDialogOverlayType overlayType = EDialogOverlayType.None, Camera camera = null, DialogViewData viewData = null, bool hideCharacter = false);

	int GetCurrentDialogIndexForCharacter(CharacterSOData character);

	void AddTalk(ECharacterType characterType);

	void ShowSubtitle(string dialogName, Camera camera = null, EDialogOverlayType overlay = EDialogOverlayType.None, bool autoskip = false);

	void ShowSubtitlePopup(EInfoMessageType messageType, float time = 2.5f);

	void ShowSubtitlePopup(string message);

	void HideSubtitle();

	void AddActionForNextDialogEnded(Action temporaryDialogAction);

	void CompleteShotAnimation();

	void UpdateTalksCount();

	void SetToLastTalk(ECharacterType character);

	void RefreshSignChecks();

	void InitializeProphetCondition(Func<int, bool> condition);

	void InitializeMushroomeaterCondition(Func<int, bool> condition);

	void InitializePriestCondition(Func<int, bool> condition);

	void InitializeGetDay(Func<int> func);

	bool IsNodeVisited(string nodeName);
}
