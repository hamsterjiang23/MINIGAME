using System;
using System.Collections.Generic;
using _Code.Characters;

namespace _Scripts.Services.DataModel.Models.Dialogs
{
	[Serializable]
	public sealed class DialogsRamData : BaseDataStorage
	{
		public List<ECharacterType> CharacterWithWhomTalkedToday;

		public CharactersTalksCountData CharactersTalksCount;
	}
}
