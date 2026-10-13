using System;
using UnityEngine;
using _Code.Menues.HUD.Animations;

namespace _Code.Utils.UI.ImageAnimating
{
	[Serializable]
	public sealed class AnimationData
	{
		[field: SerializeField]
		public EAnimationCyclingType CyclingType { get; private set; }

		[field: SerializeField]
		public Sprite[] Frames { get; private set; }

		[field: SerializeField]
		public int FramesPerSecond { get; private set; }

		public float Duration => 0f;

		public AnimationData(EAnimationCyclingType cyclingType = EAnimationCyclingType.PlayOnce, Sprite[] frames = null, int framesPerSecond = 8)
		{
		}
	}
}
