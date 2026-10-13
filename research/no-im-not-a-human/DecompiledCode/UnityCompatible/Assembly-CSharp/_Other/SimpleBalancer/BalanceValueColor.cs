using UnityEngine;

namespace _Other.SimpleBalancer
{
	public sealed class BalanceValueColor : ABalanceValue<Color>
	{
		private readonly Color _color;

		public BalanceValueColor(Color color)
		{
			_color = color;
		}

		public override Color GetValue(float time)
		{
			return _color;
		}
	}
}
