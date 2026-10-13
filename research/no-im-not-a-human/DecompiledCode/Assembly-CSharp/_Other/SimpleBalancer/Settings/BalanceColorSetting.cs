using DG.Tweening;
using UnityEngine;
using _Code.Utils.Extensions;

namespace _Other.SimpleBalancer.Settings;

public sealed class BalanceColorSetting : ABalanceSetting<BalanceValueColor, Color>
{
	protected override BalanceValueColor MinValue { get; set; }

	protected override BalanceValueColor MaxValue { get; set; }

	public BalanceColorSetting(Color minValue, Color maxValue, Ease ease = Ease.Linear)
	{
		MinValue = new BalanceValueColor(minValue);
		MaxValue = new BalanceValueColor(maxValue);
		Easing = ease;
	}

	public override Color GetValue(float progress)
	{
		return Color.Lerp(MinValue.GetValue(progress), MaxValue.GetValue(progress), progress.Ease(Easing));
	}
}
