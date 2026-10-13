using System;
using System.Collections.Generic;

namespace DefaultNamespace.EnumEventBus
{
	public abstract class AEnumEventBus<T> where T : Enum
	{
		private readonly Dictionary<T, List<object>> _subscribers;

		public void SubscribeAction(T eventType, Action subscriber)
		{
		}

		public void SubscribeAction<T1>(T eventType, Action<T1> subscriber)
		{
		}

		public void SubscribeFunction<T1>(T eventType, Func<T1> subscriber)
		{
		}

		public void SubscribeAction<T1, T2>(T eventType, Action<T1, T2> subscriber)
		{
		}

		public void SubscribeFunction<T1, T2>(T eventType, Func<T1, T2> subscriber)
		{
		}

		public void SubscribeAction<T1, T2, T3>(T eventType, Action<T1, T2, T3> subscriber)
		{
		}

		public void SubscribeFunction<T1, T2, T3>(T eventType, Func<T1, T2, T3> subscriber)
		{
		}

		public void SubscribeAction<T1, T2, T3, T4>(T eventType, Action<T1, T2, T3, T4> subscriber)
		{
		}

		public void SubscribeFunction<T1, T2, T3, T4>(T eventType, Func<T1, T2, T3, T4> subscriber)
		{
		}

		private void BaseSubscribe(T eventType, object subscriber)
		{
		}

		public void UnsubscribeAll(T eventType, object subscriber)
		{
		}

		public void Invoke(T eventType, params object[] arguments)
		{
		}

		public object Invoke<T1>(T eventType, params object[] arguments)
		{
			return null;
		}

		public object Invoke<T1, T2>(T eventType, params object[] arguments)
		{
			return null;
		}

		public object Invoke<T1, T2, T3>(T eventType, params object[] arguments)
		{
			return null;
		}

		public object Invoke<T1, T2, T3, T4>(T eventType, params object[] arguments)
		{
			return null;
		}
	}
}
