using System;
using System.Collections.Generic;
using Code.Utils;

namespace _Code.Utils;

public sealed class CauseLocker<TCause> where TCause : Enum
{
	private readonly HashSet<TCause> _lockers = new HashSet<TCause>();

	public void Lock(TCause cause)
	{
		_lockers.Add(cause);
	}

	public void Unlock(TCause cause)
	{
		if (_lockers.Contains(cause))
		{
			_lockers.Remove(cause);
		}
	}

	public static implicit operator bool(CauseLocker<TCause> locker)
	{
		return locker._lockers.Count == 0;
	}

	public string PrintCauses()
	{
		return _lockers.ElementsToString();
	}
}
