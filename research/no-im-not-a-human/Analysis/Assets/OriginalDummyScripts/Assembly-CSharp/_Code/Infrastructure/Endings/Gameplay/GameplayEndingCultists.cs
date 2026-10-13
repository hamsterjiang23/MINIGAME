using _Code.Characters;
using _Code.DialogSystem;
using _Code.Infrastructure.StateObjects;

namespace _Code.Infrastructure.Endings.Gameplay;

public sealed class GameplayEndingCultists : AGameplayEnding
{
	private GameplayEndingManagerSaveData _saveData;

	private IStateObjectController _stateObjectController;

	private IDialogManager _dialogManager;

	public override int Priority => 1;

	public override EEnding Ending => EEnding.Cult;

	public override bool AreConditionsMet => _saveData.HasBegunCultists;

	protected override void TriggerInner()
	{
		_stateObjectController.SetState(EStateObjectType.CultistsSheets, 1);
		_dialogManager.SetToLastTalk(ECharacterType.CultistOne);
		_dialogManager.SetToLastTalk(ECharacterType.CultistTwo);
		_dialogManager.SetToLastTalk(ECharacterType.CultistThree);
	}

	public void InitModules(GameplayEndingManagerSaveData saveData, IStateObjectController stateObjectController, IDialogManager dialogManager)
	{
		_saveData = saveData;
		_stateObjectController = stateObjectController;
		_dialogManager = dialogManager;
	}

	public void BeginCultists()
	{
		_saveData.HasBegunCultists = true;
	}

	public void SaveCultists()
	{
		_saveData.SavedCultists = true;
	}

	public bool CheckCondition(int visitIndex)
	{
		return visitIndex switch
		{
			0 => true, 
			1 => true, 
			2 => _saveData.SavedCultists, 
		};
	}

	public void ReinitSaveData(GameplayEndingManagerSaveData saveData)
	{
		_saveData = saveData;
	}
}
