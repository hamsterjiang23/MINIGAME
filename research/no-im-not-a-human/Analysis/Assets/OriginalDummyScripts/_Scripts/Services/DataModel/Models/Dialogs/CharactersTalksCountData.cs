using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using _Code.Characters;

namespace _Scripts.Services.DataModel.Models.Dialogs
{
	[Serializable]
	public sealed class CharactersTalksCountData
	{
		[JsonProperty]
		private Dictionary<ECharacterType, int> _charactersTalksCount;

		[JsonProperty]
		private Dictionary<ECharacterType, int> _charactersMaxTalksCount;

		public void AddTalk(ECharacterType characterType)
		{
		}

		public int GetTalksCount(ECharacterType characterType)
		{
			return 0;
		}

		public void SetToLast(ECharacterType characterType)
		{
		}

		public void SetMaxTalks(ECharacterType characterType, int count)
		{
		}
	}
}
