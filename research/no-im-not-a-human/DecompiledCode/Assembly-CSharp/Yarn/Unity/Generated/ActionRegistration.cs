using System;
using System.CodeDom.Compiler;
using UnityEngine;
using _Code.DialogSystem.Commands;

namespace Yarn.Unity.Generated;

[GeneratedCode("YarnActionAnalyzer", "1.0.0.0")]
public class ActionRegistration
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	public static void AddRegisterFunction()
	{
		Actions.AddRegistrationMethod(RegisterActions);
	}

	[GeneratedCode("YarnActionAnalyzer", "1.0.0.0")]
	public static void RegisterActions(IActionRegistration target)
	{
		target.AddCommandHandler("LetIn", (Action<string>)DialogCommandsInstance.LetCharacterIn);
		target.AddCommandHandler("AggressiveLetIn", (Action<string>)DialogCommandsInstance.AggressiveLetCharacterIn);
		target.AddCommandHandler("Refuse", (Action<string>)DialogCommandsInstance.RefuseCharacter);
		target.AddCommandHandler("SetEmotion", (Action<string>)DialogCommandsInstance.SetEmotion);
		target.AddCommandHandler("PerformEvent", (Action<string>)DialogCommandsInstance.PerformEvent);
		target.AddCommandHandler("Kill", (Action<string>)DialogCommandsInstance.KillCharacter);
		target.AddCommandHandler("FakeShot", (Action)DialogCommandsInstance.FakeShot);
		target.AddCommandHandler("KillNoGun", (Action<string>)DialogCommandsInstance.KillCharacterWithNoGun);
		target.AddCommandHandler("KillRoom", (Action<string>)DialogCommandsInstance.KillRoom);
		target.AddCommandHandler("KillTomorrow", (Action<string>)DialogCommandsInstance.KillTomorrow);
		target.AddCommandHandler("ExileByFEMA", (Action<int>)DialogCommandsInstance.ExileByFEMA);
		target.AddCommandHandler("ExileCharacter", (Action<string, bool>)DialogCommandsInstance.ExileCharacter);
		target.AddCommandHandler("ShowSign", (Action<string, string>)DialogCommandsInstance.ShowSign);
		target.AddCommandHandler("StopShowingSign", (Action)DialogCommandsInstance.StopShowingSign);
		target.AddCommandHandler("SetupGun", (Action<bool>)DialogCommandsInstance.SetUpGun);
		target.AddCommandHandler("DontSetupGun", (Action)DialogCommandsInstance.DontSetUpGun);
		target.AddCommandHandler("DeathBySuper", (Action)DialogCommandsInstance.Death);
		target.AddCommandHandler("Ending", (Action)DialogCommandsInstance.Ending);
		target.AddCommandHandler("Cutscene", (Action<string>)DialogCommandsInstance.PlayCutscene);
		target.AddCommandHandler("GetItem", (Action<string, int>)DialogCommandsInstance.GetItem);
		target.AddCommandHandler("ChangeState", (Action<string, int>)DialogCommandsInstance.ChangeState);
		target.AddCommandHandler("FadeIn", (Action<float>)DialogCommandsInstance.FadeIn);
		target.AddCommandHandler("FadeOut", (Action<float>)DialogCommandsInstance.FadeOut);
		target.AddCommandHandler("GetCat", (Action)DialogCommandsInstance.GetCat);
		target.AddCommandHandler("GoToLocation", (Action<string>)DialogCommandsInstance.GoToLocation);
		target.AddCommandHandler("BeginCultists", (Action)DialogCommandsInstance.BeginCultists);
		target.AddCommandHandler("PetCat", (Action)DialogCommandsInstance.PetCat);
		target.AddCommandHandler("TakeCat", (Action)DialogCommandsInstance.TakeCat);
		target.AddCommandHandler("UnlockPhoneNumber", (Action<string>)DialogCommandsInstance.UnlockPhoneNumber);
		target.AddCommandHandler("OrderCourier", (Action<string, int>)DialogCommandsInstance.OrderCourier);
		target.AddCommandHandler("StartWindowNoise", (Action<string>)DialogCommandsInstance.StartWindowNoise);
		target.AddCommandHandler("RefuseOldLetInNew", (Action<string>)DialogCommandsInstance.RefuseOldLetInNew);
		target.AddCommandHandler("CallFEMATonight", (Action)DialogCommandsInstance.CallFEMATonight);
		target.AddCommandHandler("ChangePose", (Action<string, string>)DialogCommandsInstance.ChangePose);
		target.AddCommandHandler("ChangePoseTomorrow", (Action<string, string>)DialogCommandsInstance.ChangePoseTomorrow);
		target.AddCommandHandler("ExileAfterTomorrow", (Action<string>)DialogCommandsInstance.ExileAfterTomorrow);
		target.AddCommandHandler("SetFridgeActiveState", (Action<bool>)DialogCommandsInstance.SetFridgeActiveState);
		target.AddCommandHandler("GetAchievement", (Action<string>)DialogCommandsInstance.GetAchievement);
		target.AddCommandHandler("ShowPlayerSign", (Action<string>)DialogCommandsInstance.ShowPlayerSign);
		target.AddCommandHandler("WashArmpits", (Action)DialogCommandsInstance.WashArmpits);
		target.AddCommandHandler("UpdateDeficitDay", (Action)DialogCommandsInstance.UpdateDeficitDay);
		target.AddCommandHandler("VigilanteRevealsPlayer", (Action)DialogCommandsInstance.VigilanteRevealsPlayer);
		target.AddCommandHandler("SaveCultists", (Action)DialogCommandsInstance.SaveCultists);
		target.AddCommandHandler("Sound", (Action<string>)DialogCommandsInstance.Sound);
		target.AddCommandHandler("UnlockDeathEnding", (Action)DialogCommandsInstance.UnlockDeathEnding);
		target.AddCommandHandler("UnlockMushroomEnding", (Action)DialogCommandsInstance.UnlockMushroomEnding);
		target.AddCommandHandler("UnlockKillerEnding", (Action)DialogCommandsInstance.UnlockKillerEnding);
		target.AddCommandHandler("PlayAnimation", (Action<string>)DialogCommandsInstance.PlayAnimation);
		target.AddCommandHandler("SkipIntro", (Action)DialogCommandsInstance.SkipIntro);
		target.AddCommandHandler("EnableSmoke", (Action)DialogCommandsInstance.EnableSmoke);
		target.AddFunction<string, bool>("IsImposter", DialogCommandsInstance.IsImposter);
		target.AddFunction<string, int, string, bool>("TryGiveItemFor", DialogCommandsInstance.TryGiveResource);
		target.AddFunction<string, int, bool>("TryGiveItem", DialogCommandsInstance.TryGiveResource);
		target.AddFunction<string, int>("GetItemCount", DialogCommandsInstance.GetItemCount);
		target.AddFunction("HasCompletedMushroomCondition", DialogCommandsInstance.HasCompletedMushroomCondition);
		target.AddFunction("GetInnocentsCount", DialogCommandsInstance.GetInnocentsCount);
		target.AddFunction<string, string>("GetPhoneNumber", DialogCommandsInstance.GetPhoneNumber);
		target.AddFunction("CanOrderCourier", DialogCommandsInstance.CanOrderCourier);
		target.AddFunction("GetCourierOrder", DialogCommandsInstance.GetCourierOrder);
		target.AddFunction<string, bool>("IsCharacterPlaceEmpty", DialogCommandsInstance.IsCharacterPlaceEmpty);
		target.AddFunction<string, string>("GetCharacterOnPlace", DialogCommandsInstance.GetCharacterOnPlace);
		target.AddFunction("GetEnergy", DialogCommandsInstance.GetEnergy);
		target.AddFunction("IsNeedToGetDeficitItem", DialogCommandsInstance.IsNeedToGetDeficitItem);
		target.AddFunction("PeekDeficitItem", DialogCommandsInstance.PeekDeficitItem);
		target.AddFunction<string, string>("LocalizeItem", DialogCommandsInstance.LocalizeItem);
		target.AddFunction<string, bool>("IsAlive", DialogCommandsInstance.IsAlive);
		target.AddFunction("IsNobodyHome", DialogCommandsInstance.IsNobodyHome);
		target.AddFunction("IsEverCompletedGame", DialogCommandsInstance.IsEverCompletedGame);
		target.AddFunction<string, bool>("HasSeenDream", DialogCommandsInstance.HasSeenDream);
	}
}
