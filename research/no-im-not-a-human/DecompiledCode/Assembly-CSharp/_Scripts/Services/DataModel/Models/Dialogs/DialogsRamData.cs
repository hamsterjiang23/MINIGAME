using System;
using System.Collections.Generic;
using _Code.Characters;

namespace _Scripts.Services.DataModel.Models.Dialogs;

[Serializable]
public sealed class DialogsRamData : BaseDataStorage
{
	public List<ECharacterType> CharacterWithWhomTalkedToday = new List<ECharacterType>();

	public CharactersTalksCountData CharactersTalksCount = new CharactersTalksCountData();
}
