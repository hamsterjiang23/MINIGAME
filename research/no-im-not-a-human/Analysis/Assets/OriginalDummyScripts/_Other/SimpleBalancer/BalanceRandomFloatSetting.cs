using DG.Tweening;

namespace _Other.SimpleBalancer
{
	public sealed class BalanceRandomFloatSetting : ABalanceSetting<BalanceValueRandomFloat, float>
	{
		protected override BalanceValueRandomFloat MinValue { get; set; }

		protected override BalanceValueRandomFloat MaxValue { get; set; }

		public BalanceRandomFloatSetting((float min, float max) startValues, (float min, float max) endValues, Ease ease = Ease.Linear)
		{
		}

		public override float GetValue(float progress)
		{
			return 0f;
		}
	}
}
