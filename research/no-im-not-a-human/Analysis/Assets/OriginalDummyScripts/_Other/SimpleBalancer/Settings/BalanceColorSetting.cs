using DG.Tweening;
using UnityEngine;

namespace _Other.SimpleBalancer.Settings
{
	public sealed class BalanceColorSetting : ABalanceSetting<BalanceValueColor, Color>
	{
		protected override BalanceValueColor MinValue { get; set; }

		protected override BalanceValueColor MaxValue { get; set; }

		public BalanceColorSetting(Color minValue, Color maxValue, Ease ease = Ease.Linear)
		{
		}

		public override Color GetValue(float progress)
		{
			return default;
		}
	}
}
