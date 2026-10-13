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

public sealed class MockDialogManager : IDialogManager
{
	public bool IsOpened { get; }

	public bool IsOpenedSubtitle { get; }

	public bool EverRudeToFema { get; }

	public event Action DialogStarted;

	public event Action SubtitleStarted;

	public event Action<bool, bool> DialogEnded;

	public event Action GunShowed;

	public event Action GunHidden;

	public event Action GunShot;

	public event Action FakedShot;

	public event Action EnergyConsumed;

	public event Action<string, Camera> Dead;

	public event Action<ERoom> RoomKilled;

	public event Action EndingTriggered;

	public event Action CutsceneTriggered;

	public event Action ShowedAura;

	public event Action GivenPovistka;

	public event Action FedCat;

	public event Action<float> FadedIn;

	public event Action<float> FadedOut;

	public event Func<bool> HasCompletedMushroomCheck;

	public event Action<ELocation> WentToLocation;

	public event Action CultistsBegun;

	public event Action CatPet;

	public event Action CatTaken;

	public event Action<EPhoneSubscriber> UnlockedPhoneSubscriber;

	public event Func<EPhoneSubscriber, string> GotPhoneNumber;

	public event Func<bool> CouldOrderCourier;

	public event Action<EConsumable, int> OrderedCourier;

	public event Action<ESound> StartedWindowNoise;

	public event Action ProphetDontCheckConditionMet;

	public event Action<ECharacterType, ERoomPeopleState> PoseChanged;

	public event Action<ECharacterType, ERoomPeopleState> PoseChangedTomorrow;

	public event Func<int> GotEnergy;

	public event Action<bool> SetFridgeActivity;

	public event Action BaseDialogLineShowed;

	public event Action ButtonsDialogLineShowed;

	public event Func<ECharacterSign, bool> PlayerSignShowed;

	public event Action ArmpitsWashed;

	public event Action PlayerRevealedByVigilante;

	public event Action CultistsSaved;

	public event Action UnlockedDeathEnding;

	public event Action<EHUDAnimation> PlayedAnimation;

	public event Action UnlockedMushroomEnding;

	public event Action UnlockedKillerEnding;

	public event Action IntroSkipped;

	public event Func<EDream, bool> HadSeenDream;

	public void AddOnDialogEndedAction(Action action)
	{
		throw new NotImplementedException();
	}

	public void RunDialog(CharacterSOData character, string nodeName, EDialogOverlayType overlayType = EDialogOverlayType.None, Camera camera = null, DialogViewData viewData = null, bool hideCharacter = false)
	{
		throw new NotImplementedException();
	}

	public int GetCurrentDialogIndexForCharacter(CharacterSOData character)
	{
		throw new NotImplementedException();
	}

	public void AddTalk(ECharacterType characterType)
	{
		throw new NotImplementedException();
	}

	public void ShowSubtitle(string dialogName, Camera camera = null, EDialogOverlayType overlay = EDialogOverlayType.None, bool autoskip = false)
	{
		throw new NotImplementedException();
	}

	public void ShowSubtitlePopup(EInfoMessageType messageType, float f)
	{
		throw new NotImplementedException();
	}

	public void ShowSubtitlePopup(string message)
	{
		throw new NotImplementedException();
	}

	public void HideSubtitle()
	{
		throw new NotImplementedException();
	}

	public void AddActionForNextDialogEnded(Action temporaryDialogAction)
	{
		throw new NotImplementedException();
	}

	public void CompleteShotAnimation()
	{
		throw new NotImplementedException();
	}

	public void UpdateTalksCount()
	{
		throw new NotImplementedException();
	}

	public void SetToLastTalk(ECharacterType character)
	{
		throw new NotImplementedException();
	}

	public void RefreshSignChecks()
	{
		throw new NotImplementedException();
	}

	public void InitializeProphetCondition(Func<int, bool> condition)
	{
		throw new NotImplementedException();
	}

	public void InitializeMushroomeaterCondition(Func<int, bool> condition)
	{
		throw new NotImplementedException();
	}

	public void InitializePriestCondition(Func<int, bool> condition)
	{
		throw new NotImplementedException();
	}

	public void InitializeGetDay(Func<int> func)
	{
		throw new NotImplementedException();
	}

	public bool IsNodeVisited(string nodeName)
	{
		throw new NotImplementedException();
	}
}
