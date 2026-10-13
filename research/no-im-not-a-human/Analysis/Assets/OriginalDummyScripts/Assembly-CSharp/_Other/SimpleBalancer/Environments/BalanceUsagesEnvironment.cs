using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Other.SimpleBalancer.Environments;

public abstract class BalanceUsagesEnvironment<TE, T1> where TE : Enum
{
	private Dictionary<TE, T1[]> _type1Dictionary = new Dictionary<TE, T1[]>();

	private int _usagesCount;

	private int _usagesLimit = 2147483647;

	public void SetUsagesLimit(int usagesLimit)
	{
		_usagesLimit = usagesLimit;
	}

	public BalanceUsagesEnvironment<TE, T1> AddType1Array(TE valueType, params T1[] array)
	{
		if (_type1Dictionary.ContainsKey(valueType))
		{
			_type1Dictionary[valueType] = array;
		}
		else
		{
			_type1Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T1 GetType1Value(TE valueType)
	{
		if (_type1Dictionary.ContainsKey(valueType))
		{
			T1[] array = _type1Dictionary[valueType];
			T1 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type1");
	}
}
public abstract class BalanceUsagesEnvironment<TE, T1, T2> where TE : Enum
{
	private Dictionary<TE, T1[]> _type1Dictionary = new Dictionary<TE, T1[]>();

	private Dictionary<TE, T2[]> _type2Dictionary = new Dictionary<TE, T2[]>();

	private int _usagesCount;

	private int _usagesLimit = 2147483647;

	public void SetUsagesLimit(int usagesLimit)
	{
		_usagesLimit = usagesLimit;
	}

	public BalanceUsagesEnvironment<TE, T1, T2> AddType1Array(TE valueType, params T1[] array)
	{
		if (_type1Dictionary.ContainsKey(valueType))
		{
			_type1Dictionary[valueType] = array;
		}
		else
		{
			_type1Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T1 GetType1Value(TE valueType)
	{
		if (_type1Dictionary.ContainsKey(valueType))
		{
			T1[] array = _type1Dictionary[valueType];
			T1 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type1");
	}

	public BalanceUsagesEnvironment<TE, T1, T2> AddType2Array(TE valueType, params T2[] array)
	{
		if (_type2Dictionary.ContainsKey(valueType))
		{
			_type2Dictionary[valueType] = array;
		}
		else
		{
			_type2Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T2 GetType2Value(TE valueType)
	{
		if (_type2Dictionary.ContainsKey(valueType))
		{
			T2[] array = _type2Dictionary[valueType];
			T2 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type2");
	}
}
public abstract class BalanceUsagesEnvironment<TE, T1, T2, T3> where TE : Enum
{
	private Dictionary<TE, T1[]> _type1Dictionary = new Dictionary<TE, T1[]>();

	private Dictionary<TE, T2[]> _type2Dictionary = new Dictionary<TE, T2[]>();

	private Dictionary<TE, T3[]> _type3Dictionary = new Dictionary<TE, T3[]>();

	private int _usagesCount;

	private int _usagesLimit = 2147483647;

	public void SetUsagesLimit(int usagesLimit)
	{
		_usagesLimit = usagesLimit;
	}

	public BalanceUsagesEnvironment<TE, T1, T2, T3> AddType1Array(TE valueType, params T1[] array)
	{
		if (_type1Dictionary.ContainsKey(valueType))
		{
			_type1Dictionary[valueType] = array;
		}
		else
		{
			_type1Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T1 GetType1Value(TE valueType)
	{
		if (_type1Dictionary.ContainsKey(valueType))
		{
			T1[] array = _type1Dictionary[valueType];
			T1 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type1");
	}

	public BalanceUsagesEnvironment<TE, T1, T2, T3> AddType2Array(TE valueType, params T2[] array)
	{
		if (_type2Dictionary.ContainsKey(valueType))
		{
			_type2Dictionary[valueType] = array;
		}
		else
		{
			_type2Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T2 GetType2Value(TE valueType)
	{
		if (_type2Dictionary.ContainsKey(valueType))
		{
			T2[] array = _type2Dictionary[valueType];
			T2 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type2");
	}

	public BalanceUsagesEnvironment<TE, T1, T2, T3> AddType3Array(TE valueType, params T3[] array)
	{
		if (_type3Dictionary.ContainsKey(valueType))
		{
			_type3Dictionary[valueType] = array;
		}
		else
		{
			_type3Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T3 GetType3Value(TE valueType)
	{
		if (_type3Dictionary.ContainsKey(valueType))
		{
			T3[] array = _type3Dictionary[valueType];
			T3 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type3");
	}
}
public abstract class BalanceUsagesEnvironment<TE, T1, T2, T3, T4> where TE : Enum
{
	private Dictionary<TE, T1[]> _type1Dictionary = new Dictionary<TE, T1[]>();

	private Dictionary<TE, T2[]> _type2Dictionary = new Dictionary<TE, T2[]>();

	private Dictionary<TE, T3[]> _type3Dictionary = new Dictionary<TE, T3[]>();

	private Dictionary<TE, T4[]> _type4Dictionary = new Dictionary<TE, T4[]>();

	private int _usagesCount;

	private int _usagesLimit = 2147483647;

	public void SetUsagesLimit(int usagesLimit)
	{
		_usagesLimit = usagesLimit;
	}

	public BalanceUsagesEnvironment<TE, T1, T2, T3, T4> AddType1Array(TE valueType, params T1[] array)
	{
		if (_type1Dictionary.ContainsKey(valueType))
		{
			_type1Dictionary[valueType] = array;
		}
		else
		{
			_type1Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T1 GetType1Value(TE valueType)
	{
		if (_type1Dictionary.ContainsKey(valueType))
		{
			T1[] array = _type1Dictionary[valueType];
			T1 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type1");
	}

	public BalanceUsagesEnvironment<TE, T1, T2, T3, T4> AddType2Array(TE valueType, params T2[] array)
	{
		if (_type2Dictionary.ContainsKey(valueType))
		{
			_type2Dictionary[valueType] = array;
		}
		else
		{
			_type2Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T2 GetType2Value(TE valueType)
	{
		if (_type2Dictionary.ContainsKey(valueType))
		{
			T2[] array = _type2Dictionary[valueType];
			T2 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type2");
	}

	public BalanceUsagesEnvironment<TE, T1, T2, T3, T4> AddType3Array(TE valueType, params T3[] array)
	{
		if (_type3Dictionary.ContainsKey(valueType))
		{
			_type3Dictionary[valueType] = array;
		}
		else
		{
			_type3Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T3 GetType3Value(TE valueType)
	{
		if (_type3Dictionary.ContainsKey(valueType))
		{
			T3[] array = _type3Dictionary[valueType];
			T3 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type3");
	}

	public BalanceUsagesEnvironment<TE, T1, T2, T3, T4> AddType4Array(TE valueType, params T4[] array)
	{
		if (_type4Dictionary.ContainsKey(valueType))
		{
			_type4Dictionary[valueType] = array;
		}
		else
		{
			_type4Dictionary.Add(valueType, array);
		}
		return this;
	}

	public T4 GetType4Value(TE valueType)
	{
		if (_type4Dictionary.ContainsKey(valueType))
		{
			T4[] array = _type4Dictionary[valueType];
			T4 result = array[Mathf.Clamp(_usagesCount, 0, array.Length - 1)];
			if (_usagesCount < _usagesLimit)
			{
				_usagesCount++;
			}
			return result;
		}
		throw new KeyNotFoundException($"Can't find {valueType} value in Type4");
	}
}
