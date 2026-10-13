namespace _Other.SimpleBalancer;

public sealed class BalanceValueFloat : ABalanceValue<float>
{
	private float Value { get; set; }

	public BalanceValueFloat(float value)
	{
		Value = value;
	}

	public override float GetValue(float time)
	{
		return Value;
	}
}
