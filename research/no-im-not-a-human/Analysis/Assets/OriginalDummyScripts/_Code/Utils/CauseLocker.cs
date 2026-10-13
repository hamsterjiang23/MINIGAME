using System;
using System.Collections.Generic;

namespace _Code.Utils
{
	public sealed class CauseLocker<TCause> where TCause : Enum
	{
		private readonly HashSet<TCause> _lockers;

		public void Lock(TCause cause)
		{
		}

		public void Unlock(TCause cause)
		{
		}

		public static implicit operator bool(CauseLocker<TCause> locker)
		{
			return false;
		}

		public string PrintCauses()
		{
			return null;
		}
	}
}
