using DG.Tweening;

namespace _Other.SimpleBalancer
{
	public abstract class ABalanceSetting<T, TA> where T : ABalanceValue<TA>
	{
		protected Ease Easing;

		protected abstract T MinValue { get; set; }

		protected abstract T MaxValue { get; set; }

		public abstract TA GetValue(float progress);
	}
}
