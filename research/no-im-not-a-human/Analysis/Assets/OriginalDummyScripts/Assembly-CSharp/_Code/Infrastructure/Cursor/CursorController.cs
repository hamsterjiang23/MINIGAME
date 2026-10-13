using System;
using System.Linq;
using UnityEngine;
using Zenject;

namespace _Code.Infrastructure.Cursor;

public sealed class CursorController : ICursorController, ITickable
{
	private CursorSOData _data;

	private ECursorType _currentType;

	private bool _isLocked;

	private bool _canGoThrough;

	public event Action Locked;

	public event Action<bool> Unlocked;

	public CursorController(ICursorDataProvider dataProvider)
	{
		_data = dataProvider.CursorData;
		UnityEngine.Cursor.lockState = CursorLockMode.Confined;
	}

	public void Lock()
	{
		_isLocked = true;
		Locked?.Invoke();
	}

	public void Unlock()
	{
		_isLocked = false;
		Unlocked?.Invoke(_canGoThrough);
	}

	public void SetThrough(bool isThrough)
	{
		_canGoThrough = isThrough;
		if (isThrough)
		{
			UnityEngine.Cursor.lockState = CursorLockMode.None;
		}
		else
		{
			UnityEngine.Cursor.lockState = ((!_isLocked) ? CursorLockMode.Locked : CursorLockMode.Confined);
		}
	}

	public void SetType(ECursorType type)
	{
		if (type != _currentType)
		{
			_currentType = type;
			CursorTextureByTypeData cursorTextureByTypeData = _data.TextureByType.FirstOrDefault((CursorTextureByTypeData x) => x.Type == type);
			if (cursorTextureByTypeData != null)
			{
				UnityEngine.Cursor.SetCursor(cursorTextureByTypeData.Sprite, Vector2.zero, CursorMode.Auto);
			}
		}
	}

	public void Reset()
	{
	}

	public void Tick()
	{
	}
}
