using System;
using Cysharp.Threading.Tasks;

namespace _Code.Utils
{
	public static class BetterUniTask
	{
		public static async UniTaskVoid InvokeDelayed(Action action, float delay)
		{
			await UniTask.Delay(TimeSpan.FromSeconds(delay));
			action();
		}

		public static async UniTaskVoid InvokeAfter(Action action, Func<bool> condition)
		{
			await UniTask.WaitUntil(condition);
			action();
		}
	}
}
