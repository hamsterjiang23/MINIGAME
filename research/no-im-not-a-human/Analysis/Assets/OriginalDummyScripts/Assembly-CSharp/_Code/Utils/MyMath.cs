using System;

namespace _Code.Utils;

public static class MyMath
{
	public static long Fibonacci(int n)
	{
		if (n < 0)
		{
			throw new ArgumentOutOfRangeException("n", "n must be non-negative.");
		}
		switch (n)
		{
		case 0:
			return 0L;
		case 1:
			return 1L;
		default:
		{
			long num = 0L;
			long num2 = 1L;
			for (int i = 2; i <= n; i++)
			{
				long num3 = num + num2;
				num = num2;
				num2 = num3;
			}
			return num2;
		}
		}
	}
}
