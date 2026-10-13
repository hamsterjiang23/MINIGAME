using DG.Tweening;
using UnityEngine;
using _Code.Utils.Extensions;

namespace _Other.SimpleBalancer
{
	public sealed class BalanceRandomFloatSetting : ABalanceSetting<BalanceValueRandomFloat, float>
	{
		protected override BalanceValueRandomFloat MinValue { get; set; }

		protected override BalanceValueRandomFloat MaxValue { get; set; }

		public BalanceRandomFloatSetting((float min, float max) startValues, (float min, float max) endValues, Ease ease = Ease.Linear)
		{
			MinValue = new BalanceValueRandomFloat(startValues.min, startValues.max);
			MaxValue = new BalanceValueRandomFloat(endValues.min, endValues.max);
			Easing = ease;
		}

		public override float GetValue(float progress)
		{
			return Random.Range(MinValue.GetValue(progress.Ease(Easing)), MaxValue.GetValue(progress.Ease(Easing)));
		}
	}
}
