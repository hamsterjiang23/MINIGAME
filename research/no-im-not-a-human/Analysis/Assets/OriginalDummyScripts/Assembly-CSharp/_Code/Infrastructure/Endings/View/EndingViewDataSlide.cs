using System;
using UnityEngine;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Infrastructure.Endings.View;

[Serializable]
public sealed class EndingViewDataSlide
{
	[SerializeField]
	private AnimationData _animationData;

	[SerializeField]
	private string _textNodeName;

	[SerializeField]
	private EndingSoundData _sound;

	public AnimationData AnimationData => _animationData;

	public string TextNodeName => _textNodeName;

	public EndingSoundData Sound => _sound;
}
