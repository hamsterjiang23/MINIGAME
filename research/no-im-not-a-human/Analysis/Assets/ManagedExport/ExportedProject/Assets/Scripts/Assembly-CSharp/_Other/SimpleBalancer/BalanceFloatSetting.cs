using DG.Tweening;
using UnityEngine;
using _Code.Utils.Extensions;

namespace _Other.SimpleBalancer
{
	public sealed class BalanceFloatSetting : ABalanceSetting<BalanceValueFloat, float>
	{
		protected override BalanceValueFloat MinValue { get; set; }

		protected override BalanceValueFloat MaxValue { get; set; }

		public BalanceFloatSetting(float minValue, float maxValue, Ease easing = Ease.Linear)
		{
			MinValue = new BalanceValueFloat(minValue);
			MaxValue = new BalanceValueFloat(maxValue);
			Easing = easing;
		}

		public override float GetValue(float progress)
		{
			return Mathf.Lerp(MinValue.GetValue(progress), MaxValue.GetValue(progress), progress.Ease(Easing));
		}
	}
}
