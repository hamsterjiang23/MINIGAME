using System;
using UnityEngine;

namespace _Code.Characters
{
	[Serializable]
	public sealed class CharacterBaseSpriteByEmotionData : ACharacterSpriteByEmotion
	{
		[SerializeField]
		private Sprite _sprite;

		public override Sprite[] EmotionSprites => null;
	}
}
