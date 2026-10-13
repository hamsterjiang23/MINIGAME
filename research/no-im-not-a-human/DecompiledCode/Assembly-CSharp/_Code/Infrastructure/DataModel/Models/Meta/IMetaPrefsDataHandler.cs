using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using _Code.Characters;
using _Code.Infrastructure.CloseUps.Views.Phone;
using _Code.Infrastructure.Consumables;
using _Code.Infrastructure.Endings;
using _Code.Rooms;

namespace _Code.Infrastructure.DataModel.Models.Meta;

public interface IMetaPrefsDataHandler
{
	bool IsReady { get; }

	UniTask LoadData();

	UniTask LoadLocalData();

	void LookAtWindow(int index, int day);

	void MeetCharacter(ECharacterType characterType);

	void KillImposter(ECharacterType character);

	void KillInnocent(ECharacterType character);

	void ReachEnding(EEnding ending);

	int GetCompletedGameTimesCount(EGameCompleteType gameCompleteType);

	List<EEnding> GetUnlockedEndings();

	List<ECharacterType> GetUnlockedCharacters();

	bool IsGameOverEnding(EEnding ending);

	bool IsEverUsedConsumable(EConsumable consumable);

	void UseConsumable(EConsumable consumable);

	void WatchNarrativeObject(ENarrativeObject narrativeObject);

	void WatchSelfSign(ECharacterSign sign);

	void CallSubscriber(EPhoneSubscriber subscriber);

	void DecryptRadio(int day);

	void WatchTV(string dialogName);
}
