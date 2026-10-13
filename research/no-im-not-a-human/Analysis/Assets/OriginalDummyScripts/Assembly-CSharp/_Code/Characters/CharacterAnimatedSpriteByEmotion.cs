using System;
using UnityEngine;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Characters;

[Serializable]
public sealed class CharacterAnimatedSpriteByEmotion : ACharacterSpriteByEmotion
{
	[SerializeField]
	private AnimationData _animationData;

	public AnimationData AnimationData => _animationData;

	public override Sprite[] EmotionSprites => _animationData.Frames;
}
