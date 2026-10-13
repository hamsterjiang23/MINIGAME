using System;
using System.Collections.Generic;

namespace _Code.Characters;

[Serializable]
public sealed class CharacterEmotionsArray
{
	private List<ACharacterSpriteByEmotion> _emotions = new List<ACharacterSpriteByEmotion>();

	public IReadOnlyList<ACharacterSpriteByEmotion> Emotions => _emotions;

	public void AddBase()
	{
		_emotions.Add(new CharacterBaseSpriteByEmotionData());
	}

	public void AddLocalized()
	{
		_emotions.Add(new CharacterLocalizedSpriteByEmotionData());
	}

	public void AddAnimated()
	{
		_emotions.Add(new CharacterAnimatedSpriteByEmotion());
	}

	public void RemoveEmotion(EDialogEmotionState emotionState)
	{
		_emotions.RemoveAll((ACharacterSpriteByEmotion emotion) => emotion.Emotion == emotionState);
	}
}
