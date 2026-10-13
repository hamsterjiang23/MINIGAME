using System;
using RoboRyanTron.SearchableEnum;
using UnityEngine;

namespace _Code.Characters
{
	[Serializable]
	public abstract class ACharacterSpriteByEmotion
	{
		[SerializeReference]
		[SerializeField]
		[SearchableEnum]
		private EDialogEmotionState _emotion;

		public EDialogEmotionState Emotion => _emotion;

		public abstract Sprite[] EmotionSprites { get; }
	}
}
