using System;
using UnityEngine;
using UnityEngine.Localization;

namespace _Code.Characters
{
	[Serializable]
	public sealed class CharacterLocalizedSpriteByEmotionData : ACharacterSpriteByEmotion
	{
		[SerializeField]
		private LocalizedAsset<Sprite> _localizedSprite;

		public override Sprite[] EmotionSprites => null;
	}
}
