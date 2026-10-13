using DG.Tweening;

namespace _Other.SimpleBalancer
{
	public sealed class BalanceFloatSetting : ABalanceSetting<BalanceValueFloat, float>
	{
		protected override BalanceValueFloat MinValue { get; set; }

		protected override BalanceValueFloat MaxValue { get; set; }

		public BalanceFloatSetting(float minValue, float maxValue, Ease easing = Ease.Linear)
		{
		}

		public override float GetValue(float progress)
		{
			return 0f;
		}
	}
}
