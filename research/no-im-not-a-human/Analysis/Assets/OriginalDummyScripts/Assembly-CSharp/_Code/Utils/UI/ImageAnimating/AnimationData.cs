using System;
using UnityEngine;
using _Code.Menues.HUD.Animations;

namespace _Code.Utils.UI.ImageAnimating;

[Serializable]
public sealed class AnimationData
{
	[field: SerializeField]
	public EAnimationCyclingType CyclingType { get; private set; }

	[field: SerializeField]
	public Sprite[] Frames { get; private set; }

	[field: SerializeField]
	public int FramesPerSecond { get; private set; } = 12;

	public float Duration => (float)Frames.Length / (float)FramesPerSecond;

	public AnimationData(EAnimationCyclingType cyclingType = EAnimationCyclingType.PlayOnce, Sprite[] frames = null, int framesPerSecond = 8)
	{
		CyclingType = cyclingType;
		FramesPerSecond = framesPerSecond;
		Frames = frames ?? Array.Empty<Sprite>();
	}
}
