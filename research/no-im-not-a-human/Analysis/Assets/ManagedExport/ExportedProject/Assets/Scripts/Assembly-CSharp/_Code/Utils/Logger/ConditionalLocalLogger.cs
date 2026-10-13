using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Code.Utils.Logger
{
	public sealed class ConditionalLocalLogger
	{
		private Func<bool> _condition = () => true;

		private Queue<object> _messageQueue = new Queue<object>();

		public void InitCondition(Func<bool> condition, bool runConditionAwaiter = false)
		{
		}

		public void Log(object message)
		{
		}

		private void CheckQueue()
		{
			while (_messageQueue.Count > 0)
			{
				Debug.Log(_messageQueue.Dequeue());
			}
		}

		private async UniTaskVoid ConditionAwaiter()
		{
			await UniTask.WaitUntil(() => _condition());
			CheckQueue();
		}
	}
}
