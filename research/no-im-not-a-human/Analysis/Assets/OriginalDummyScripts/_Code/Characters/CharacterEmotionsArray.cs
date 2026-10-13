using System;
using System.Collections.Generic;

namespace _Code.Characters
{
	[Serializable]
	public sealed class CharacterEmotionsArray
	{
		private List<ACharacterSpriteByEmotion> _emotions;

		public IReadOnlyList<ACharacterSpriteByEmotion> Emotions => null;

		public void AddBase()
		{
		}

		public void AddLocalized()
		{
		}

		public void AddAnimated()
		{
		}

		public void RemoveEmotion(EDialogEmotionState emotionState)
		{
		}
	}
}
