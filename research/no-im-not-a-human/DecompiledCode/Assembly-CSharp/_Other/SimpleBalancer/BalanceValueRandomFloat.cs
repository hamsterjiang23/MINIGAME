using UnityEngine;

namespace _Other.SimpleBalancer;

public sealed class BalanceValueRandomFloat : ABalanceValue<float>
{
	private float MaxValue { get; set; }

	private float MinValue { get; set; }

	public BalanceValueRandomFloat(float maxValue, float minValue)
	{
		MaxValue = maxValue;
		MinValue = minValue;
	}

	public override float GetValue(float time)
	{
		return Mathf.Lerp(MinValue, MaxValue, time);
	}
}
