using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.EnumEventBus;

public abstract class AEnumEventBus<T> where T : Enum
{
	private readonly Dictionary<T, List<object>> _subscribers = new Dictionary<T, List<object>>();

	public void SubscribeAction(T eventType, Action subscriber)
	{
		BaseSubscribe(eventType, subscriber);
	}

	public void SubscribeAction<T1>(T eventType, Action<T1> subscriber)
	{
		BaseSubscribe(eventType, subscriber);
	}

	public void SubscribeFunction<T1>(T eventType, Func<T1> subscriber)
	{
		BaseSubscribe(eventType, subscriber);
	}

	public void SubscribeAction<T1, T2>(T eventType, Action<T1, T2> subscriber)
	{
		BaseSubscribe(eventType, subscriber);
	}

	public void SubscribeFunction<T1, T2>(T eventType, Func<T1, T2> subscriber)
	{
		BaseSubscribe(eventType, subscriber);
	}

	public void SubscribeAction<T1, T2, T3>(T eventType, Action<T1, T2, T3> subscriber)
	{
		BaseSubscribe(eventType, subscriber);
	}

	public void SubscribeFunction<T1, T2, T3>(T eventType, Func<T1, T2, T3> subscriber)
	{
		BaseSubscribe(eventType, subscriber);
	}

	public void SubscribeAction<T1, T2, T3, T4>(T eventType, Action<T1, T2, T3, T4> subscriber)
	{
		BaseSubscribe(eventType, subscriber);
	}

	public void SubscribeFunction<T1, T2, T3, T4>(T eventType, Func<T1, T2, T3, T4> subscriber)
	{
		BaseSubscribe(eventType, subscriber);
	}

	private void BaseSubscribe(T eventType, object subscriber)
	{
		if (!_subscribers.ContainsKey(eventType))
		{
			_subscribers.Add(eventType, new List<object>());
		}
		Dictionary<T, List<object>> subscribers = _subscribers;
		if (subscribers[eventType] == null)
		{
			List<object> list = (subscribers[eventType] = new List<object>());
		}
		_subscribers[eventType].Add(subscriber);
	}

	public void UnsubscribeAll(T eventType, object subscriber)
	{
		if (_subscribers.ContainsKey(eventType) && _subscribers[eventType].Contains(subscriber))
		{
			_subscribers[eventType].Clear();
		}
	}

	public void Invoke(T eventType, params object[] arguments)
	{
		if (!_subscribers.ContainsKey(eventType))
		{
			return;
		}
		foreach (object item in _subscribers[eventType])
		{
			if (item is Action action)
			{
				action();
			}
		}
	}

	public object Invoke<T1>(T eventType, params object[] arguments)
	{
		Debug.Log("INVOKED");
		if (!_subscribers.ContainsKey(eventType))
		{
			return null;
		}
		foreach (object item in _subscribers[eventType])
		{
			if (item is Action<T1> action)
			{
				action((arguments[0] is T1) ? ((T1)arguments[0]) : default(T1));
			}
			if (item is Func<T1> func)
			{
				return func();
			}
		}
		return null;
	}

	public object Invoke<T1, T2>(T eventType, params object[] arguments)
	{
		if (!_subscribers.ContainsKey(eventType))
		{
			return null;
		}
		foreach (object item in _subscribers[eventType])
		{
			if (item is Action<T1, T2> action)
			{
				action((arguments[0] is T1) ? ((T1)arguments[0]) : default(T1), (arguments[1] is T2) ? ((T2)arguments[1]) : default(T2));
			}
			if (item is Func<T1, T2> func)
			{
				return func((arguments[0] is T1) ? ((T1)arguments[0]) : default(T1));
			}
		}
		return null;
	}

	public object Invoke<T1, T2, T3>(T eventType, params object[] arguments)
	{
		if (!_subscribers.ContainsKey(eventType))
		{
			return null;
		}
		foreach (object item in _subscribers[eventType])
		{
			if (item is Action<T1, T2, T3> action)
			{
				action((arguments[0] is T1) ? ((T1)arguments[0]) : default(T1), (arguments[1] is T2) ? ((T2)arguments[1]) : default(T2), (arguments[2] is T3) ? ((T3)arguments[2]) : default(T3));
			}
			if (item is Func<T1, T2, T3> func)
			{
				return func((arguments[0] is T1) ? ((T1)arguments[0]) : default(T1), (arguments[1] is T2) ? ((T2)arguments[1]) : default(T2));
			}
		}
		return null;
	}

	public object Invoke<T1, T2, T3, T4>(T eventType, params object[] arguments)
	{
		if (!_subscribers.ContainsKey(eventType))
		{
			return null;
		}
		foreach (object item in _subscribers[eventType])
		{
			if (item is Action<T1, T2, T3, T4> action)
			{
				action((arguments[0] is T1) ? ((T1)arguments[0]) : default(T1), (arguments[1] is T2) ? ((T2)arguments[1]) : default(T2), (arguments[2] is T3) ? ((T3)arguments[2]) : default(T3), (arguments[3] is T4) ? ((T4)arguments[3]) : default(T4));
			}
			if (item is Func<T1, T2, T3, T4> func)
			{
				return func((arguments[0] is T1) ? ((T1)arguments[0]) : default(T1), (arguments[1] is T2) ? ((T2)arguments[1]) : default(T2), (arguments[2] is T3) ? ((T3)arguments[2]) : default(T3));
			}
		}
		return null;
	}
}
