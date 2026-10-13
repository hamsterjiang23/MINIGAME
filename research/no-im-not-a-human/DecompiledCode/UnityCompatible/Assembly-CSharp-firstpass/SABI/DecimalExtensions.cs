using System;

namespace SABI
{
	public static class DecimalExtensions
	{
		public static decimal TruncateTo(this decimal n, uint digits)
		{
			decimal num = decimal.Truncate(n);
			decimal num2 = n - num;
			double num3 = Math.Pow(10.0, digits);
			decimal num4 = Math.Truncate(num2 * (decimal)num3) / (decimal)num3;
			return num + num4;
		}
	}
}
