namespace _Other.SimpleBalancer
{
	public sealed class BalanceValueRandomFloat : ABalanceValue<float>
	{
		private float MaxValue { get; set; }

		private float MinValue { get; set; }

		public BalanceValueRandomFloat(float maxValue, float minValue)
		{
		}

		public override float GetValue(float time)
		{
			return 0f;
		}
	}
}
