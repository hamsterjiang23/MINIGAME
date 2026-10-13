using System;
using UnityEngine;
using _Code.Utils.UI.ImageAnimating;

namespace _Code.Menues.HUD.Animations
{
	[Serializable]
	public sealed class HUDAnimationData
	{
		[field: SerializeField]
		public EHUDAnimation AnimationType { get; private set; }

		[field: SerializeField]
		public AnimationData AnimationData { get; private set; }
	}
}
